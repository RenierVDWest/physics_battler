using PhysicsBattler.Combat;
using PhysicsBattler.Locomotion;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Units
{
    /// <summary>
    /// The plan "Bringing the lab into the fight", step 4: a fight unit moved by the lab's clip fighter
    /// (<see cref="ClipFighter"/>: the walk, the guard, the body held to its pose) instead of the old pose pipeline,
    /// gait, balance and stance. The unit keeps what the fight reads of it: its health and poise, its team, what it
    /// sees and knows, its tempo and rhythm. The fight loop steers it as a velocity and a facing
    /// (<see cref="ClipWant"/>, <see cref="FacePoint"/>), not by moving its root; its root follows the walk. Blows
    /// are held off in this step (<see cref="ClipBlows"/>).
    /// </summary>
    public partial class UnitController
    {
        /// <summary>Fight units are clip fighters (headless: <c>CLIP_FIGHT=1</c>; in Unity, the sandbox's "Clip motion").</summary>
        public static bool ClipMotion = System.Environment.GetEnvironmentVariable("CLIP_FIGHT") != null;

        /// <summary>A clip fighter's body is physical (off headless, where there is no physics: the lab's runners have it so).</summary>
        public static bool ClipPhysical = true;

        /// <summary>Clip fighters strike (step 5 of the plan); off, they only move.</summary>
        public static bool ClipBlows = false;

        /// <summary>The clip fighter moving this unit, or null (the old pose path).</summary>
        public ClipFighter Clip { get; private set; }

        /// <summary>Moved by a clip fighter.</summary>
        public bool UsesClips => Clip != null;

        /// <summary>The walk the fight loop wants of it (world, m/s, flat): its steering, instead of moving its root.</summary>
        public Vector3 ClipWant { get; set; }

        /// <summary>How hard the fight loop is pushing it off other bodies (0 to 1 of its walking pace): its circling gives way to it.</summary>
        public float ClipCrowded { get; set; }

        /// <summary>Its circling pace in the guard (m/s at a Medium's size), and how long it goes round, and stands, between turns (seconds).</summary>
        public const float ClipCircleSpeed = 0.35f;

        private static readonly Vector2 ClipCircleFor = new Vector2(1.5f, 3.5f), ClipStandFor = new Vector2(0.8f, 2.2f);

        private float circleSide = 1f, circleLeft;
        private bool circleStanding;
        private System.Random circleLuck;

        private bool InitializeClips(bool asPhysical)
        {
            LocoLibrary.Load();
            float facing = transform.eulerAngles.y;
            float groundY = transform.position.y;
            string body = string.IsNullOrEmpty(profile.body) ? "Medium" : profile.body;
            bool physicalBody = asPhysical && ClipPhysical;
            Clip = new ClipFighter(gameObject, Rig, profile, transform.position, facing, groundY, LocoLibrary.CombatPrefix(body), physicalBody);
            if (!Clip.Loco.Ready)
            {
                Debug.LogError($"[Unit] {name}: the clip library (Resources/Locomotion/LocoClips.json) is missing clips.", this);
                enabled = false;
                return false;
            }

            Anim = Clip.Anim;
            Authority = Clip.Authority;
            Body = BodyMeasure.Of(transform, Anim, Rig);
            Loadout = new Loadout { Scale = Body.SizeFactor };
            Clip.Poser.BodyKey = body;
            Clip.Poser.TwoHanded = LocoLibrary.TwoHanded(body);
            Clip.Stance = ClipFighter.Stances.Open;

            // (The root follows the walk: the pose and the physical parts live beside it, so moving it drags neither.)
            var container = new GameObject(name + " (body)").transform;
            container.SetParent(physical ? BodiesContainer : transform.parent, false);
            detachedBody = container.gameObject;
            Anim.Root.SetParent(container, true);
            foreach (var id in PartInfo.All)
            {
                Rig.Part(id).transform.SetParent(container, true);
            }

            int seed = 7919 * team + 104729 * ++spawnCount;
            unitNumber = spawnCount;
            circleLuck = new System.Random(seed + 5);
            circleSide = (unitNumber % 2) == 0 ? 1f : -1f;
            circleLeft = Next(ClipCircleFor);
            InitializeCombat(seed);
            Stance = new Stance(seed);
            Stance.Configure(Style, Body);
            Stance.GroundY = groundY;
            Stance.Plant(transform.position, facing);
            InitializeRhythm(seed);
            InitializeTempo(seed);
            InitializePerception(seed);
            facingYaw = facing;
            initialized = true;
            if (physical)
            {
                all.Add(this);
                Initialized?.Invoke(this);
            }

            return true;
        }

        private float Next(Vector2 range) => Mathf.Lerp(range.x, range.y, (float)circleLuck.NextDouble());

        /// <summary>
        /// The clip fighter's step: what it sees, its tempo, then its walk toward what the fight loop wants (and round
        /// its opponent, once near), facing the point the loop gave it; the root put where the walk is.
        /// </summary>
        private void StepClips(float dt)
        {
            if (!IsAlive)
            {
                return;
            }

            LocalTime += dt;
            StateTime += dt;
            // (Knocked off its feet (a bump in the physical body, as the lab's fighters can be), the fight sees it down
            // until its own get-up has it standing: not steered, walked round.)
            var footing = Clip.Footing == ClipFighter.Footings.Down ? UnitState.Down : Clip.Footing == ClipFighter.Footings.GettingUp ? UnitState.GettingUp : UnitState.Standing;
            if (footing != State)
            {
                State = footing;
                StateTime = 0f;
            }

            UpdatePerception();
            UpdateClipTempo(dt);

            var loco = Clip.Loco;
            Vector3 want = ClipWant;
            want.y = 0f;
            want += ClipCircling(dt);
            float yaw = loco.Facing;
            if (facePoint.HasValue)
            {
                Vector3 to = facePoint.Value - loco.Position;
                to.y = 0f;
                if (to.sqrMagnitude > 1e-4f)
                {
                    yaw = Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg;
                }
            }

            Vector3 opponent = Opponent != null ? Opponent.Root.position : loco.Position + Quaternion.Euler(0f, loco.Facing, 0f) * Vector3.forward * 2f;
            Clip.Move(dt, want, yaw, opponent, false);
            Clip.Apply(dt);
            facingYaw = loco.Facing;
            transform.SetPositionAndRotation(new Vector3(loco.Position.x, Stance.GroundY, loco.Position.z), Quaternion.Euler(0f, loco.Facing, 0f));
        }

        /// <summary>
        /// Once near its opponent and free (<see cref="CircleOpponent"/>), it goes round it sideways in spells, standing
        /// between them, now one way, now the other, as the lab's footwork does (<see cref="Footwork"/>).
        /// </summary>
        private Vector3 ClipCircling(float dt)
        {
            if (!CircleOpponent || Opponent == null || !Opponent.IsAlive)
            {
                return Vector3.zero;
            }

            circleLeft -= dt;
            if (circleLeft <= 0f)
            {
                circleStanding = !circleStanding;
                circleLeft = Next(circleStanding ? ClipStandFor : ClipCircleFor);
                if (!circleStanding && circleLuck.NextDouble() < 0.5)
                {
                    circleSide = -circleSide;
                }
            }

            if (circleStanding)
            {
                return Vector3.zero;
            }

            Vector3 to = Opponent.Root.position - Clip.Loco.Position;
            to.y = 0f;
            if (to.sqrMagnitude < 1e-4f)
            {
                return Vector3.zero;
            }

            return Vector3.Cross(Vector3.up, to.normalized) * (circleSide * ClipCircleSpeed * Mathf.Sqrt(Body.SizeFactor) * Mathf.Clamp01(1f - 2f * ClipCrowded));
        }

        /// <summary>The tempo's step (as <see cref="UpdateTempo"/>, without the old pose's mood and the rhythm's postures).</summary>
        private void UpdateClipTempo(float dt)
        {
            if (Tempo == null || Rhythm == null || !TempoOn)
            {
                return;
            }

            bool walking = Clip.Loco.Current == Locomotor.Mode.Cycle;
            Tempo.Update(dt, new TempoInput
            {
                energy = Rhythm.Energy,
                aggression = Rhythm.Aggression,
                canAct = CanAct && IsAlive,
                attacking = CurrentMove != null,
                walking = walking,
                engaged = TempoEngaged,
                targetVulnerable = TargetVulnerable,
            });
            Rhythm.Recharge(Tempo.Regen * dt);
        }

        /// <summary>
        /// Weapons in a clip fighter's hands, as the lab holds them: the weapon's look on the hand's slot (its colliders
        /// triggers, never pushing its own body), the grip, what the arms carry, and its defence by its loadout.
        /// </summary>
        private void EquipClips(WeaponDefinition main, WeaponDefinition off)
        {
            Loadout.Main = main;
            Loadout.Off = off;
            Clip.ShieldColliders.Clear();
            Hold(Rig.SlotWeaponR, Anim.SlotWeaponR, main, main != null && !main.IsShield ? MainGrip : Quaternion.identity);
            Hold(Rig.SlotWeaponL, Anim.SlotWeaponL, off, off != null && off.IsShield ? ShieldGrip : Quaternion.identity);
            string body = Clip.Poser.BodyKey;
            bool oneHanded = main != null && (main.kind == WeaponKind.Sword || main.kind == WeaponKind.Club) && !LocoLibrary.TwoHanded(body);
            Clip.Stance = off != null && off.IsShield ? ClipFighter.Stances.Shield
                : oneHanded && off == null ? ClipFighter.Stances.Sword : ClipFighter.Stances.Open;
            var poser = Clip.Poser;
            poser.CarryR = main == null ? LocoBody.Carry.Free : main.IsShield ? LocoBody.Carry.Shield : LocoBody.Carry.Weapon;
            poser.CarryL = off == null ? LocoBody.Carry.Free : off.IsShield ? LocoBody.Carry.Shield : LocoBody.Carry.Weapon;
            poser.BladeR = (Anim.SlotWeaponR != null ? Anim.SlotWeaponR.localRotation : Quaternion.identity) * Vector3.forward;
            poser.BladeL = (Anim.SlotWeaponL != null ? Anim.SlotWeaponL.localRotation : Quaternion.identity) * Vector3.forward;
            float scale = Clip.Size;
            poser.WeaponLength = main != null && !main.IsShield ? main.length * scale : 0.9f * scale;
            Clip.Parry = Clip.Stance == ClipFighter.Stances.Sword
                ? HeldAs(new SwordParry(Anim, Clip.Loco, Rig, scale, poser.WeaponLength), main)
                : null;
        }

        private static SwordParry HeldAs(SwordParry parry, WeaponDefinition weapon) =>
            weapon != null && weapon.kind == WeaponKind.Club ? parry.AsClub() : parry;

        private void Hold(Transform slot, Transform animSlot, WeaponDefinition definition, Quaternion grip)
        {
            if (slot == null)
            {
                return;
            }

            // (What the hand held before goes.)
            for (int i = slot.childCount - 1; i >= 0; i--)
            {
                var child = slot.GetChild(i).gameObject;
                if (Application.isPlaying)
                {
                    Destroy(child);
                }
                else
                {
                    DestroyImmediate(child);
                }
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
            held.transform.localScale = Vector3.one * Clip.Size;
            foreach (var behaviour in held.GetComponentsInChildren<MonoBehaviour>(true))
            {
                Destroy(behaviour);
            }

            foreach (var collider in held.GetComponentsInChildren<Collider>(true))
            {
                collider.isTrigger = true;
                foreach (var part in Rig.Parts)
                {
                    if (part.partCollider != null)
                    {
                        Physics.IgnoreCollision(collider, part.partCollider, true);
                    }
                }

                if (definition.IsShield)
                {
                    Clip.ShieldColliders.Add(collider);
                }
            }
        }
    }
}
