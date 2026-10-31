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
    public class UnitController : MonoBehaviour
    {
        [SerializeField] private UnitProfile profile;
        [SerializeField] private int team;

        private static readonly List<UnitController> all = new List<UnitController>();
        private static Transform bodiesContainer;
        private static int spawnCount;   // seeds each unit's idle and shuffles differently
        private GameObject detachedBody;
        private float facingYaw;
        private Vector3? facePoint;
        private bool initialized;
        private bool physical;

        public static event Action<UnitController> Initialized;
        public static event Action<UnitController, MoveState> MoveStarted;
        public static event Action<UnitController, MoveState, ContactReport> MoveContact;
        public static event Action<UnitController, MoveState> MoveEnded;
        public static event Action<UnitController, Weapon, BodyPart, float> WeaponTouched;

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
                    if (part.joint != null)
                    {
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
            }

            int seed = 7919 * team + 104729 * ++spawnCount;
            Stance = new Stance(seed);
            Stance.Configure(Style, Body);
            Idle = new CombatIdle(seed + 1);
            Pose = new PosePipeline(transform, Anim, Body, Stance, Idle, Style) { Loadout = Loadout };
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

            Loadout.Main = main;
            Loadout.Off = off;
            MainWeapon = main != null ? Weapon.Equip(main, this, Rig.SlotWeaponR, true, Loadout.Scale) : null;
            OffWeapon = off != null ? Weapon.Equip(off, this, Rig.SlotWeaponL, false, Loadout.Scale) : null;
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

        private static void DestroyWeapon(Weapon weapon)
        {
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
        public bool StartMove(MoveState state)
        {
            if (CurrentMove != null || state == null)
            {
                return false;
            }

            state.Time = 0f;
            state.ContactPassed = state.SteppedOut = state.LeadOut = state.SteppedBack = state.RearMoved = state.RearBack = false;
            state.HasLastStrike = state.HasLastPhysical = false;
            state.PhysicalClosest = -1f;
            CurrentMove = state;
            Pose.Move = state;
            MainWeapon?.ResetTouches();
            OffWeapon?.ResetTouches();
            Stance.Hold = true;
            MoveStarted?.Invoke(this, state);
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
            var saved = Stance.Save();
            var anim = SaveAnim();
            var previousMove = Pose.Move;
            var rotation = transform.rotation;
            try
            {
                // At contact the unit faces along the aim frame, with its lead foot planted at the end of the lunge.
                float yaw = Quaternion.LookRotation(state.Frame * Vector3.forward).eulerAngles.y;
                transform.rotation = Quaternion.Euler(0f, yaw, 0f);
                if (NeedsLungeStep(state))
                {
                    LungeFeet(state, yaw, out var leadTo, out var rearTo, out bool rearMoves);
                    Stance.SetFoot(Stance.Lead, leadTo, yaw + Stance.HomeYawLocal(Stance.Lead));
                    if (rearMoves)
                    {
                        Stance.SetFoot(1 - Stance.Lead, rearTo, yaw + Stance.HomeYawLocal(1 - Stance.Lead));
                    }
                }

                var atContact = state.CopyAt(state.ContactTime);
                Pose.Move = atContact;
                Pose.Compose(LocalTime, 0f, true);
                return Pose.AimError;
            }
            finally
            {
                Pose.Move = previousMove;
                if (!keep)
                {
                    transform.rotation = rotation;
                    Stance.Restore(saved);
                    RestoreAnim(anim);
                }
            }
        }

        private bool NeedsLungeStep(MoveState state) => state.Lunge.magnitude > 0.06f * Body.SizeFactor;

        /// <summary>How much longer than its stance a lunge may stretch the feet (fraction of leg length).</summary>
        private const float LungeStretch = 0.3f;

        /// <summary>
        /// Where a lunge puts the feet at contact: the lead foot out by the whole lunge, and, for a
        /// long one, the rear foot following so the stance stretches by at most
        /// <see cref="LungeStretch"/> of the leg (an advance-lunge). The hips then stay high enough
        /// for the arm to reach up.
        /// </summary>
        private void LungeFeet(MoveState state, float yaw, out Vector3 leadTo, out Vector3 rearTo, out bool rearMoves)
        {
            int lead = Stance.Lead;
            Vector3 root = transform.position;
            leadTo = Stance.HomeWorld(lead, root + state.Lunge, yaw);
            float follow = state.Lunge.magnitude - LungeStretch * Body.LegLength;
            rearMoves = follow > 0.05f * Body.LegLength;
            Vector3 rearRoot = root + (rearMoves ? state.Lunge.normalized * follow : Vector3.zero);
            rearTo = Stance.HomeWorld(1 - lead, rearRoot, yaw);
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

            Step(Time.fixedDeltaTime, true);
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
            LocalTime += dt;
            var move = CurrentMove;
            if (move != null)
            {
                facePoint = move.Aim;
            }

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

            if (move != null)
            {
                TrackPhysicalWeapon(move);
                AdvanceMove(move, dt);
            }

            Pose.Compose(LocalTime, dt);

            if (move != null)
            {
                // The striking part moves several centimetres a step, so contact is judged on the
                // stretch it swept since the last step, as a hit would be.
                if (!move.ContactPassed && move.T >= move.ContactTime)
                {
                    move.ContactPassed = true;
                    MoveContact?.Invoke(this, move, Report(move));
                }

                move.LastStrike = Pose.StrikePoint;
                move.HasLastStrike = true;
            }

            if (drive)
            {
                float blend = move != null ? move.Blend : 0f;
                float armPin = Mathf.Lerp(profile.armPin, move != null ? move.Move.weaponArmPin : profile.armPin, blend);
                Authority.Set(BodyRegion.ArmR, AuthorityLevel.Pinned, armPin);
                Authority.Apply(dt);
            }

            if (move != null && move.Done)
            {
                CurrentMove = null;
                Pose.Move = null;
                Stance.Hold = false;
                MoveEnded?.Invoke(this, move);
            }
        }

        /// <summary>The physical weapon as PhysX left it: its sweep since the last step, against the aim point.</summary>
        private void TrackPhysicalWeapon(MoveState move)
        {
            if (MainWeapon == null || move.Weapon == null)
            {
                return;
            }

            Vector3 now = MainWeapon.PhysicalPoint(move.Weapon.StrikePoint(move.Move.strikeWith));
            if (move.HasLastPhysical && move.Active)
            {
                float d = SegmentDistance(move.Aim, move.LastPhysical, now);
                move.PhysicalClosest = move.PhysicalClosest < 0f ? d : Mathf.Min(move.PhysicalClosest, d);
            }

            move.LastPhysical = now;
            move.HasLastPhysical = true;
        }

        /// <summary>
        /// The lunge's planned steps: for a long lunge the rear foot moves up first, then the lead
        /// foot goes out to land just before contact; both come back in the recovery, lead first.
        /// </summary>
        private void AdvanceMove(MoveState move, float dt)
        {
            move.Time += dt;
            if (!NeedsLungeStep(move))
            {
                return;
            }

            float tc = move.ContactTime;
            int lead = Stance.Lead;
            int rear = 1 - lead;
            float yaw = Quaternion.LookRotation(move.Frame * Vector3.forward).eulerAngles.y;
            LungeFeet(move, yaw, out var leadTo, out var rearTo, out bool rearMoves);
            float windUp = tc * move.Duration;
            if (!move.SteppedOut && move.T >= 0.05f * tc)
            {
                move.SteppedOut = true;
                if (rearMoves)
                {
                    Stance.StepTo(rear, rearTo, yaw + Stance.HomeYawLocal(rear), 0.4f * windUp);
                }

                move.LeadOutAt = rearMoves ? 0.42f * tc : 0.2f * tc;
                move.RearMoved = rearMoves;
            }

            if (!move.LeadOut && move.SteppedOut && move.T >= move.LeadOutAt)
            {
                move.LeadOut = true;
                Stance.StepTo(lead, leadTo, yaw + Stance.HomeYawLocal(lead), (0.97f * tc - move.LeadOutAt) * move.Duration);
            }

            float back = tc + 0.3f * (1f - tc);
            float recovery = (1f - tc) * move.Duration;
            if (!move.SteppedBack && move.T >= back)
            {
                move.SteppedBack = true;
                Stance.StepTo(lead, Stance.HomeWorld(lead, transform.position, facingYaw), facingYaw + Stance.HomeYawLocal(lead), 0.32f * recovery);
            }

            if (move.RearMoved && !move.RearBack && move.T >= back + 0.33f * (1f - tc))
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
                AnimError = SegmentDistance(move.Aim, from, Pose.StrikePoint), PhysicalError = -1f, StruckDistance = float.MaxValue,
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
            if (entered && PlaytestTrace.Enabled && collision.collider != null && collision.relativeVelocity.magnitude > 1f)
            {
                PlaytestTrace.Note(this, "contact", $"{part.id} touched {collision.collider.name} at {collision.relativeVelocity.magnitude:0.0} m/s");
            }
        }

        /// <summary>
        /// One of this unit's weapons touched another unit's body part (milestone 2: logged, not a hit
        /// yet). True if it counted (during a blow's active window).
        /// </summary>
        public bool OnWeaponTouched(Weapon weapon, BodyPart part, Collider collider)
        {
            var move = CurrentMove;
            if (move == null || !move.Active)
            {
                return false;
            }

            var hand = Rig.Part(weapon.InMainHand ? PartId.HandR : PartId.HandL).body;
            float speed = hand != null ? hand.GetPointVelocity(collider.transform.position).magnitude : 0f;
            WeaponTouched?.Invoke(this, weapon, part, speed);
            return true;
        }
    }
}
