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
    /// P pause, M the panel back at the top left, comma and period the combat clip to play, J play it. Driving: W A S D (against the camera), Shift run, Space hold the facing
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

        /// <summary>The walker: a clip fighter that strikes (and doesn't defend yet: nothing strikes it), steered by the course or by hand.</summary>
        private ClipFighter walker;

        // (The walker's own parts, for short.)
        private GameObject unit;
        private UnitRig rig;
        private AnimRig anim;
        private Locomotor loco;
        private LocoBody poser;
        private Authority.AuthorityController authority;

        private LocoMeasures measures;
        private LocoCourse path;
        private LocoFilm film;
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
            walker.ShieldColliders.Clear();
            HoldFor(walker, rig.SlotWeaponR, anim.SlotWeaponR, main, main != null && !main.IsShield ? UnitController.MainGrip : Quaternion.identity);
            HoldFor(walker, rig.SlotWeaponL, anim.SlotWeaponL, off, off != null && off.IsShield ? UnitController.ShieldGrip : Quaternion.identity);
            // (Its defence, as its loadout has it: a shield blocks; a sword or a club in one hand alone parries, the free
            // hand tucked out of the way (the playtest: the Large's club alone guarded as if behind an unseen shield);
            // bare hands, none yet.)
            bool oneHanded = main != null && (main.kind == WeaponKind.Sword || main.kind == WeaponKind.Club) && !LocoLibrary.TwoHanded(body.ToString());
            walker.Stance = off != null && off.IsShield ? ClipFighter.Stances.Shield
                : oneHanded && off == null ? ClipFighter.Stances.Sword : ClipFighter.Stances.Open;
            walker.ShieldSkill = walkerShieldSkill;
            walker.ParrySkill = walkerParrySkill;
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
            Streak(rig.SlotWeaponR, main, scale);
            // (One weapon alone: its parry from the start, so its free hand is tucked away even with no dummy to face;
            // the dummy's making gives it the dummy to keep out of.)
            walker.Parry = walker.Stance == ClipFighter.Stances.Sword ? HeldAs(new SwordParry(anim, loco, rig, size, poser.WeaponLength), main) : null;
        }

        /// <summary>A parry held as its weapon is: a club as a brute holds one (<see cref="SwordParry.AsClub"/>), a sword as a fencer.</summary>
        private static SwordParry HeldAs(SwordParry parry, WeaponDefinition weapon) =>
            weapon != null && weapon.kind == WeaponKind.Club ? parry.AsClub() : parry;

        /// <summary>How the blades' streaks look (every streak shares it: the panel's switch and length change it live).</summary>
        private readonly StreakSettings streaks = new StreakSettings();

        /// <summary>A streak on the blade in a hand (none for a shield or an empty hand); it goes when the blade does.</summary>
        private void Streak(Transform slot, WeaponDefinition definition, float scale)
        {
            if (slot == null || definition == null || definition.IsShield || !Application.isPlaying)
            {
                return;
            }

            // (The slot's own space: the weapon sits on it unturned, at the unit's scale.)
            Vector3 tip = definition.tip.sqrMagnitude > 1e-6f ? definition.tip * scale : Vector3.forward * (definition.length * scale);
            float from = definition.kind == WeaponKind.Club ? 0.45f : 0.25f;
            BladeStreak.Sweep(slot, tip * from, tip, streaks, transform);
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

            if (walker != null)
            {
                walker.Destroy();
                walker = null;
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

            // The walker: a clip fighter, its body set up as a unit sets up its own (its joints in their frames,
            // its limits, its friction) while it is still in its rest pose, then held as in the fight: the legs
            // carried exactly (the feet's plants are the walk's), the rest pinned.
            var profile = prefab.Profile != null ? prefab.Profile : UnitProfile.CreateDefault();
            walker = new ClipFighter(unit, rig, profile, origin, 0f, origin.y, LocoLibrary.CombatPrefix(body.ToString()), physical) { TraceName = "the walker" };
            anim = walker.Anim;
            loco = walker.Loco;
            poser = walker.Poser;
            authority = walker.Authority;
            down = 0f;
            usualOff = Vector3.zero;
            if (!loco.Ready)
            {
                Debug.LogError("[LocomotionLab] the clip library (Resources/Locomotion/LocoClips.json) is missing clips.");
                enabled = false;
                return;
            }

            measures = new LocoMeasures(anim, loco, poser);
            film = new LocoFilm(Time.fixedDeltaTime, Mathf.Max(1, Mathf.RoundToInt(0.04f / Time.fixedDeltaTime)));
            size = walker.Size;
            EquipLoadout(size);
            // (The guard, step 2 of the combat overhaul: the sword and shield clips for every body for now, LocoLibrary.CombatPrefix.)
            loco.GuardWanted = guardUp;
            hits = walker.EnableStrikes(mainWeapon);
            hits.Slot = rig.SlotWeaponR;
            hits.Finishing = () => combo != null && combo.LastBlow;
            hits.Comboing = () => combo != null && combo.Playing && combo.Cores;
            hits.Landed += text => legs.Add($"{time:0.00} s: {text}{(dummy != null && dummy.LastReaction.Length > 0 ? "; the dummy: " + dummy.LastReaction : "")}");
            combo = new ComboPlayer(loco, poser) { TargetFor = TargetFor, Strikes = Strikes(), TargetVelocity = () => dummy != null ? dummy.Loco.Velocity : Vector3.zero };
            walker.Combo = combo;
            // (A strike alone follows the dummy, as far as its pull goes.)
            walker.Follow = clip => dummy != null ? TargetFor(clip) : (Vector3?)null;
            combo.Started += (clip, link) =>
            {
                BeganAim(clip, poser.AimTarget);
                if (link == null && !combo.Pressing && comboBlow < 0)
                {
                    comboHits.Clear();
                }

                comboBlow++;
            };
            // (The combo's beat: when each of its blows first landed, for the panel.)
            hits.Landed += text =>
            {
                if (combo != null && combo.Playing && comboBlow != comboLanded)
                {
                    comboLanded = comboBlow;
                    comboHits.Add(time);
                }
            };
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
                if (authority != null && walker.Snapped)
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
                if (footwork && dummy != null && walkerFeet != null)
                {
                    // (Footwork: it steers itself against the dummy, as the fight's units steer.)
                    walkerFeet.Steer(dummy, dummyFeet, dt, out want, out facing);
                    leg = "footwork: " + walkerFeet.Doing;
                }

                // (A combo pressing after the dummy (its last blow drove it back): the combo walks the walker in.)
                if (combo != null && combo.Pressing)
                {
                    want = combo.Want;
                    facing = combo.WantFacing;
                    leg = "pressing";
                }
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

            // (The fighting distance: neither body is taken nearer the other than it, the walker's lunges cut short
            // there, the dummy stepping back from it.)
            if (dummy != null)
            {
                float keep = ClipFighter.FightingDistance(size, dummy.Size);
                poser.KeepFrom = dummy.Loco.Position;
                poser.KeepDistance = keep;
                dummy.KeepDistance = keep;
                dummy.StandDistance = ClipFighter.StandingDistance(size, dummy.Size);
            }
            else
            {
                poser.KeepFrom = null;
            }

            // (A hit stop, either one's blow: the walker and the dummy stand still a moment; the physical bodies are still held.)
            bool frozen = (hits != null && hits.HitStop > 0f) || (dummy?.Strikes != null && dummy.Strikes.HitStop > 0f);
            // (The dummy's blow coming, for the walker to meet with its shield or its sword.)
            walker.Watch(dummy?.Strikes);
            walker.Move(dt, want, facing, dummy != null ? dummy.Loco.Position : loco.Position, frozen);
            if (!frozen)
            {
                if (comboWas && combo != null && !combo.Playing)
                {
                    legs.Add($"{time:0.00} s: the combo ended{(combo.Ended.Length > 0 ? ": " + combo.Ended : "")}");
                }

                if (combo != null && combo.Pressing && !pressingWas)
                {
                    legs.Add($"{time:0.00} s: the combo presses after the dummy: {combo.Ended}");
                }

                comboWas = combo != null && combo.Playing;
                pressingWas = combo != null && combo.Pressing;
            }

            hits?.Step(dt, dummy);
            if (dummy != null)
            {
                // (The blow coming, for the dummy to meet with its shield or its sword.)
                dummy.Watch(hits);
            }

            if (dummy != null)
            {
                dummy.KeepOff(walker);
            }

            if (footwork && dummy != null && dummyFeet != null)
            {
                // (Footwork: the dummy steers itself against the walker too, instead of keeping to its spot.)
                dummyFeet.Steer(walker, walkerFeet, dt, out var dummyWant, out float dummyYaw);
                dummy.Move(dt, dummyWant, dummyYaw, anim.Part(PartId.Chest).position, frozen);
                dummy.Apply(dt);
                LogFootwork();
            }
            else
            {
                dummy?.Step(dt, anim.Part(PartId.Chest).position, frozen);
            }

            // (Two bodies never pass into each other, whatever they do.)
            if (dummy != null)
            {
                ClipFighter.KeepApart(walker, dummy);
            }

            if (dummy?.Strikes != null)
            {
                walkerOnly[0] = walker;
                dummy.Strikes.Step(dt, walkerOnly);
                WatchDummyBlow();
            }

            if (!frozen)
            {
                TradeBlows();
            }

            WatchAim();
            measures.Sample(dt, time, leg);
            film.Add(time, leg, loco, poser, anim, origin);
            bool wasSnapped = walker.Snapped;
            if (down > 0f)
            {
                // (Knocked down by a bump this step: the body is all physics's now; its arms aren't held again.)
                authority.Apply(dt);
            }
            else
            {
                walker.Apply(dt);
            }

            if (authority != null && wasSnapped)
            {
                // (Its own clip carrying it fast, a lunge stretched to reach, is not a push it can't step after:
                // the Large's long lunge reached 3.3 m/s and was knocked down by its own strike.)
                bool ownMotion = loco.Current == Locomotor.Mode.OneShot && loco.Shot != null && loco.Shot.IsGuard;
                if (!ownMotion && new Vector2(loco.Velocity.x, loco.Velocity.z).magnitude > KnockdownSpeed)
                {
                    KnockDown("pushed faster than it can step");
                }
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
            // (A blow that landed on it is its reaction's to show, and its own push back's: not a bump to step after.)
            if (authority == null || !walker.Snapped || walker.SinceStruck < 0.6f)
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

            // (Its own lunge leaves the pinned chest behind its pose for a moment, more the bigger and faster the
            // body: that lag is not a knock. Then only a knock beyond the usual lag counts.)
            bool ownMotion = loco.Current == Locomotor.Mode.OneShot && loco.Shot != null && loco.Shot.IsGuard;
            float offNow = ownMotion ? knock.magnitude : off.magnitude;
            if (offNow > 0.45f * Mathf.Max(1f, size))
            {
                KnockDown("upper body knocked " + (offNow * 100f).ToString("0") + " cm off");
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
                    // (No faster than a body can be pushed: a combo's snap is not a speed it carries off, sliding.)
                    part.body.linearVelocity = Vector3.ClampMagnitude(loco.Velocity, KnockdownSpeed);
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

        /// <summary>
        /// Leaves the lab for the fight (F): this run is logged, the lab's body and footprints go, and the
        /// sandbox spawns the fight, whose units walk with the new walk.
        /// </summary>
        public void WatchFight()
        {
            WriteLog();
            ClearPrints();
            if (walker != null)
            {
                walker.Destroy();
                walker = null;
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
                if (pair.Value.IsGuard && pair.Key.StartsWith(prefix) && (pair.Value.Role != "strike" || LocoLibrary.StrikeFor(body.ToString(), pair.Key)))
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
            if ((combo != null && combo.Playing) || (loco.Current == Locomotor.Mode.OneShot && loco.Shot != null && loco.Shot.ContactFrame >= 0))
            {
                // (A strike is under way: it plays out first. Started again on top of itself, it jerked back
                // to its own start each time J was pressed.)
                return;
            }

            if (clip.ContactFrame >= 0 && !course)
            {
                // (A strike: aimed at the dummy's part picked; a blocking dummy raises its shield there.)
                Vector3 at = DummyTarget;
                poser.PlayStrike(clip, at);
                BeganAim(clip, at);
                legs.Add($"{time:0.00} s: struck at the dummy ({DummyHeightNames[dummyHeight]}, {Flat(at - loco.Position):0.00} m away; it {DummyStanceName}) with {clip.Name}: turn {poser.AimTurn:+0;-0} degrees, lunge x{poser.AimLunge:0.00}, the arm to make up {poser.AimMissBeforePull:0.00} m");
                return;
            }

            loco.Play(clip);
            legs.Add($"{time:0.00} s: played {clip.Name}");
        }

        // ---- The dummy (the combat plan's steps 3 and 4: strikes aimed at it, and landing on it) ----------

        private static readonly string[] DummyHeightNames = { "low (hips)", "middle (chest)", "high (face)", "legs (knees: every strike and combo)" };

        /// <summary>The aim at the dummy's legs (U's fourth): every strike and combo blow goes at its knees, to sweep them.</summary>
        private const int AimLegs = 3;
        private int dummyHeight = 1;
        private Vector3 dummyAt;
        private float dummyFacing = 180f;
        private float size = 1f;
        private ClipFighter dummy;
        private StrikeHits hits;
        private WeaponDefinition mainWeapon;
        private GameObject aimMark;
        private Material aimMaterial;
        private LocoClip aiming;
        private float aimBest;
        private Vector3 aimPoint;
        private int aimHits, aimBlocks;
        private string lastAim = "";

        [Tooltip("How the dummy is set up (O): open (its guard down, a sword only: every blow lands), sword and shield (it blocks with the shield), or sword only (it parries with the sword).")]
        public ClipFighter.Stances dummyStance = ClipFighter.Stances.Sword;

        [Tooltip("The dummy's shield skill and parry skill (0 to 1).")]
        [Range(0f, 1f)] public float dummyShieldSkill = 1f, dummyParrySkill = 1f;

        /// <summary>Where the dummy is to be struck: the part picked (U), as it stands now.</summary>
        private Vector3 DummyTarget => dummy != null ? dummy.TargetAt(dummyHeight == AimLegs ? -1 : dummyHeight) : dummyAt + Vector3.up * (1.3f * size);

        private static float Flat(Vector3 v) => new Vector2(v.x, v.z).magnitude;

        /// <summary>The dummy (a Medium, holding a Medium's own loadout) made again where it is to stand.</summary>
        private void MakeDummy()
        {
            dummy?.Destroy();
            dummy = null;
            // (The parry's reading of each blow, into the session's log: what it read coming, what it took, and how its
            // read changed as the blow came on; the playtest's own record of a parry that was late or wrong.)
            SwordParry.Trace = line =>
            {
                if (line.StartsWith("blow ", StringComparison.Ordinal))
                {
                    legs.Add($"{time:0.00} s: the parry: {line}");
                }
            };
            // (And the shield's (the plan "Shield and blade contact"): where it plans to meet each blow's blade.)
            ClipFighter.ShieldTrace = line => legs.Add($"{time:0.00} s: the shield: {line}");
            // (And each step back to a blow: the weight plan's step 5.)
            ClipFighter.GroundTrace = line => legs.Add($"{time:0.00} s: {line}");
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
            // (Every guard left foot forward, sword alone too: every strike and hit reaction is, and a fencer's right-lead guard
            // dragged a planted foot through the switch each time it struck or was struck.)
            dummy = new ClipFighter(go, dummyRig, profile, dummyAt, dummyFacing, origin.y, "sns_", physical) { Stance = dummyStance, TraceName = "the dummy" };
            var (main, off) = LoadoutOf(BodySize.Medium);
            if (dummyStance != ClipFighter.Stances.Shield)
            {
                // (Open, and parrying with the sword: no shield.)
                off = null;
            }

            dummy.ShieldSkill = dummyShieldSkill;
            dummy.ParrySkill = dummyParrySkill;
            HoldFor(dummy, dummyRig.SlotWeaponR, dummy.Anim.SlotWeaponR, main, main != null && !main.IsShield ? UnitController.MainGrip : Quaternion.identity);
            HoldFor(dummy, dummyRig.SlotWeaponL, dummy.Anim.SlotWeaponL, off, off != null && off.IsShield ? UnitController.ShieldGrip : Quaternion.identity);
            dummy.Poser.CarryR = main == null ? LocoBody.Carry.Free : main.IsShield ? LocoBody.Carry.Shield : LocoBody.Carry.Weapon;
            dummy.Poser.CarryL = off == null ? LocoBody.Carry.Free : off.IsShield ? LocoBody.Carry.Shield : LocoBody.Carry.Weapon;
            dummy.Poser.BladeR = (dummy.Anim.SlotWeaponR != null ? dummy.Anim.SlotWeaponR.localRotation : Quaternion.identity) * Vector3.forward;
            dummy.Poser.BladeL = (dummy.Anim.SlotWeaponL != null ? dummy.Anim.SlotWeaponL.localRotation : Quaternion.identity) * Vector3.forward;
            Streak(dummyRig.SlotWeaponR, main, dummy.Size);
            float swordLength = (main != null && !main.IsShield ? main.length : 0.8f) * dummy.Size;
            dummy.Poser.WeaponLength = swordLength;
            // (It strikes back too (the plan's step 2): with its sword, at the walker.)
            dummy.Poser.BodyKey = BodySize.Medium.ToString();
            var dummyHits = dummy.EnableStrikes(main != null && !main.IsShield ? main : null);
            dummyHits.Landed += text =>
            {
                dummyBlowLanded = true;
                legs.Add($"{time:0.00} s: the dummy's {text}{(walker.LastReaction.Length > 0 ? "; the walker: " + walker.LastReaction : "")}");
            };
            dummy.Follow = clip => walker != null ? dummy.AimAt(walker, clip) : (Vector3?)null;
            dummyAnswerAt = -1f;
            walkerFeet = new Footwork(walker, 0) { Mode = walkerTempo };
            dummyFeet = new Footwork(dummy, 1) { Mode = dummyTempo };
            if (walker.Stance == ClipFighter.Stances.Sword)
            {
                // (The walker's own sword parry, against the dummy: its body, its shield, its sword as they keep out of it.)
                walker.Parry = HeldAs(new SwordParry(anim, loco, rig, size, poser.WeaponLength)
                {
                    Opponent = new OpponentShape(dummy.Anim, dummy.Size, dummyStance == ClipFighter.Stances.Shield ? dummyRig.SlotWeaponL : null, 0.27f)
                    {
                        WeaponSlot = dummyRig.SlotWeaponR, WeaponLength = swordLength,
                    },
                }, mainWeapon);
            }
            else
            {
                walker.Parry = null;
            }

            if (dummyStance == ClipFighter.Stances.Sword)
            {
                dummy.Parry = new SwordParry(dummy.Anim, dummy.Loco, dummyRig, dummy.Size, swordLength)
                {
                    // (The walker as its sword keeps out of it: its body, and its shield if it carries one.)
                    Opponent = new OpponentShape(anim, size, poser.CarryL == LocoBody.Carry.Shield ? rig.SlotWeaponL : null, 0.27f) { WeaponSlot = poser.CarryR == LocoBody.Carry.Weapon ? rig.SlotWeaponR : null, WeaponLength = poser.WeaponLength },
                };
            }
        }

        // ---- Footwork (the plan "Bringing the lab into the fight", step 3) --------------------------------------

        [Tooltip("Both steer themselves: each walks in to where its strikes land, holds that distance, circles, backs off when crowded, as the fight's units do (its tempo moves the distance).")]
        public bool footwork;

        public Footwork.Tempo walkerTempo = Footwork.Tempo.Probe, dummyTempo = Footwork.Tempo.Probe;

        private Footwork walkerFeet, dummyFeet;
        private string footworkWas = "";

        private void ToggleFootwork()
        {
            footwork = !footwork;
            if (!footwork && dummy != null)
            {
                // (Its spot is where the footwork left it: it doesn't walk back across the board.)
                dummy.Home = dummy.Loco.Position;
            }

            legs.Add($"{time:0.00} s: footwork {(footwork ? $"on (the walker {walkerTempo}, the dummy {dummyTempo})" : "off")}");
        }

        private void LogFootwork()
        {
            if (walkerFeet == null || dummyFeet == null)
            {
                return;
            }

            string now = $"the walker {walkerFeet.Doing}, the dummy {dummyFeet.Doing}";
            if (now != footworkWas)
            {
                footworkWas = now;
                legs.Add($"{time:0.00} s: {Flat(dummy.Loco.Position - loco.Position):0.00} m apart (the walker wants {walkerFeet.Want:0.00}, the dummy {dummyFeet.Want:0.00}): {now}");
            }
        }

        private static Footwork.Tempo NextTempo(Footwork.Tempo t) => (Footwork.Tempo)(((int)t + 1) % 4);

        // ---- The dummy strikes back (the plan "Bringing the lab into the fight", step 2) -------------------------

        [Tooltip("The dummy trades blows: a moment after each of the walker's strikes or combos ends, it answers with one of its own (every third a combo).")]
        public bool dummyFightsBack = true;

        [Tooltip("How long after the walker's blow ends the dummy answers (seconds).")]
        public float answerAfter = 0.45f;

        [Range(0f, 1f)] public float walkerShieldSkill = 0.5f, walkerParrySkill = 0.5f;

        private readonly List<ClipFighter> walkerOnly = new List<ClipFighter> { null };
        private float dummyAnswerAt = -1f;
        private bool walkerWasStriking;
        private int dummyAnswers, dummyVariant;

        private LocoClip dummyBlow;
        private bool dummyBlowLanded;

        /// <summary>Each of the dummy's blows that met nothing is logged as it ends: missed, or cut off by its reaction to the walker's.</summary>
        private void WatchDummyBlow()
        {
            var shot = dummy.Loco.Current == Locomotor.Mode.OneShot && dummy.Loco.Shot != null && dummy.Loco.Shot.ContactFrame >= 0 ? dummy.Loco.Shot : null;
            if (dummyBlow != null && shot != dummyBlow)
            {
                if (!dummyBlowLanded)
                {
                    bool cut = dummy.Loco.Current == Locomotor.Mode.OneShot && dummy.Loco.Shot != null && dummy.Loco.Shot.ContactFrame < 0;
                    legs.Add($"{time:0.00} s: the dummy's {dummyBlow.Name}: {(cut ? "cut off by its reaction to your blow" : "missed")}");
                }

                dummyBlow = null;
            }

            if (shot != null && shot != dummyBlow)
            {
                dummyBlow = shot;
                dummyBlowLanded = false;
            }
        }

        /// <summary>Trading blows: the walker's blow over, the dummy answers a moment later.</summary>
        private void TradeBlows()
        {
            if (dummy == null || walker == null || course)
            {
                return;
            }

            bool striking = walker.Striking;
            if (walkerWasStriking && !striking && dummyFightsBack)
            {
                dummyAnswerAt = time + answerAfter;
            }
            else if (striking)
            {
                // (The walker strikes again before it answers: its turn waits until this blow is over too.)
                dummyAnswerAt = -1f;
            }

            walkerWasStriking = striking;
            if (dummyAnswerAt >= 0f && time >= dummyAnswerAt && !dummy.Busy && !walker.Fallen)
            {
                dummyAnswerAt = -1f;
                DummyStrikes(dummyAnswers % 3 == 2);
                dummyAnswers++;
            }
        }

        /// <summary>The dummy strikes at the walker (Z; Shift+Z its combo): a strike of its own that lands from where it stands.</summary>
        private void DummyStrikes(bool combo)
        {
            if (dummy == null || walker == null || course || dummy.Strikes == null || dummy.Busy)
            {
                return;
            }

            var clip = dummy.StrikeThatLands(walker);
            if (clip == null)
            {
                legs.Add($"{time:0.00} s: the dummy: none of its strikes reaches the walker from {Flat(dummy.Loco.Position - loco.Position):0.00} m");
                return;
            }

            if (combo && dummy.ComboAt(walker, clip, dummyVariant++, out var opener, out var chain))
            {
                legs.Add($"{time:0.00} s: the dummy strikes back with a combo: {ComboText(opener, chain)}");
                return;
            }

            Vector3 at = dummy.StrikeAt(walker, clip);
            legs.Add($"{time:0.00} s: the dummy strikes back with {clip.Name}, at {at.y - origin.y:0.0} m: turn {dummy.Poser.AimTurn:+0;-0} degrees, lunge x{dummy.Poser.AimLunge:0.00}");
        }

        private void SetWalkerShieldSkill(float skill)
        {
            walkerShieldSkill = Mathf.Clamp01(Mathf.Round(skill * 4f) / 4f);
            if (walker != null)
            {
                walker.ShieldSkill = walkerShieldSkill;
            }

            legs.Add($"{time:0.00} s: the walker's shield skill {walkerShieldSkill:0.00} ({SkillName(walkerShieldSkill)})");
        }

        private void SetWalkerParrySkill(float skill)
        {
            walkerParrySkill = Mathf.Clamp01(Mathf.Round(skill * 4f) / 4f);
            if (walker != null)
            {
                walker.ParrySkill = walkerParrySkill;
            }

            legs.Add($"{time:0.00} s: the walker's parry skill {walkerParrySkill:0.00} ({SkillName(walkerParrySkill)})");
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
        private static void HoldFor(ClipFighter holder, Transform slot, Transform animSlot, WeaponDefinition definition, Quaternion grip)
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
            dummyStance = (ClipFighter.Stances)(((int)dummyStance + 1) % 3);
            // (Made again: what it holds changes with it.)
            MakeDummy();
            legs.Add($"{time:0.00} s: the dummy {DummyStanceName}");
        }

        private string DummyStanceName => dummyStance == ClipFighter.Stances.Open ? "stands open"
            : dummyStance == ClipFighter.Stances.Shield ? "has a sword and shield" : "has a sword only";

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

        /// <summary>What the strike aimed last did, told (when it ends, or the combo's next takes over).</summary>
        private void FinishAim()
        {
            if (aiming == null)
            {
                return;
            }

            bool landed = hits != null && hits.Hits > aimHits, blocked = hits != null && hits.Blocks > aimBlocks;
            string result = landed || blocked ? hits.Last.Substring(hits.Last.IndexOf(':') + 2)
                : aimBest < float.MaxValue ? $"missed: the blade passed {aimBest * 100f:0} cm from where it was aimed" : "missed";
            lastAim = $"{aiming.Name}: {aimPlan}; {result}{((landed || blocked) && dummy != null && dummy.LastReaction.Length > 0 ? "; the dummy: " + dummy.LastReaction : "")}";
            legs.Add($"{time:0.00} s: {lastAim}");
            ShowAimMark(true, landed ? new Color(0.35f, 0.8f, 0.35f) : blocked ? new Color(0.35f, 0.55f, 0.9f) : new Color(0.9f, 0.35f, 0.3f));
            aiming = null;
        }

        /// <summary>A strike aimed at the dummy has started (alone, or in a combo): watched from now.</summary>
        private void BeganAim(LocoClip clip, Vector3 at)
        {
            FinishAim();
            aimPoint = at;
            if (dummy != null)
            {
                dummy.BlockAt = at;
            }

            aiming = clip;
            aimBest = float.MaxValue;
            aimHits = hits != null ? hits.Hits : 0;
            aimBlocks = hits != null ? hits.Blocks : 0;
            aimPlan = $"turn {poser.AimTurn:+0;-0}°, lunge x{poser.AimLunge:0.00}, the arm made up {poser.AimMissBeforePull:0.00} m";
            ShowAimMark(true, new Color(0.95f, 0.85f, 0.3f));
        }

        private string aimPlan = "";

        // ---- The weapon arm in a blow (as the fight holds it) --------------------------------------------

        // ---- Combos (the combat plan's step 5) ----------------------------------------------------------

        private ComboPlayer combo;
        private bool comboWas, pressingWas;
        private string lastCombo = "";

        /// <summary>The last combo's blows: when each first landed (seconds of the lab's clock), and which blow is playing.</summary>
        private readonly List<float> comboHits = new List<float>();
        private int comboBlow = -1, comboLanded = -2;

        /// <summary>The last combo's beat, as the panel shows it: hit to hit, and first contact to last.</summary>
        private string ComboBeat()
        {
            if (comboHits.Count < 2)
            {
                return comboHits.Count == 1 ? "Last combo: one blow landed" : "";
            }

            var gaps = new StringBuilder();
            for (int i = 1; i < comboHits.Count; i++)
            {
                gaps.Append(i > 1 ? ", " : "").Append((comboHits[i] - comboHits[i - 1]).ToString("0.00"));
            }

            return $"Last combo: {comboHits.Count} blows, hit to hit {gaps} s; first contact to last {comboHits[comboHits.Count - 1] - comboHits[0]:0.00} s";
        }

        /// <summary>This body's strikes, as clips.</summary>
        private List<LocoClip> Strikes()
        {
            var strikes = new List<LocoClip>();
            foreach (var name in CombatClips())
            {
                var clip = LocoLibrary.Get(name);
                if (clip != null && clip.Role == "strike")
                {
                    strikes.Add(clip);
                }
            }

            return strikes;
        }

        /// <summary>The combo the picked strike starts: up to three strikes, each the best follow-on of the last.</summary>
        private static string SkillName(float skill) => skill < 0.125f ? "novice" : skill < 0.375f ? "poor" : skill < 0.625f ? "fair" : skill < 0.875f ? "good" : "expert";

        private void SetShieldSkill(float skill)
        {
            dummyShieldSkill = Mathf.Clamp01(Mathf.Round(skill * 4f) / 4f);
            if (dummy != null)
            {
                dummy.ShieldSkill = dummyShieldSkill;
            }

            legs.Add($"{time:0.00} s: shield skill {dummyShieldSkill:0.00} ({SkillName(dummyShieldSkill)})");
        }

        private void SetParrySkill(float skill)
        {
            dummyParrySkill = Mathf.Clamp01(Mathf.Round(skill * 4f) / 4f);
            if (dummy != null)
            {
                dummy.ParrySkill = dummyParrySkill;
            }

            legs.Add($"{time:0.00} s: parry skill {dummyParrySkill:0.00} ({SkillName(dummyParrySkill)})");
        }

        /// <summary>The combo the picked strike is in (Bam! Bam! BAM!): it opens it, or a showstopper picked ends it (<see cref="StrikeCombos.CoreChain"/>).</summary>
        private List<StrikeLink> ComboFrom(LocoClip first) => first == null || first.ContactFrame < 0 ? new List<StrikeLink>()
            : combo != null && !combo.Cores ? StrikeCombos.Chain(first, Strikes(), 3) : StrikeCombos.CoreChain(first, Strikes(), 3, comboVariant);

        /// <summary>Which of the picked strike's combos as good as its best plays next (each I plays the next one round).</summary>
        private int comboVariant;

        private int ComboVariants(LocoClip first) => combo != null && combo.Cores && first != null && first.ContactFrame >= 0 ? StrikeCombos.CoreChains(first, Strikes()).Count : 1;

        /// <summary>A chain's opener (its first link's from; the picked strike when it has none).</summary>
        private static LocoClip Opener(LocoClip picked, List<StrikeLink> chain) => chain.Count > 0 ? chain[0].From : picked;

        private LocoClip PickedClip()
        {
            var combat = CombatClips();
            return combat.Count > 0 ? LocoLibrary.Get(combat[Mathf.Clamp(combatClip, 0, combat.Count - 1)]) : null;
        }

        /// <summary>Plays the picked strike's combo at the dummy (I), each strike aimed at the part nearest its own reach's height.</summary>
        private void PlayCombo()
        {
            var first = PickedClip();
            if (combo == null || course || first == null || first.ContactFrame < 0 || combo.Playing
                || (loco.Current == Locomotor.Mode.OneShot && loco.Shot != null && loco.Shot.ContactFrame >= 0))
            {
                return;
            }

            var chain = ComboFrom(first);
            combo.Keep = first;
            first = Opener(first, chain);
            comboHits.Clear();
            comboBlow = -1;
            comboLanded = -2;
            lastCombo = ComboText(first, chain);
            legs.Add($"{time:0.00} s: combo from {first.Name} ({(combo.Cores ? $"cores: {lastCombo}, each blow cut just after its contact and the next taken up just before its own, the last the showstopper" : $"planned {lastCombo}; each next strike is chosen as it comes, one that will land")})");
            combo.Start(first, chain);
            comboVariant++;
        }

        private static string ComboText(LocoClip first, List<StrikeLink> chain)
        {
            var text = new StringBuilder(first.Name);
            foreach (var link in chain)
            {
                text.Append($" > {link.To.Name} ({link.Cost:0.00})");
            }

            return text.ToString();
        }

        /// <summary>Where on the dummy a strike goes: its part nearest the height the strike reaches.</summary>
        private Vector3 TargetFor(LocoClip clip)
        {
            if (dummy == null || dummyHeight == AimLegs)
            {
                return DummyTarget;
            }

            float h = dummy.TargetAt(1).y;
            if (StrikeReach.TryGet(body.ToString(), clip.Name, out var grip, out var way))
            {
                h = (grip + way * (poser.WeaponLength * poser.SweetSpot)).y + origin.y;
            }

            int best = 0;
            for (int k = 1; k < 3; k++)
            {
                if (Mathf.Abs(dummy.TargetAt(k).y - h) < Mathf.Abs(dummy.TargetAt(best).y - h))
                {
                    best = k;
                }
            }

            return dummy.TargetAt(best);
        }

        /// <summary>How near the blade came to the dummy's head about the aimed strike's contact; told when it ends.</summary>
        private void WatchAim()
        {
            if (aiming == null)
            {
                return;
            }

            if (loco.Shot != aiming)
            {
                FinishAim();
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

        [Tooltip("Sparring (X): the walker keeps its guard up, faces the dummy and walks itself to where the picked strike reaches it; J or I strikes from there. W A S D still move it.")]
        public bool spar = true;

        /// <summary>Within this of its sparring spot (m at a Medium's size) and facing the dummy within 10 degrees, it is in position.</summary>
        public const float SparNear = 0.1f;

        private bool InPosition { get; set; }

        /// <summary>How near its target a strike planned from the standing distance must come to be taken from there (metres at a Medium's size): near enough to land on the body somewhere.</summary>
        public const float LandsNear = ClipFighter.LandsNear;

        /// <summary>Where the walker stands to strike the dummy with the picked strike (or the combo it starts): the strike's own reach from its target.</summary>
        private Vector3 SparSpot()
        {
            var clip = PickedClip();
            // (Where the combo the picked strike is in reaches from: its opener, taken up where the combo takes it up.
            // From the picked strike's own start, a combo opening with a lunge already part done stood too near and its
            // lunge was cut to a fifth.)
            float startFrom = 0f;
            if (combo != null && combo.Cores && clip != null && clip.ContactFrame >= 0)
            {
                clip = Opener(clip, ComboFrom(clip));
                startFrom = StrikeCombos.OpenerEntry(clip);
            }

            Vector3 dummyAtNow = dummy.Loco.Position;
            Vector3 target = clip != null && clip.ContactFrame >= 0 ? TargetFor(clip) : DummyTarget;
            float reach = 1.4f * size;
            if (clip != null && StrikeReach.TryGet(body.ToString(), clip.Name, out var grip, out var way))
            {
                reach = Flat(grip + way * (poser.WeaponLength * poser.SweetSpot));
            }

            Vector3 from = loco.Position - dummyAtNow;
            from.y = 0f;
            Vector3 dir = from.sqrMagnitude > 1e-4f ? from.normalized : Quaternion.Euler(0f, dummyFacing, 0f) * Vector3.forward;
            // (From the standing distance, the strike's lunge taking it in; a strike that can't reach from there
            // (a Small's, a short one) is stood in for, only as near as it needs, never nearer than the fighting distance.)
            float least = ClipFighter.FightingDistance(size, dummy.Size) + 0.1f * size, ownReach = reach;
            reach = Mathf.Max(reach, ClipFighter.StandingDistance(size, dummy.Size));
            Vector3 flatTarget = new Vector3(target.x, loco.Position.y, target.z);
            if (clip != null && clip.ContactFrame >= 0)
            {
                // (Else from its own reach, as near as the fighting distance lets it.)
                float faceIn = Mathf.Atan2(-dir.x, -dir.z) * Mathf.Rad2Deg;
                float own = Mathf.Max(ownReach, least), best = own;
                for (float d = reach; d > own; d -= 0.05f * size)
                {
                    if (poser.MissAfterPull(clip, target, startFrom, flatTarget + dir * d, faceIn) <= LandsNear * size)
                    {
                        best = d;
                        break;
                    }
                }

                reach = best;
            }

            return flatTarget + dir * Mathf.Max(reach, least);
        }

        private void ToggleSpar()
        {
            spar = !spar;
            legs.Add($"{time:0.00} s: sparring {(spar ? "on" : "off")}");
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
            if (spar && !course && dummy != null)
            {
                // (Sparring: the guard up, facing the dummy, walked to the picked strike's reach of it.)
                if (!guardUp)
                {
                    ToggleGuard();
                }

                Vector3 toDummy = dummy.Loco.Position - loco.Position;
                facing = Flat(toDummy) > 0.2f ? Mathf.Atan2(toDummy.x, toDummy.z) * Mathf.Rad2Deg : loco.Facing;
                Vector3 off = SparSpot() - loco.Position;
                off.y = 0f;
                bool driven = way.sqrMagnitude > 0.01f;
                InPosition = !driven && off.magnitude < SparNear * size && Mathf.Abs(Mathf.DeltaAngle(loco.Facing, facing)) < 10f;
                want = driven ? way * speed
                    : off.magnitude < 0.5f * SparNear * size ? Vector3.zero
                    : Vector3.ClampMagnitude(off * 2.5f, walkSpeed * PaceScale * 0.8f);
                return;
            }

            InPosition = false;
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
            if (SandboxInput.KeyPressed(KeyCode.Z)) { DummyStrikes(SandboxInput.KeyHeld(KeyCode.LeftShift)); }
            if (SandboxInput.KeyPressed(KeyCode.I)) { PlayCombo(); }
            if (SandboxInput.KeyPressed(KeyCode.X)) { ToggleSpar(); }
            if (SandboxInput.KeyPressed(KeyCode.T)) { ToggleGuard(); }
            if (SandboxInput.KeyPressed(KeyCode.N)) { body = body == BodySize.Small ? BodySize.Medium : body == BodySize.Medium ? BodySize.Large : BodySize.Small; Build(); }
            if (SandboxInput.KeyPressed(KeyCode.M)) { ResetPanel(); }
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

        /// <summary>How big the panel is drawn (its − and + buttons; kept between runs).</summary>
        private float uiScale = -1f;

        private const string UiScaleKey = "LocomotionLab.UiScale";

        /// <summary>Which of the panel's sections are open.</summary>
        private bool showBody = true, showWalk = false, showAttacker = true, showFootwork = true, showDefender = true, showLog = true;

        private GUIStyle wrapped, header;

        private void OnGUI()
        {
            if (loco == null)
            {
                return;
            }

            if (uiScale < 0f)
            {
                uiScale = PlayerPrefs.GetFloat(UiScaleKey, 1.5f);
            }

            // (The whole panel scaled: text and buttons alike.)
            var was = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(uiScale, uiScale, 1f));
            panel.width = 400f;
            panel = GUILayout.Window(7711, panel, DrawPanel, "Locomotion lab");
            // (Never dragged off the screen: its title bar stays where it can be grabbed.)
            float screenW = Screen.width / uiScale, screenH = Screen.height / uiScale;
            panel.x = Mathf.Clamp(panel.x, 60f - panel.width, Mathf.Max(0f, screenW - 60f));
            panel.y = Mathf.Clamp(panel.y, 0f, Mathf.Max(0f, screenH - 30f));
            GUI.matrix = was;
        }

        /// <summary>The panel back at the top left at its usual size (M): for when it was dragged away or scaled too big.</summary>
        private void ResetPanel()
        {
            uiScale = 1.5f;
            PlayerPrefs.SetFloat(UiScaleKey, uiScale);
            panel = new Rect(12, 12, 400, 10);
        }

        /// <summary>A section's heading, a button that opens and closes it.</summary>
        private bool Section(string title, ref bool open, string about = null)
        {
            header ??= new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold };
            if (GUILayout.Button((open ? "\u25BE  " : "\u25B8  ") + title, header))
            {
                open = !open;
            }

            if (open && about != null)
            {
                Note(about);
            }

            return open;
        }

        private void Note(string text, Color? color = null)
        {
            wrapped ??= new GUIStyle(GUI.skin.label) { wordWrap = true };
            var was = GUI.contentColor;
            if (color.HasValue)
            {
                GUI.contentColor = color.Value;
            }

            GUILayout.Label(text, wrapped);
            GUI.contentColor = was;
        }

        private void DrawPanel(int id)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Panel size {uiScale:0.0}x");
            if (GUILayout.Button("\u2212", GUILayout.Width(36f)))
            {
                uiScale = Mathf.Max(0.8f, uiScale - 0.1f);
                PlayerPrefs.SetFloat(UiScaleKey, uiScale);
            }

            if (GUILayout.Button("+", GUILayout.Width(36f)))
            {
                uiScale = Mathf.Min(3f, uiScale + 0.1f);
                PlayerPrefs.SetFloat(UiScaleKey, uiScale);
            }

            GUILayout.EndHorizontal();
            Note("Lost the panel? M puts it back here.");

            // ---- Body
            if (Section($"Body: {body}", ref showBody))
            {
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
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Restart (R)")) Build();
                if (GUILayout.Button(paused ? "Play (P)" : "Pause (P)")) paused = !paused;
                if (GUILayout.Button(physical ? "Physical (B)" : "Kinematic (B)")) { physical = !physical; Build(); }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Time");
                foreach (float sp in new[] { 0.25f, 0.5f, 1f })
                {
                    if (GUILayout.Button(sp.ToString("0.##") + "x"))
                    {
                        Time.timeScale = sp;
                    }
                }

                GUILayout.EndHorizontal();
                if (loadouts.Count > 0 && GUILayout.Button($"Carrying: {loadouts[loadoutNow].name} (L)"))
                {
                    NextLoadout();
                }

                if (GUILayout.Button("Watch the fight (F)"))
                {
                    WatchFight();
                    return;
                }

                Note($"{loco.Current}{(loco.Shot != null ? " " + loco.Shot.Name : "")}, {new Vector2(loco.Velocity.x, loco.Velocity.z).magnitude:0.00} m/s");
                if (down > 0f)
                {
                    Note(note + $" (up again in {down:0.0} s)", new Color(1f, 0.6f, 0.4f));
                }
            }

            // ---- Walk
            if (Section("Walk", ref showWalk, course ? $"Course: {leg}" : "W A S D drive, Shift runs, Space holds the facing, V the walking speed."))
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(course ? "Drive (C)" : "Course (C)")) { course = !course; Build(); }
                if (GUILayout.Button(guardUp ? "Guard down (T)" : "Guard up (T)")) ToggleGuard();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Shove (G)")) Shove();
                if (GUILayout.Button("Hard (K)")) Shove(2.5f);
                if (GUILayout.Button("Ball (H)")) Throw();
                GUILayout.EndHorizontal();
                footprints = GUILayout.Toggle(footprints, "Footprints");
                Note($"Walking {walkSpeed * PaceScale:0.0} m/s, running {RunSpeed:0.0} m/s; run {loco.RunWeight:0.00}, phase {loco.Phase:0.00}, guard {loco.GuardWeight:0.0}.");
                Note($"Planted feet moved at most {measures.PlantDriftWorst * 100f:0.0} cm over {measures.PlantsDone} plants; {measures.ForcedReleases} forced steps.");
            }

            var combat = CombatClips();
            if (course)
            {
                // (On the walking course there is no dummy: the attacker and the defender wait for driving.)
                if (Section("Attacker and defender", ref showAttacker, "On the walking course there is no dummy to strike. Leave the course to strike, spar and set up the dummy."))
                {
                    if (GUILayout.Button("Leave the course: drive and strike (C)"))
                    {
                        course = false;
                        Build();
                    }
                }
            }
            else if (combat.Count > 0)
            {
                // ---- Attacker
                combatClip = Mathf.Clamp(combatClip, 0, combat.Count - 1);
                var shown = LocoLibrary.Get(combat[combatClip]);
                if (Section("Attacker", ref showAttacker))
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("<  (,)", GUILayout.Width(70f))) StepCombatClip(-1);
                    if (GUILayout.Button($"Strike: {combat[combatClip].Replace(LocoLibrary.CombatPrefix(body.ToString()), "")} (J)")) PlayCombatClip();
                    if (GUILayout.Button("(.)  >", GUILayout.Width(70f))) StepCombatClip(1);
                    GUILayout.EndHorizontal();
                    if (shown != null)
                    {
                        string reach = "";
                        if (shown.ContactFrame >= 0 && StrikeReach.TryGet(body.ToString(), shown.Name, out var grip, out var way))
                        {
                            Vector3 sweet = grip + way * (poser.WeaponLength * poser.SweetSpot);
                            reach = $"; reaches {Flat(sweet):0.0} m ahead, {sweet.y:0.0} m up{(Mathf.Abs(sweet.x) > 0.3f ? (sweet.x > 0f ? ", to the right" : ", to the left") : "")}";
                        }

                        Note($"{shown.Kind}{(shown.ContactFrame >= 0 ? $", contact at {shown.ContactFrame / shown.Rate:0.00} s" : "")}{reach}");
                        if (shown.ContactFrame >= 0)
                        {
                            var chain = ComboFrom(shown);
                            int ways = ComboVariants(shown);
                            if (GUILayout.Button(chain.Count > 0 ? $"Combo (I): {ComboText(Opener(shown, chain), chain).Replace(LocoLibrary.CombatPrefix(body.ToString()), "")}{(ways > 1 ? $" ({comboVariant % ways + 1} of {ways})" : "")}" : "Combo (I): no follow-on"))
                            {
                                PlayCombo();
                            }
                        }
                    }

                    string beat = ComboBeat();
                    if (beat.Length > 0)
                    {
                        Note(beat);
                    }

                    GUILayout.BeginHorizontal();
                    streaks.On = GUILayout.Toggle(streaks.On, "Blade streaks");
                    GUILayout.Label($"{streaks.Lifetime:0.00} s");
                    if (GUILayout.Button("\u2212", GUILayout.Width(36f))) streaks.Lifetime = Mathf.Max(0.04f, streaks.Lifetime - 0.02f);
                    if (GUILayout.Button("+", GUILayout.Width(36f))) streaks.Lifetime = Mathf.Min(0.5f, streaks.Lifetime + 0.02f);
                    GUILayout.EndHorizontal();

                    if (GUILayout.Button(spar ? $"Sparring (X): on, {(InPosition ? "in position" : "moving into position")}" : "Sparring (X): off, driven by hand"))
                    {
                        ToggleSpar();
                    }

                    if (walker != null && walker.Stance != ClipFighter.Stances.Open)
                    {
                        // (Its own defence against the dummy's blows, as its loadout has it.)
                        bool shielded = walker.Stance == ClipFighter.Stances.Shield;
                        float skill = shielded ? walkerShieldSkill : walkerParrySkill;
                        GUILayout.BeginHorizontal();
                        GUILayout.Label($"Its {(shielded ? "shield" : "parry")} skill {skill:0.00} ({SkillName(skill)})");
                        if (GUILayout.Button("\u2212", GUILayout.Width(36f))) { if (shielded) SetWalkerShieldSkill(skill - 0.25f); else SetWalkerParrySkill(skill - 0.25f); }
                        if (GUILayout.Button("+", GUILayout.Width(36f))) { if (shielded) SetWalkerShieldSkill(skill + 0.25f); else SetWalkerParrySkill(skill + 0.25f); }
                        GUILayout.EndHorizontal();
                    }
                    else if (walker != null)
                    {
                        Note("No defence of its own (a club or bare hands): the dummy's blows land.");
                    }

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button($"Aim: {DummyHeightNames[dummyHeight]} (U)")) NextDummyHeight();
                    if (GUILayout.Button("Dummy in reach (Y)")) DummyInReach();
                    GUILayout.EndHorizontal();
                    if (lastAim.Length > 0)
                    {
                        // (Green it landed, blue it was blocked or parried, red it missed.)
                        var colour = lastAim.Contains("missed") ? new Color(1f, 0.5f, 0.45f)
                            : lastAim.Contains("blocked") || lastAim.Contains("parried") ? new Color(0.55f, 0.75f, 1f) : new Color(0.55f, 0.95f, 0.55f);
                        Note("Last: " + lastAim, colour);
                    }
                }

                // ---- Footwork
                if (Section("Footwork (both steer themselves)", ref showFootwork))
                {
                    if (GUILayout.Button(footwork ? "Footwork: on (W A S D off)" : "Footwork: off"))
                    {
                        ToggleFootwork();
                    }

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button($"Walker: {walkerTempo}"))
                    {
                        walkerTempo = NextTempo(walkerTempo);
                        if (walkerFeet != null) walkerFeet.Mode = walkerTempo;
                        legs.Add($"{time:0.00} s: the walker's tempo {walkerTempo}");
                    }

                    if (GUILayout.Button($"Dummy: {dummyTempo}"))
                    {
                        dummyTempo = NextTempo(dummyTempo);
                        if (dummyFeet != null) dummyFeet.Mode = dummyTempo;
                        legs.Add($"{time:0.00} s: the dummy's tempo {dummyTempo}");
                    }

                    GUILayout.EndHorizontal();
                    Note("Probe: at its striking distance. Gather: just out of the other's reach. Press: closes in. Shell: gives ground.");
                    if (footwork && walkerFeet != null && dummyFeet != null && dummy != null)
                    {
                        Note($"{Flat(dummy.Loco.Position - loco.Position):0.00} m apart. The walker: {walkerFeet.Doing} (wants {walkerFeet.Want:0.00}). The dummy: {dummyFeet.Doing} (wants {dummyFeet.Want:0.00}).");
                    }
                }

                // ---- Defender
                if (Section("Defender (the dummy)", ref showDefender))
                {
                    GUILayout.BeginHorizontal();
                    foreach (var st in new[] { ClipFighter.Stances.Open, ClipFighter.Stances.Shield, ClipFighter.Stances.Sword })
                    {
                        string name = st == ClipFighter.Stances.Open ? "Open" : st == ClipFighter.Stances.Shield ? "Sword + shield" : "Sword only";
                        if (GUILayout.Button(dummyStance == st ? "> " + name + " <" : name) && dummyStance != st)
                        {
                            dummyStance = st;
                            MakeDummy();
                            legs.Add($"{time:0.00} s: the dummy {DummyStanceName}");
                        }
                    }

                    GUILayout.EndHorizontal();
                    Note(dummyStance == ClipFighter.Stances.Open ? "Open: its guard down, no defence; every blow lands. (O cycles the three.)"
                        : dummyStance == ClipFighter.Stances.Shield ? "Blocks each blow with its shield. (O cycles the three.)"
                        : $"Parries each blow with its sword from a medium guard. (O cycles the three.)");
                    if (dummyStance == ClipFighter.Stances.Shield)
                    {
                        // (Shield skill, 0 a novice to 1 an expert: how soon it moves, how well it reads the line, how fast its
                        // shield comes and turns, whether it covers low cuts and slants the shield so a cut glances off.)
                        GUILayout.BeginHorizontal();
                        GUILayout.Label($"Shield skill {dummyShieldSkill:0.00} ({SkillName(dummyShieldSkill)})");
                        if (GUILayout.Button("\u2212", GUILayout.Width(36f))) SetShieldSkill(dummyShieldSkill - 0.25f);
                        if (GUILayout.Button("+", GUILayout.Width(36f))) SetShieldSkill(dummyShieldSkill + 0.25f);
                        GUILayout.EndHorizontal();
                    }

                    if (dummyStance == ClipFighter.Stances.Sword)
                    {
                        // (Parry skill, 0 a novice to 1 an expert: how soon it moves, how well it reads the blow and meets the blade.)
                        GUILayout.BeginHorizontal();
                        GUILayout.Label($"Parry skill {dummyParrySkill:0.00} ({SkillName(dummyParrySkill)})");
                        if (GUILayout.Button("\u2212", GUILayout.Width(36f))) SetParrySkill(dummyParrySkill - 0.25f);
                        if (GUILayout.Button("+", GUILayout.Width(36f))) SetParrySkill(dummyParrySkill + 0.25f);
                        GUILayout.EndHorizontal();
                        if (dummy?.Parry != null && dummy.Parry.BlowsRead > 0)
                        {
                            Note($"Read {dummy.Parry.BlowsRead} blows, took the wrong parry for {dummy.Parry.WrongPicks}");
                        }
                    }

                    if (dummy != null)
                    {
                        dummyFightsBack = GUILayout.Toggle(dummyFightsBack, "Fights back: answers each of your blows");
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("Strike (Z)")) DummyStrikes(false);
                        if (GUILayout.Button("Combo (Shift+Z)")) DummyStrikes(true);
                        GUILayout.EndHorizontal();
                        if (dummy.Strikes != null && (dummy.Strikes.Hits + dummy.Strikes.Blocks + dummy.Strikes.Parries) > 0)
                        {
                            Note($"Its blows: {dummy.Strikes.Hits} hits, {dummy.Strikes.Blocks} blocked, {dummy.Strikes.Parries} parried");
                        }

                        dummy.Anchored = GUILayout.Toggle(dummy.Anchored, "Anchored (a training post: not driven back)");
                        Note($"{Flat(DummyTarget - loco.Position):0.0} m away, struck at {DummyTarget.y - origin.y:0.0} m; {(hits != null ? $"{hits.Hits} hits, {hits.Blocks} blocked, {hits.Parries} parried" : "")}");
                        if (dummy.Parry != null)
                        {
                            Note($"Now: {dummy.Parry.CurrentName}");
                        }

                        if (dummy.LastReaction.Length > 0)
                        {
                            Note($"Took the last blow: {dummy.LastReaction}");
                        }
                    }
                }
            }

            // ---- Log
            if (Section("Log (newest first)", ref showLog))
            {
                for (int i = legs.Count - 1, n = 0; i >= 0 && n < 8; i--, n++)
                {
                    Note(legs[i]);
                }
            }

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
            walker?.Destroy();
            walker = null;
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
