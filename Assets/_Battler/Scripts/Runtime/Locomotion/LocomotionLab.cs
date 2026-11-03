using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PhysicsBattler.Combat;
using PhysicsBattler.Muscles;
using PhysicsBattler.Rig;
using PhysicsBattler.Sandbox;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// The locomotion lab in the game (branch locomotion-lab): one Medium on the board, walked by the
    /// new locomotion (<see cref="Locomotor"/> and <see cref="LocoBody"/>) round the lab's course, or
    /// driven by hand. Nothing of the fighting runs: the unit's own controller is taken off, and its
    /// body parts are carried along by the posed animation rig (kinematic for now; the physical body
    /// following it is the plan's stage 5).
    ///
    /// Turned on by <see cref="BattleSandbox.locomotionLab"/>. Keys: C course or driving, R restart,
    /// P pause, comma and period the combat clip to play, J play it. Driving: W A S D (against the camera), Shift run, Space hold the facing
    /// (side-steps and walking backward), V walking speed. Each run's measures and film are written to
    /// PlaytestLogs/locolab-(time)/ when Play stops.
    /// </summary>
    [DefaultExecutionOrder(-140)]
    public class LocomotionLab : MonoBehaviour
    {
        public UnitController prefab;
        public UnitController smallPrefab, mediumPrefab, largePrefab;

        [Tooltip("Which body walks (N: the next one).")]
        public BodySize body = BodySize.Medium;
        public OrbitCamera orbitCamera;
        public Material tint;

        [Tooltip("Walk the lab's course (off: drive it with W A S D).")]
        public bool course = true;

        [Tooltip("The body is physical (B): the legs carried exactly, so the feet stay planted; the torso, head and arms held by pins, swaying with the walk and pushed about by what hits them. Off: every part carried kinematically.")]
        public bool physical = true;

        [Tooltip("How hard a shove (G) is: the change in the body's speed (m/s).")]
        public float shove = 1.3f;

        [Tooltip("Show the footprints planted feet leave.")]
        public bool footprints = true;

        private GameObject unit;
        private UnitRig rig;
        private AnimRig anim;
        private Locomotor loco;
        private LocoBody poser;
        private LocoMeasures measures;
        private LocoCourse path;
        private LocoFilm film;
        private Authority.AuthorityController authority;
        private bool snapped;
        private int shoves;
        private bool holding;
        private readonly System.Random dice = new System.Random();
        private float time;
        private string leg = "";
        private bool paused, stepOnce;
        private float walkSpeed = 1.4f;
        private Vector3 origin;
        private readonly List<string> legs = new List<string>();
        private Rect panel = new Rect(12, 12, 330, 10);
        private string folder;

        public const float BaseRunSpeed = 3.6f;

        /// <summary>A Medium's speeds, for this body: its natural pace goes with the square root of its leg.</summary>
        private float PaceScale => loco != null ? Mathf.Sqrt(loco.Leg / 0.845f) : 1f;

        private float RunSpeed => BaseRunSpeed * PaceScale;

        // ---- What it carries (the join's step 1: walking with weapons) -----------------------------------

        /// <summary>The loadouts it can carry: bare hands, and each one a unit marker in the scene holds.</summary>
        private readonly List<(string name, WeaponDefinition main, WeaponDefinition off)> loadouts = new List<(string, WeaponDefinition, WeaponDefinition)>();

        [Tooltip("Which loadout it carries (L: the next one); -1: the body's own, as its marker in the scene has it.")]
        public int loadout = -1;

        private int loadoutNow;

        private void FindLoadouts()
        {
            loadouts.Clear();
            loadouts.Add(("bare hands", null, null));
            foreach (var spawn in FindObjectsByType<UnitSpawn>())
            {
                if (spawn.mainHand == null && spawn.offHand == null)
                {
                    continue;
                }

                string name = (spawn.mainHand != null ? spawn.mainHand.name : "bare") + (spawn.offHand != null ? " + " + spawn.offHand.name : "");
                if (!loadouts.Exists(l => l.main == spawn.mainHand && l.off == spawn.offHand))
                {
                    loadouts.Add((name, spawn.mainHand, spawn.offHand));
                }
            }
        }

        private int OwnLoadout()
        {
            // (The body's own: what its marker holds, the first team's first.)
            UnitSpawn own = null;
            foreach (var spawn in FindObjectsByType<UnitSpawn>())
            {
                if (spawn.body == body && (own == null || spawn.team < own.team))
                {
                    own = spawn;
                }
            }

            int i = own != null ? loadouts.FindIndex(l => l.main == own.mainHand && l.off == own.offHand) : 0;
            return Mathf.Max(0, i);
        }

        /// <summary>The loadout's weapons put in the hands (just their look: they hit nothing here), and the walk told what each hand carries.</summary>
        private void EquipLoadout(float scale)
        {
            if (loadouts.Count == 0)
            {
                FindLoadouts();
            }

            loadoutNow = loadout >= 0 && loadout < loadouts.Count ? loadout : OwnLoadout();
            var (_, main, off) = loadouts[loadoutNow];
            Hold(rig.SlotWeaponR, anim.SlotWeaponR, main, main != null && !main.IsShield ? UnitController.MainGrip : Quaternion.identity, scale);
            Hold(rig.SlotWeaponL, anim.SlotWeaponL, off, off != null && off.IsShield ? UnitController.ShieldGrip : Quaternion.identity, scale);
            poser.CarryR = main == null ? LocoBody.Carry.Free : main.IsShield ? LocoBody.Carry.Shield : LocoBody.Carry.Weapon;
            poser.CarryL = off == null ? LocoBody.Carry.Free : off.IsShield ? LocoBody.Carry.Shield : LocoBody.Carry.Weapon;
            poser.BladeR = (anim.SlotWeaponR != null ? anim.SlotWeaponR.localRotation : Quaternion.identity) * Vector3.forward;
            poser.BladeL = (anim.SlotWeaponL != null ? anim.SlotWeaponL.localRotation : Quaternion.identity) * Vector3.forward;
            film.BladeLength = main != null && !main.IsShield ? main.length * scale : 0f;
            poser.WeaponLength = main != null && !main.IsShield ? main.length * scale : 0.9f * scale;
            mainWeapon = main != null && !main.IsShield ? main : null;
            poser.BodyKey = body.ToString();
            poser.TwoHanded = LocoLibrary.TwoHanded(body.ToString());
            film.ShieldRadius = off != null && off.IsShield ? 0.3f * scale : 0f;
        }

        private void Hold(Transform slot, Transform animSlot, WeaponDefinition definition, Quaternion grip, float scale)
        {
            if (slot == null)
            {
                return;
            }

            slot.localRotation = grip;
            if (animSlot != null)
            {
                animSlot.localRotation = grip;
            }

            if (definition == null || definition.prefab == null)
            {
                return;
            }

            var held = Instantiate(definition.prefab, slot);
            held.transform.localPosition = Vector3.zero;
            held.transform.localRotation = Quaternion.identity;
            held.transform.localScale = Vector3.one * scale;
            foreach (var behaviour in held.GetComponentsInChildren<MonoBehaviour>(true))
            {
                Destroy(behaviour);
            }

            foreach (var collider in held.GetComponentsInChildren<Collider>(true))
            {
                collider.isTrigger = true;
                foreach (var part in rig.Parts)
                {
                    if (part.partCollider != null)
                    {
                        Physics.IgnoreCollision(collider, part.partCollider, true);
                    }
                }
            }
        }

        public void Begin(UnitController small, UnitController medium, UnitController large, OrbitCamera cam, Material material, Vector3 boardOrigin)
        {
            smallPrefab = small;
            mediumPrefab = medium;
            largePrefab = large;
            orbitCamera = cam;
            tint = material;
            origin = boardOrigin;
            Build();
        }

        private void Build() => Build(null, 0f);

        /// <param name="standAt">Stand up here (after a knockdown: the same run goes on), not at the start.</param>
        private void Build(Vector3? standAt, float standFacing)
        {
            // (Each run is logged on its own: a restart or a switch writes the run before it.)
            if (!standAt.HasValue)
            {
                WriteLog();
                written = false;
                legs.Clear();
            }

            if (unit != null)
            {
                Destroy(unit);
            }

            LocoLibrary.Load();
            prefab = body == BodySize.Small ? smallPrefab : body == BodySize.Large ? largePrefab : mediumPrefab;
            if (prefab == null)
            {
                prefab = mediumPrefab;
            }

            if (prefab == null)
            {
                Debug.LogError("[LocomotionLab] no unit prefab: run Physics Battler > Setup > Run Project Setup.");
                enabled = false;
                return;
            }

            // Made under an inactive holder, its own controller taken off before it wakes.
            var holder = new GameObject("Spawning");
            holder.SetActive(false);
            var made = Instantiate(prefab, origin, Quaternion.identity, holder.transform);
            made.name = "Locomotion lab Medium";
            BattleSandbox.Tint(made, tint);
            unit = made.gameObject;
            for (int pass = 0; pass < 3; pass++)
            {
                // (Latest first: a component another one needs was usually added before it.)
                var behaviours = unit.GetComponentsInChildren<MonoBehaviour>(true);
                for (int i = behaviours.Length - 1; i >= 0; i--)
                {
                    var behaviour = behaviours[i];
                    if (behaviour != null && !(behaviour is UnitRig) && !(behaviour is BodyPart))
                    {
                        DestroyImmediate(behaviour);
                    }
                }
            }

            rig = unit.GetComponent<UnitRig>();
            unit.transform.SetParent(null, true);
            Destroy(holder);

            // The physical body set up as a unit sets up its own (its joints in their frames, its
            // limits, its friction), while it is still in its rest pose; the animation rig is copied
            // then too. The parts then leave the hierarchy, each its own body.
            var profile = prefab.Profile != null ? prefab.Profile : UnitProfile.CreateDefault();
            if (physical)
            {
                JointConventions.EnsureProbed();
                var bodyMaterial = PhysicsMaterials.Get("Body", profile.bodyFriction);
                var footMaterial = PhysicsMaterials.Get("Foot", profile.footFriction);
                foreach (var part in rig.Parts)
                {
                    UnitBuilder.ConfigureBody(part.body, profile);
                    part.limits = JointLimitTable.Physical(part.id);
                    if (part.joint != null)
                    {
                        part.jointFrame = JointFrames.For(rig, part.id);
                        JointFrames.Apply(part.joint, part.jointFrame);
                        JointMath.ApplyLimits(part.joint, part.limits);
                    }

                    if (part.partCollider != null)
                    {
                        part.partCollider.sharedMaterial = PartInfo.IsFoot(part.id) ? footMaterial : bodyMaterial;
                    }
                }
            }

            anim = new AnimRig(unit.transform, rig);
            foreach (var part in rig.Parts)
            {
                if (part.body != null)
                {
                    part.body.isKinematic = true;
                    part.body.interpolation = RigidbodyInterpolation.Interpolate;
                }
            }

            foreach (var id in PartInfo.All)
            {
                rig.Part(id).transform.SetParent(unit.transform, true);
            }

            authority = null;
            if (physical)
            {
                Physics.SyncTransforms();
                var parts = rig.Parts;
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

                // As in the fight: the legs carried exactly (the feet's plants are the walk's), the rest held.
                authority = new Authority.AuthorityController(rig, anim);
                authority.Set(Authority.BodyRegion.Legs, Authority.AuthorityLevel.Kinematic, 1f);
                authority.Set(Authority.BodyRegion.Torso, Authority.AuthorityLevel.Pinned, profile.torsoPin);
                authority.Set(Authority.BodyRegion.Head, Authority.AuthorityLevel.Pinned, profile.torsoPin);
                authority.Set(Authority.BodyRegion.ArmL, Authority.AuthorityLevel.Pinned, profile.armPin);
                authority.Set(Authority.BodyRegion.ArmR, Authority.AuthorityLevel.Pinned, profile.armPin);
            }

            snapped = false;
            down = 0f;
            usualOff = Vector3.zero;

            float legLength = anim.RestLocalPosition(PartId.ShinL).magnitude + anim.RestLocalPosition(PartId.FootL).magnitude;
            loco = new Locomotor(legLength);
            if (!loco.Ready)
            {
                Debug.LogError("[LocomotionLab] the clip library (Resources/Locomotion/LocoClips.json) is missing clips.");
                enabled = false;
                return;
            }

            poser = new LocoBody(anim, loco) { GroundY = origin.y };
            poser.SoleFrom(rig);
            measures = new LocoMeasures(anim, loco, poser);
            film = new LocoFilm(Time.fixedDeltaTime, Mathf.Max(1, Mathf.RoundToInt(0.04f / Time.fixedDeltaTime)));
            // (The guard, step 2 of the combat overhaul: the sword and shield clips for every body for now, LocoLibrary.CombatPrefix.)
            loco.SetGuard(LocoLibrary.CombatPrefix(body.ToString()));
            loco.GuardWanted = guardUp;
            size = legLength / 0.845f;
            EquipLoadout(size);
            float mass = 0f;
            foreach (var part in rig.Parts)
            {
                mass += part.body != null ? part.body.mass : 0f;
            }

            hits = new StrikeHits(loco, poser, anim) { Weapon = mainWeapon, Scale = size, Slot = rig.SlotWeaponR, Mass = Mathf.Max(20f, mass) };
            hits.Landed += text => legs.Add($"{time:0.00} s: {text}");
            path = LocoCourse.Lab();
            path.SpeedScale = PaceScale;
            path.LegStarted += (i, name) =>
            {
                leg = name ?? "";
                legs.Add($"{time:0.00} s: {leg}");
            };
            if (standAt.HasValue)
            {
                loco.Place(standAt.Value, standFacing);
                path.Reset();
                return;
            }

            loco.Place(origin + (course ? path.Start : Vector3.zero), course ? path.StartYaw : 0f);
            path.Reset();
            time = 0f;
            ClearPrints();
            dummyAt = origin + Vector3.forward * (1.6f * size);
            dummyFacing = 180f;
            aiming = null;
            lastAim = "";
            MakeDummy();
        }

        private void FixedUpdate()
        {
            if (loco == null)
            {
                return;
            }

            if (paused && !stepOnce)
            {
                // (Paused: the pose stands still, and the physical body is still held to it; left alone,
                // its pinned upper body would sag over its kinematic legs.)
                if (authority != null && snapped)
                {
                    authority.Apply(Time.fixedDeltaTime);
                }

                dummy?.Step(Time.fixedDeltaTime, anim.Part(PartId.Chest).position, true);
                return;
            }

            stepOnce = false;
            float dt = Time.fixedDeltaTime;
            time += dt;
            Vector3 want;
            float facing;
            if (course)
            {
                // (The course is in board metres: the walk's world is the scene's, the board's middle at origin.)
                path.Steer(dt, loco.Position - origin, loco.Facing, loco.Velocity, out want, out facing);
                var stop = path.StopPoint;
                loco.StopAt = stop.HasValue ? stop.Value + origin : (Vector3?)null;
            }
            else
            {
                Drive(out want, out facing);
                loco.StopAt = null;
                leg = "driving";
            }

            if (down > 0f)
            {
                // Knocked down: physics alone has the body; it is stood up again after a while.
                down -= dt;
                authority.Apply(dt);
                dummy?.Step(dt, rig.Part(PartId.Chest).transform.position, false);
                if (down <= 0f)
                {
                    // (Up again where it fell, facing the way its chest faces: the getting up itself
                    // is the game's, when the two are joined.)
                    var pelvis = rig.Part(PartId.Pelvis).transform;
                    Vector3 at = new Vector3(pelvis.position.x, origin.y, pelvis.position.z);
                    Vector3 chestForward = rig.Part(PartId.Chest).transform.forward;
                    float yaw = new Vector2(chestForward.x, chestForward.z).sqrMagnitude > 1e-4f ? Mathf.Atan2(chestForward.x, chestForward.z) * Mathf.Rad2Deg : loco.Facing;
                    // (The film and the clock go on; the measures start again with the new body.)
                    var keepFilm = film;
                    float keepTime = time;
                    Build(at, yaw);
                    film = keepFilm;
                    time = keepTime;
                    legs.Add($"{time:0.00} s: stood up again");
                }

                return;
            }

            Bumped(dt);
            if (loco.Current == Locomotor.Mode.OneShot && loco.Shot != null && loco.Shot.IsGuard)
            {
                // (A combat clip playing: it moves the body itself; the steering waits.)
                want = Vector3.zero;
                facing = loco.Facing;
            }

            // (A hit stop: the walker and the dummy stand still a moment; the physical bodies are still held.)
            bool frozen = hits != null && hits.HitStop > 0f;
            if (!frozen)
            {
                loco.Step(dt, want, facing);
                poser.Pose(dt);
            }

            hits?.Step(dt, dummy);
            dummy?.Step(dt, anim.Part(PartId.Chest).position, frozen);
            WatchAim();
            measures.Sample(dt, time, leg);
            film.Add(time, leg, loco, poser, anim, origin);
            if (authority != null && snapped)
            {
                authority.Apply(dt);
                if (new Vector2(loco.Velocity.x, loco.Velocity.z).magnitude > KnockdownSpeed)
                {
                    KnockDown("pushed faster than it can step");
                }
            }
            else
            {
                Carry();
                snapped = true;
            }

            Prints();
        }

        // ---- Bumps and knockdowns (the plan's stage 6) ---------------------------------------------------

        [Tooltip("Something that knocks the physical upper body this far off its pose (m) pushes the walk; it steps to follow.")]
        public float bumpFrom = 0.04f;

        [Tooltip("How hard the upper body's displacement pushes the walk (m/s added a second, per metre).")]
        public float bumpGain = 25f;

        [Tooltip("Faster than this (m/s) and the steps cannot catch the body: it goes down (a ragdoll), and stands again after a while.")]
        public float knockdownSpeed = 3.2f;

        private float KnockdownSpeed => Mathf.Max(knockdownSpeed, loco.Running ? RunSpeed * 1.2f : knockdownSpeed);

        private float down;
        private string note = "";
        private Vector3 usualOff;

        /// <summary>
        /// Contact: the chest's physical body pushed off its pose (by a ball, a body, a blow) pushes the
        /// walk the same way, so the feet step after the body instead of the body tipping over them.
        /// </summary>
        private void Bumped(float dt)
        {
            if (authority == null || !snapped)
            {
                return;
            }

            var chest = rig.Part(PartId.Chest).body;
            if (chest == null || chest.isKinematic)
            {
                return;
            }

            Vector3 off = chest.position - anim.Part(PartId.Chest).position;
            off.y = 0f;
            // (Against the chest's usual lag behind its pose, which the walk's own speeding up and
            // slowing down makes: only a knock off that counts, or the walk would push itself.)
            usualOff = Vector3.Lerp(usualOff, off, 1f - Mathf.Exp(-dt / 0.5f));
            Vector3 knock = off - usualOff;
            float beyond = knock.magnitude - bumpFrom;
            if (beyond > 0f)
            {
                loco.Nudge(knock.normalized * (beyond * bumpGain * dt));
            }

            if (off.magnitude > 0.45f)
            {
                KnockDown("upper body knocked " + (off.magnitude * 100f).ToString("0") + " cm off");
            }
        }

        private void KnockDown(string why)
        {
            if (authority == null || down > 0f)
            {
                return;
            }

            down = 3f;
            note = "Knocked down: " + why;
            legs.Add($"{time:0.00} s: knocked down ({why})");
            foreach (Authority.BodyRegion region in Enum.GetValues(typeof(Authority.BodyRegion)))
            {
                authority.Set(region, Authority.AuthorityLevel.Ragdoll, 0f);
            }

            // (The body keeps the speed it had.)
            foreach (var part in rig.Parts)
            {
                if (part.body != null && !part.body.isKinematic)
                {
                    continue;
                }

                if (part.body != null)
                {
                    part.body.isKinematic = false;
                    part.body.linearVelocity = loco.Velocity;
                }
            }
        }

        /// <summary>A heavy ball thrown at the chest from a random side (H): something physical to bump into it.</summary>
        public void Throw()
        {
            if (loco == null)
            {
                return;
            }

            float yaw = (float)(dice.NextDouble() * 360.0);
            Vector3 chest = anim.Part(PartId.Chest).position;
            Vector3 from = chest + Quaternion.Euler(0f, yaw, 0f) * Vector3.forward * 3f + Vector3.up * 0.3f;
            var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.name = "Lab ball";
            ball.transform.position = from;
            ball.transform.localScale = Vector3.one * 0.3f;
            var body = ball.AddComponent<Rigidbody>();
            body.mass = ballMass;
            body.linearVelocity = (chest - from).normalized * ballSpeed + Vector3.up * 1.2f;
            Destroy(ball, 5f);
            legs.Add($"{time:0.00} s: ball thrown from {yaw:0} degrees");
        }

        [Tooltip("The thrown ball (H): its mass (kg) and speed (m/s).")]
        public float ballMass = 12f, ballSpeed = 8f;

        /// <summary>The body parts where the animation rig has them (each one kinematic, moved by its body).</summary>
        private void Carry()
        {
            foreach (var id in PartInfo.All)
            {
                var part = rig.Part(id);
                var target = anim.Part(id);
                if (part.body != null)
                {
                    part.body.MovePosition(target.position);
                    part.body.MoveRotation(target.rotation);
                }
                else
                {
                    part.transform.SetPositionAndRotation(target.position, target.rotation);
                }
            }
        }

        /// <summary>
        /// Leaves the lab for the fight (F): this run is logged, the lab's body and footprints go, and the
        /// sandbox spawns the fight, whose units walk with the new walk.
        /// </summary>
        public void WatchFight()
        {
            WriteLog();
            ClearPrints();
            if (unit != null)
            {
                Destroy(unit);
            }

            dummy?.Destroy();
            dummy = null;
            ShowAimMark(false);
            loco = null;
            Time.timeScale = 1f;
            var sandbox = GetComponent<BattleSandbox>();
            if (sandbox == null)
            {
                sandbox = FindAnyObjectByType<BattleSandbox>();
            }

            Destroy(this);
            if (sandbox != null)
            {
                sandbox.WatchFight();
            }
        }

        /// <summary>
        /// A shove from a random side: the chest is struck (the physical upper body takes it) and the walk
        /// steps to catch the body.
        /// </summary>
        // ---- The combat clips, played as they are (the combat plan's step 1: checking them on our body) ----

        private int combatClip;

        [Tooltip("The guard is up (T): the walker goes into its guard, and out of it again, over half a second.")]
        public bool guardUp;

        private void ToggleGuard()
        {
            guardUp = !guardUp;
            if (loco != null)
            {
                loco.GuardWanted = guardUp;
                legs.Add($"{time:0.00} s: guard {(guardUp ? "up" : "down")}");
            }
        }

        /// <summary>This body's guard and strike clips (LocoLibrary.CombatPrefix: the Medium's for every body for now).</summary>
        private List<string> CombatClips()
        {
            string prefix = LocoLibrary.CombatPrefix(body.ToString());
            var names = new List<string>();
            foreach (var pair in LocoLibrary.Clips)
            {
                if (pair.Value.IsGuard && pair.Key.StartsWith(prefix))
                {
                    names.Add(pair.Key);
                }
            }

            names.Sort((x, y) => string.CompareOrdinal(Order(x), Order(y)));
            return names;
        }

        // (Strikes first, then the guards, the footwork, the turns.)
        private static string Order(string name)
        {
            var clip = LocoLibrary.Get(name);
            string role = clip != null ? clip.Role : "";
            int rank = role == "strike" ? 0 : role == "guard_idle" ? 1 : role == "block" ? 2 : role == "cycle" ? 3 : 4;
            return rank + name;
        }

        private void StepCombatClip(int by)
        {
            int count = CombatClips().Count;
            if (count > 0)
            {
                combatClip = ((combatClip + by) % count + count) % count;
            }
        }

        private void PlayCombatClip()
        {
            var combat = CombatClips();
            if (loco == null || combat.Count == 0)
            {
                return;
            }

            var clip = LocoLibrary.Get(combat[Mathf.Clamp(combatClip, 0, combat.Count - 1)]);
            if (loco.Current == Locomotor.Mode.OneShot && loco.Shot != null && loco.Shot.ContactFrame >= 0)
            {
                // (A strike is under way: it plays out first. Started again on top of itself, it jerked back
                // to its own start each time J was pressed.)
                return;
            }

            if (clip.ContactFrame >= 0 && !course)
            {
                // (A strike: aimed at the dummy's part picked; a blocking dummy raises its shield there.)
                aimPoint = DummyTarget;
                poser.PlayStrike(clip, aimPoint);
                if (dummy != null)
                {
                    dummy.BlockAt = aimPoint;
                }

                aiming = clip;
                aimBest = float.MaxValue;
                aimHits = hits != null ? hits.Hits : 0;
                aimBlocks = hits != null ? hits.Blocks : 0;
                ShowAimMark(true, new Color(0.95f, 0.85f, 0.3f));
                legs.Add($"{time:0.00} s: struck at the dummy ({DummyHeightNames[dummyHeight]}, {Flat(aimPoint - loco.Position):0.00} m away; it {DummyStanceName}) with {clip.Name}: turn {poser.AimTurn:+0;-0} degrees, lunge x{poser.AimLunge:0.00}, the arm to make up {poser.AimMissBeforePull:0.00} m");
                return;
            }

            loco.Play(clip);
            legs.Add($"{time:0.00} s: played {clip.Name}");
        }

        // ---- The dummy (the combat plan's steps 3 and 4: strikes aimed at it, and landing on it) ----------

        private static readonly string[] DummyHeightNames = { "low (hips)", "middle (chest)", "high (face)" };
        private int dummyHeight = 1;
        private Vector3 dummyAt;
        private float dummyFacing = 180f;
        private float size = 1f;
        private LabDummy dummy;
        private StrikeHits hits;
        private WeaponDefinition mainWeapon;
        private GameObject aimMark;
        private Material aimMaterial;
        private LocoClip aiming;
        private float aimBest;
        private Vector3 aimPoint;
        private int aimHits, aimBlocks;
        private string lastAim = "";

        [Tooltip("How the dummy stands (O): open (its guard down), in its guard, or blocking (its shield held across each blow).")]
        public LabDummy.Stances dummyStance = LabDummy.Stances.Guard;

        /// <summary>Where the dummy is to be struck: the part picked (U), as it stands now.</summary>
        private Vector3 DummyTarget => dummy != null ? dummy.TargetAt(dummyHeight) : dummyAt + Vector3.up * (1.3f * size);

        private static float Flat(Vector3 v) => new Vector2(v.x, v.z).magnitude;

        /// <summary>The dummy (a Medium, holding a Medium's own loadout) made again where it is to stand.</summary>
        private void MakeDummy()
        {
            dummy?.Destroy();
            dummy = null;
            ShowAimMark(false);
            if (course || mediumPrefab == null)
            {
                return;
            }

            var holder = new GameObject("Spawning dummy");
            holder.SetActive(false);
            var made = Instantiate(mediumPrefab, dummyAt, Quaternion.Euler(0f, dummyFacing, 0f), holder.transform);
            made.name = "Lab dummy";
            var go = made.gameObject;
            for (int pass = 0; pass < 3; pass++)
            {
                var behaviours = go.GetComponentsInChildren<MonoBehaviour>(true);
                for (int i = behaviours.Length - 1; i >= 0; i--)
                {
                    var behaviour = behaviours[i];
                    if (behaviour != null && !(behaviour is UnitRig) && !(behaviour is BodyPart))
                    {
                        DestroyImmediate(behaviour);
                    }
                }
            }

            var dummyRig = go.GetComponent<UnitRig>();
            go.transform.SetParent(null, true);
            Destroy(holder);
            var profile = mediumPrefab.Profile != null ? mediumPrefab.Profile : UnitProfile.CreateDefault();
            dummy = new LabDummy(go, dummyRig, profile, dummyAt, dummyFacing, origin.y, "sns_", physical) { Stance = dummyStance };
            var (main, off) = LoadoutOf(BodySize.Medium);
            HoldFor(dummy, dummyRig.SlotWeaponR, dummy.Anim.SlotWeaponR, main, main != null && !main.IsShield ? UnitController.MainGrip : Quaternion.identity);
            HoldFor(dummy, dummyRig.SlotWeaponL, dummy.Anim.SlotWeaponL, off, off != null && off.IsShield ? UnitController.ShieldGrip : Quaternion.identity);
            dummy.Poser.CarryR = main == null ? LocoBody.Carry.Free : main.IsShield ? LocoBody.Carry.Shield : LocoBody.Carry.Weapon;
            dummy.Poser.CarryL = off == null ? LocoBody.Carry.Free : off.IsShield ? LocoBody.Carry.Shield : LocoBody.Carry.Weapon;
            dummy.Poser.BladeR = (dummy.Anim.SlotWeaponR != null ? dummy.Anim.SlotWeaponR.localRotation : Quaternion.identity) * Vector3.forward;
            dummy.Poser.BladeL = (dummy.Anim.SlotWeaponL != null ? dummy.Anim.SlotWeaponL.localRotation : Quaternion.identity) * Vector3.forward;
        }

        /// <summary>What a body's marker in the scene holds (the first team's first).</summary>
        private static (WeaponDefinition main, WeaponDefinition off) LoadoutOf(BodySize size)
        {
            UnitSpawn own = null;
            foreach (var spawn in FindObjectsByType<UnitSpawn>())
            {
                if (spawn.body == size && (own == null || spawn.team < own.team))
                {
                    own = spawn;
                }
            }

            return own != null ? (own.mainHand, own.offHand) : (null, null);
        }

        /// <summary>A weapon put in the dummy's hand: its colliders only for the sweep (a shield's block blows).</summary>
        private static void HoldFor(LabDummy holder, Transform slot, Transform animSlot, WeaponDefinition definition, Quaternion grip)
        {
            if (slot == null)
            {
                return;
            }

            slot.localRotation = grip;
            if (animSlot != null)
            {
                animSlot.localRotation = grip;
            }

            if (definition == null || definition.prefab == null)
            {
                return;
            }

            var held = Instantiate(definition.prefab, slot);
            held.transform.localPosition = Vector3.zero;
            held.transform.localRotation = Quaternion.identity;
            held.transform.localScale = Vector3.one * holder.Size;
            foreach (var behaviour in held.GetComponentsInChildren<MonoBehaviour>(true))
            {
                Destroy(behaviour);
            }

            foreach (var collider in held.GetComponentsInChildren<Collider>(true))
            {
                collider.isTrigger = true;
                foreach (var part in holder.Rig.Parts)
                {
                    if (part.partCollider != null)
                    {
                        Physics.IgnoreCollision(collider, part.partCollider, true);
                    }
                }

                if (definition.IsShield)
                {
                    holder.ShieldColliders.Add(collider);
                }
            }
        }

        private void NextDummyHeight() => dummyHeight = (dummyHeight + 1) % DummyHeightNames.Length;

        private void NextDummyStance()
        {
            dummyStance = (LabDummy.Stances)(((int)dummyStance + 1) % 3);
            if (dummy != null)
            {
                dummy.Stance = dummyStance;
            }

            legs.Add($"{time:0.00} s: the dummy {DummyStanceName}");
        }

        private string DummyStanceName => dummyStance == LabDummy.Stances.Open ? "stands open" : dummyStance == LabDummy.Stances.Guard ? "is in its guard" : "blocks";

        /// <summary>The dummy put straight ahead of the walker, as far as the picked strike reaches (Y).</summary>
        private void DummyInReach()
        {
            var combat = CombatClips();
            var clip = combat.Count > 0 ? LocoLibrary.Get(combat[Mathf.Clamp(combatClip, 0, combat.Count - 1)]) : null;
            float reach = 1.6f * size;
            if (clip != null && StrikeReach.TryGet(body.ToString(), clip.Name, out var grip, out var way))
            {
                Vector3 sweet = grip + way * (poser.WeaponLength * poser.SweetSpot);
                reach = Flat(sweet);
            }

            dummyAt = new Vector3(loco.Position.x, origin.y, loco.Position.z) + Quaternion.Euler(0f, loco.Facing, 0f) * Vector3.forward * reach;
            dummyFacing = loco.Facing + 180f;
            MakeDummy();
        }

        /// <summary>A small ball where the last strike was aimed, coloured by what it did (green landed, blue blocked, red missed).</summary>
        private void ShowAimMark(bool on, Color color = default)
        {
            if (aimMark == null)
            {
                if (!on)
                {
                    return;
                }

                var shader = Shader.Find("Universal Render Pipeline/Unlit");
                if (shader == null)
                {
                    shader = Shader.Find("Unlit/Color");
                }

                aimMaterial = new Material(shader);
                aimMark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                aimMark.name = "Aim mark";
                var collider = aimMark.GetComponent<Collider>();
                if (collider != null)
                {
                    Destroy(collider);
                }

                aimMark.GetComponent<MeshRenderer>().sharedMaterial = aimMaterial;
                aimMark.transform.localScale = Vector3.one * 0.06f;
            }

            aimMark.SetActive(on);
            if (on)
            {
                aimMark.transform.position = aimPoint;
                aimMaterial.color = color;
            }
        }

        /// <summary>How near the blade came to where it was aimed about the strike's contact; told when it ends.</summary>
        private void WatchAim()
        {
            if (aiming == null)
            {
                return;
            }

            if (loco.Shot != aiming)
            {
                bool landed = hits != null && hits.Hits > aimHits, blocked = hits != null && hits.Blocks > aimBlocks;
                string result = landed || blocked ? hits.Last.Substring(hits.Last.IndexOf(':') + 2)
                    : aimBest < float.MaxValue ? $"missed: the blade passed {aimBest * 100f:0} cm from where it was aimed" : "missed";
                lastAim = $"{aiming.Name}: turn {poser.AimTurn:+0;-0}°, lunge x{poser.AimLunge:0.00}, the arm made up {poser.AimMissBeforePull:0.00} m; {result}";
                legs.Add($"{time:0.00} s: {lastAim}");
                ShowAimMark(true, landed ? new Color(0.35f, 0.8f, 0.35f) : blocked ? new Color(0.35f, 0.55f, 0.9f) : new Color(0.9f, 0.35f, 0.3f));
                aiming = null;
                return;
            }

            float contact = aiming.ContactFrame / aiming.Rate;
            var slot = anim.SlotWeaponR;
            if (slot == null || Mathf.Abs(loco.ShotTime - contact) > 0.1f)
            {
                return;
            }

            Vector3 a = slot.position, ab = slot.rotation * Vector3.forward * poser.WeaponLength;
            float t = Mathf.Clamp01(Vector3.Dot(aimPoint - a, ab) / Mathf.Max(1e-6f, ab.sqrMagnitude));
            aimBest = Mathf.Min(aimBest, (a + ab * t - aimPoint).magnitude);
        }

        private void NextLoadout()
        {
            if (loadouts.Count == 0)
            {
                return;
            }

            loadout = (loadoutNow + 1) % loadouts.Count;
            Build();
        }

        public void Shove() => Shove(1f);

        public void Shove(float times)
        {
            if (loco == null)
            {
                return;
            }

            float yaw = (float)(dice.NextDouble() * 360.0);
            Vector3 way = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
            loco.Push(way * shove * times);
            shoves++;
            legs.Add($"{time:0.00} s: shoved toward {yaw:0} degrees");
            if (authority != null)
            {
                var chest = rig.Part(PartId.Chest).body;
                if (chest != null && !chest.isKinematic)
                {
                    chest.AddForce(way * (shove * times * 2.5f), ForceMode.VelocityChange);
                }
            }
        }

        private void Drive(out Vector3 want, out float facing)
        {
            Vector3 input = Vector3.zero;
            if (SandboxInput.KeyHeld(KeyCode.W)) input.z += 1f;
            if (SandboxInput.KeyHeld(KeyCode.S)) input.z -= 1f;
            if (SandboxInput.KeyHeld(KeyCode.D)) input.x += 1f;
            if (SandboxInput.KeyHeld(KeyCode.A)) input.x -= 1f;
            float camYaw = orbitCamera != null ? orbitCamera.transform.eulerAngles.y : 0f;
            Vector3 way = Quaternion.Euler(0f, camYaw, 0f) * Vector3.ClampMagnitude(input, 1f);
            float speed = SandboxInput.KeyHeld(KeyCode.LeftShift) ? RunSpeed : walkSpeed * PaceScale;
            want = way * speed;
            bool hold = SandboxInput.KeyHeld(KeyCode.Space);
            if (loco.GuardWanted)
            {
                // (In the guard it faces the dummy, as it would an enemy (where the camera looks, on the
                // course), and steps whichever way it is driven: forward, back, to the sides.)
                want = way * speed;
                Vector3 toDummy = (dummy != null ? dummy.Loco.Position : dummyAt) - loco.Position;
                facing = !course && Flat(toDummy) > 0.2f ? Mathf.Atan2(toDummy.x, toDummy.z) * Mathf.Rad2Deg : camYaw;
                return;
            }

            if (hold != holding)
            {
                holding = hold;
                legs.Add($"{time:0.00} s: facing {(hold ? "held" : "let go")}");
            }

            facing = !hold && way.sqrMagnitude > 0.01f ? Mathf.Atan2(way.x, way.z) * Mathf.Rad2Deg : loco.Facing;
        }

        private void Update()
        {
            if (loco == null)
            {
                return;
            }

            if (SandboxInput.KeyPressed(KeyCode.R)) Build();
            if (SandboxInput.KeyPressed(KeyCode.C)) { course = !course; Build(); }
            if (SandboxInput.KeyPressed(KeyCode.P)) paused = !paused;
            // (< and > pick the combat clip, as the panel's buttons do; the pause, P, stays.)
            if (SandboxInput.KeyPressed(KeyCode.Comma)) { StepCombatClip(-1); }
            if (SandboxInput.KeyPressed(KeyCode.Period)) { StepCombatClip(1); }
            if (SandboxInput.KeyPressed(KeyCode.B)) { physical = !physical; Build(); }
            if (SandboxInput.KeyPressed(KeyCode.G)) Shove();
            if (SandboxInput.KeyPressed(KeyCode.H)) Throw();
            if (SandboxInput.KeyPressed(KeyCode.K)) Shove(2.5f);
            if (SandboxInput.KeyPressed(KeyCode.F))
            {
                WatchFight();
                return;
            }

            if (SandboxInput.KeyPressed(KeyCode.L)) { NextLoadout(); }
            if (SandboxInput.KeyPressed(KeyCode.J)) { PlayCombatClip(); }
            if (SandboxInput.KeyPressed(KeyCode.U)) { NextDummyHeight(); }
            if (SandboxInput.KeyPressed(KeyCode.Y)) { DummyInReach(); }
            if (SandboxInput.KeyPressed(KeyCode.O)) { NextDummyStance(); }
            if (SandboxInput.KeyPressed(KeyCode.T)) { ToggleGuard(); }
            if (SandboxInput.KeyPressed(KeyCode.N)) { body = body == BodySize.Small ? BodySize.Medium : body == BodySize.Medium ? BodySize.Large : BodySize.Small; Build(); }
            if (SandboxInput.KeyPressed(KeyCode.V)) walkSpeed = walkSpeed > 1.2f ? 0.7f : walkSpeed > 0.9f ? 1.4f : 1.0f;
        }

        private void LateUpdate()
        {
            if (loco != null && orbitCamera != null)
            {
                Vector3 follow = loco.Position + Vector3.up * 1.0f;
                orbitCamera.target = Vector3.Lerp(orbitCamera.target, follow, 1f - Mathf.Exp(-4f * Time.deltaTime));
            }
        }

        // ---- Footprints ---------------------------------------------------------------------------------

        private readonly List<GameObject> prints = new List<GameObject>();
        private int nextPrint;
        private readonly bool[] wasLocked = new bool[2];
        private Material[] printMaterials;

        private void ClearPrints()
        {
            foreach (var p in prints)
            {
                if (p != null)
                {
                    Destroy(p);
                }
            }

            prints.Clear();
            nextPrint = 0;
            wasLocked[0] = wasLocked[1] = false;
        }

        private void Prints()
        {
            for (int f = 0; f < 2; f++)
            {
                bool locked = poser.Feet[f].Locked;
                if (footprints && locked && !wasLocked[f])
                {
                    var foot = anim.Part(f == 0 ? PartId.FootL : PartId.FootR);
                    Vector3 heel = foot.position;
                    Vector3 toe = foot.position + foot.rotation * new Vector3(0f, -0.06f, 0.16f);
                    Vector3 along = toe - heel;
                    along.y = 0f;
                    var mark = PrintMark(f);
                    mark.transform.position = new Vector3(0.5f * (heel.x + toe.x), origin.y + 0.006f, 0.5f * (heel.z + toe.z));
                    mark.transform.rotation = along.sqrMagnitude > 1e-6f ? Quaternion.LookRotation(along) : Quaternion.identity;
                    mark.SetActive(true);
                }

                wasLocked[f] = locked;
            }
        }

        private GameObject PrintMark(int foot)
        {
            if (printMaterials == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Unlit");
                if (shader == null)
                {
                    shader = Shader.Find("Unlit/Color");
                }

                printMaterials = new Material[2];
                for (int i = 0; i < 2; i++)
                {
                    printMaterials[i] = new Material(shader) { color = i == 0 ? new Color(0.3f, 0.5f, 0.9f) : new Color(0.9f, 0.4f, 0.3f) };
                }
            }

            const int Most = 24;
            GameObject mark;
            if (prints.Count < Most)
            {
                mark = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mark.name = "Footprint";
                var collider = mark.GetComponent<Collider>();
                if (collider != null)
                {
                    Destroy(collider);
                }

                mark.transform.localScale = new Vector3(0.07f, 0.005f, 0.2f);
                prints.Add(mark);
            }
            else
            {
                mark = prints[nextPrint % Most];
            }

            nextPrint++;
            mark.GetComponent<MeshRenderer>().sharedMaterial = printMaterials[foot];
            return mark;
        }

        // ---- The panel and the log ----------------------------------------------------------------------

        private void OnGUI()
        {
            if (loco == null)
            {
                return;
            }

            panel = GUILayout.Window(7711, panel, DrawPanel, "Locomotion lab");
        }

        private void DrawPanel(int id)
        {
            GUILayout.Label(course ? $"Course: {leg}" : "Driving: W A S D, Shift run, Space hold facing, V walk speed");
            GUILayout.Label($"{loco.Current}{(loco.Shot != null ? " " + loco.Shot.Name : "")}   {new Vector2(loco.Velocity.x, loco.Velocity.z).magnitude:0.00} m/s   run {loco.RunWeight:0.00}   phase {loco.Phase:0.00}");
            GUILayout.Label($"{body}: walking speed {walkSpeed * PaceScale:0.0} m/s, running {RunSpeed:0.0} m/s");
            GUILayout.BeginHorizontal();
            foreach (BodySize size in new[] { BodySize.Small, BodySize.Medium, BodySize.Large })
            {
                if (GUILayout.Button(body == size ? "> " + size + " <" : size.ToString()) && body != size)
                {
                    body = size;
                    Build();
                }
            }

            GUILayout.EndHorizontal();
            if (down > 0f)
            {
                GUILayout.Label(note + $" (up again in {down:0.0} s)");
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(course ? "Drive (C)" : "Course (C)")) { course = !course; Build(); }
            if (GUILayout.Button("Restart (R)")) Build();
            if (GUILayout.Button("Watch the fight (F)"))
            {
                WatchFight();
                return;
            }
            if (GUILayout.Button(paused ? "Play (P)" : "Pause (P)")) paused = !paused;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Shove (G)")) Shove();
            if (GUILayout.Button("Hard (K)")) Shove(2.5f);
            if (GUILayout.Button("Ball (H)")) Throw();
            if (GUILayout.Button(physical ? "Kinematic body (B)" : "Physical body (B)")) { physical = !physical; Build(); }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Speed");
            foreach (float s in new[] { 0.25f, 0.5f, 1f })
            {
                if (GUILayout.Button(s.ToString("0.##") + "x"))
                {
                    Time.timeScale = s;
                }
            }

            GUILayout.EndHorizontal();
            if (GUILayout.Button(guardUp ? $"Guard down (T)   [guard {loco.GuardWeight:0.0}]" : $"Guard up (T)   [guard {loco.GuardWeight:0.0}]"))
            {
                ToggleGuard();
            }

            var combat = CombatClips();
            if (combat.Count > 0)
            {
                combatClip = Mathf.Clamp(combatClip, 0, combat.Count - 1);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("< (,)")) StepCombatClip(-1);
                if (GUILayout.Button($"Play {combat[combatClip]} (J)")) PlayCombatClip();
                if (GUILayout.Button("(.) >")) StepCombatClip(1);
                GUILayout.EndHorizontal();
                var shown = LocoLibrary.Get(combat[combatClip]);
                if (shown != null)
                {
                    GUILayout.Label($"  {shown.Role}{(shown.Kind.Length > 0 ? ", " + shown.Kind : "")}, {shown.Duration:0.00} s{(shown.ContactFrame >= 0 ? $", contact at {shown.ContactFrame / shown.Rate:0.00} s" : "")}");
                    if (shown.ContactFrame >= 0 && StrikeReach.TryGet(body.ToString(), shown.Name, out var grip, out var way))
                    {
                        Vector3 sweet = grip + way * (poser.WeaponLength * poser.SweetSpot);
                        GUILayout.Label($"  reaches {Flat(sweet):0.0} m ahead, {sweet.y:0.0} m up{(Mathf.Abs(sweet.x) > 0.3f ? (sweet.x > 0f ? ", to the right" : ", to the left") : "")}");
                    }
                }

                if (!course)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button($"Aim: {DummyHeightNames[dummyHeight]} (U)")) NextDummyHeight();
                    if (GUILayout.Button($"Dummy {DummyStanceName} (O)")) NextDummyStance();
                    if (GUILayout.Button("In reach (Y)")) DummyInReach();
                    GUILayout.EndHorizontal();
                    GUILayout.Label($"  the dummy is {Flat(DummyTarget - loco.Position):0.0} m away, struck at {DummyTarget.y - origin.y:0.0} m{(hits != null ? $"; {hits.Hits} hits, {hits.Blocks} blocked" : "")}");
                    if (lastAim.Length > 0)
                    {
                        GUILayout.Label("  " + lastAim);
                    }
                }
            }

            if (loadouts.Count > 0 && GUILayout.Button($"Carrying: {loadouts[loadoutNow].name} (L)"))
            {
                NextLoadout();
            }

            footprints = GUILayout.Toggle(footprints, "Footprints");
            GUILayout.Label($"Planted feet moved at most {measures.PlantDriftWorst * 100f:0.0} cm over {measures.PlantsDone} plants; {measures.ForcedReleases} forced steps.");
            GUI.DragWindow();
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
            {
                Time.timeScale = 1f;
            }
        }

        private void OnApplicationQuit() => WriteLog();

        private void OnDestroy()
        {
            WriteLog();
            dummy?.Destroy();
            dummy = null;
            if (aimMark != null)
            {
                Destroy(aimMark);
            }
        }

        private bool written;

        /// <summary>The run's measures, its legs and its film, into PlaytestLogs/locolab-(time)/.</summary>
        private void WriteLog()
        {
            if (written || measures == null || time < 3f)
            {
                return;
            }

            written = true;
            try
            {
                folder = Path.Combine(Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PlaytestLogs")), "locolab-" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + (course ? "-course" : "-driven"));
                Directory.CreateDirectory(folder);
                var md = new StringBuilder();
                md.AppendLine("# Locomotion lab");
                md.AppendLine();
                md.AppendLine($"{time:0.0} s, {(course ? "the course" : "driven by hand")}. Draw the film with `python3 Tools/Mocap/Locomotion/locofilm.py {Path.GetFileName(folder)}/locolab.json film.gif` (from PlaytestLogs/).");
                md.AppendLine();
                md.AppendLine("## Measures");
                md.AppendLine();
                md.AppendLine(measures.Report());
                md.AppendLine("## Legs");
                md.AppendLine();
                foreach (var l in legs)
                {
                    md.AppendLine("- " + l);
                }

                File.WriteAllText(Path.Combine(folder, "session.md"), md.ToString());
                File.WriteAllText(Path.Combine(folder, "locolab.json"), film.ToString());
                Debug.Log("[LocomotionLab] wrote " + folder);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[LocomotionLab] could not write the log: " + e.Message);
            }
        }
    }
}
