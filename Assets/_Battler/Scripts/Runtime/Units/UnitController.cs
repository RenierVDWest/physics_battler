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
        public PosePipeline Pose { get; private set; }
        public AuthorityController Authority { get; private set; }
        public Loadout Loadout { get; private set; }
        public Weapon MainWeapon { get; private set; }
        public Weapon OffWeapon { get; private set; }

        /// <summary>The blow under way, or null.</summary>
        public MoveState CurrentMove { get; private set; }
        public bool IsAttacking => Clip.Striking;

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

            Initialize();
        }

        private bool Initialize()
        {
            physical = true;
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

            // (Moved by the lab's clip fighter: the plan "Bringing the lab into the fight", step 4.)
            return InitializeClips(true);
        }

        private void OnDestroy()
        {
            all.Remove(this);
            Clip?.DestroyGetUpRoot();
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

        // ---- Equipment -------------------------------------------------------------------------

        /// <summary>Puts weapons in the hands: a main weapon (right hand) and an off-hand item (left), either may be null.</summary>
        public void Equip(WeaponDefinition main, WeaponDefinition off)
        {
            EquipClips(Rig.IsSevered(PartId.HandR) ? null : main, Rig.IsSevered(PartId.HandL) ? null : off);
            return;
        }

        /// <summary>
        /// The shield's turn in the hand (centre grip): its face (forward) along the forearm, away from
        /// the elbow; its bar (its up) through the fist along the knuckles, as a sword's grip is. With
        /// the forearm raised the thumb is on top and the wrist straight (the bar stands upright).
        /// </summary>
        public static readonly Quaternion ShieldGrip = Quaternion.LookRotation(Vector3.down, Vector3.forward);

        /// <summary>A weapon's turn in the hand: the hammer grip, its blade leaning <see cref="PosePipeline.GripLean"/> toward the fingers.</summary>
        public static readonly Quaternion MainGrip = Quaternion.Euler(PosePipeline.GripLean, 0f, 0f);

        // ---- Gameplay ----------------------------------------------------------------------------

        /// <summary>Face a point on the board (the unit turns at its turn speed, stepping as it goes).</summary>
        public void FacePoint(Vector3 point) => facePoint = point;

        /// <summary>Moves the unit somewhere else at once, body and all (the sandbox's scenarios).</summary>
        public void Teleport(Vector3 position, float yaw)
        {
            // (A clip fighter: its walk placed there, its body carried with it.)
            Vector3 from = Clip.Loco.Position;
            Clip.Loco.Place(new Vector3(position.x, Stance.GroundY, position.z), yaw);
            Clip.CarryBy(Clip.Loco.Position - from);
            transform.SetPositionAndRotation(Clip.Loco.Position, Quaternion.Euler(0f, yaw, 0f));
            facingYaw = yaw;
            facePoint = null;
            ClipWant = Vector3.zero;
            return;
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
            => Clearance(state, samples, float.MinValue, out closest, out when, out weapon);

        /// <param name="enough">Stop at the first pose clearer than nothing below this (the planner only asks whether a blow passes): float.MinValue to measure them all.</param>
        public float Clearance(MoveState state, int samples, float enough, out PartId closest, out float when, out bool weapon)
        {
            using (Perf.Begin(PerfSection.Clearance))
            {
                return ClearanceCore(state, samples, enough, out closest, out when, out weapon);
            }
        }

        private float ClearanceCore(MoveState state, int samples, float enough, out PartId closest, out float when, out bool weapon)
        {
            float best = float.MaxValue;
            closest = PartId.Chest;
            when = 0f;
            weapon = false;
            for (int i = 0; i < samples; i++)
            {
                float c = ClearanceAt(state, i, samples, out var part, out bool blade);
                if (c < best)
                {
                    best = c;
                    closest = part;
                    when = ClearanceTime(i, samples);
                    weapon = blade;
                }

                if (best < enough)
                {
                    break;   // (it fails: no need to see the rest)
                }
            }

            return best;
        }

        /// <summary>The normalized time of the <paramref name="i"/>th of <paramref name="samples"/> clearance poses.</summary>
        public static float ClearanceTime(int i, int samples) => Mathf.Lerp(0.04f, 0.96f, i / (float)Mathf.Max(1, samples - 1));

        /// <summary>
        /// One of a blow's clearance poses (the <paramref name="i"/>th of <paramref name="samples"/>):
        /// how clear of its own body the weapon and weapon arm are there, everything put back after.
        /// (The performance pass: a hand-keyed blow's striking part is kept at each pose, from where
        /// the body stood, so the posed path the blow lays as it starts takes these instead of posing
        /// them again; and the planner can take them a few a step.)
        /// </summary>
        public float ClearanceAt(MoveState state, int i, int samples, out PartId part, out bool blade)
        {
            bool keep = state.Generated && state.HasContactHand;
            if (keep && (i == 0 || state.PosedTimes == null || state.PosedTimes.Length != samples))
            {
                state.PosedTimes = new float[samples];
                state.PosedStrikes = new Vector3[samples];
                state.PosedCount = 0;
            }

            float t = ClearanceTime(i, samples);
            float length = state.Weapon != null ? state.Weapon.length * 0.98f : 0.8f;
            var saved = Stance.Save();
            var anim = SaveAnim();
            var rotation = transform.rotation;
            try
            {
                PoseDry(state, t, true);
                if (keep && state.PosedCount == i)
                {
                    state.PosedTimes[i] = t;
                    state.PosedStrikes[i] = Pose.StrikePoint - transform.position;
                    state.PosedCount = i + 1;
                }

                return BodyClearance.Measure(Rig, Anim, length, Loadout.Scale, out part, out blade);
            }
            finally
            {
                transform.rotation = rotation;
                Stance.Restore(saved);
                RestoreAnim(anim);
            }
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
                StepClips(Time.fixedDeltaTime);
                return;
            }
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
