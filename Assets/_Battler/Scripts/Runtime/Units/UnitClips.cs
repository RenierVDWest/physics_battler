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

        /// <summary>Clip fighters strike (step 5 of the plan); off, they only move (step 4).</summary>
        public static bool ClipBlows = System.Environment.GetEnvironmentVariable("CLIP_NO_BLOWS") == null;

        /// <summary>A clip fighter started a strike or a combo (what, at whom): the logs and the runners count blows by it, a clip strike having no <see cref="MoveState"/>.</summary>
        public static event System.Action<UnitController, string, UnitController> ClipStrikeStarted;

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

        private static readonly System.Collections.Generic.List<ClipFighter> clipBodies = new System.Collections.Generic.List<ClipFighter>();
        private float clipStopLeft;
        private int clipVariant;

        /// <summary>Where one of its parts is now: a clip fighter's as it has it (posed, or its body on the ground), else the physical part's.</summary>
        public Vector3 PartPosition(PartId id) => Clip != null ? Clip.PartAt(id) : Rig.Part(id).transform.position;

        /// <summary>It doesn't swing with an ally nearer than the fighting distance within this of the line to its target (degrees).</summary>
        public const float ClipAllyAngle = 70f;

        private static readonly bool NoKeepApart = System.Environment.GetEnvironmentVariable("CLIP_NO_KEEPAPART") != null;

        /// <summary>Two of a side stand no nearer than this many times their two bodies' radii together (root to root).</summary>
        public const float ClipAllyGap = 1.6f;

        /// <summary>Who its strike playing is aimed at (its arm's pull follows them).</summary>
        public UnitController ClipTarget { get; private set; }

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
            Clip.FightUnit = this;
            Clip.Health = profile.maxHealth;
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
                // (Dead: its body lies where it fell, its place with it.)
                Clip.Move(dt, Vector3.zero, Clip.Loco.Facing, Clip.Loco.Position, false);
                transform.position = new Vector3(Clip.Loco.Position.x, Stance.GroundY, Clip.Loco.Position.z);
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
            UpdateClipThreats();

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

            var against = Watched ?? Opponent;
            Vector3 opponent = against != null ? against.Root.position : loco.Position + Quaternion.Euler(0f, loco.Facing, 0f) * Vector3.forward * 2f;
            if (Clip.Parry != null && against != null && against.ClipShape != null)
            {
                Clip.Parry.Opponent = against.ClipShape;
            }
            // (Hit stop: both stand still a moment as a blow lands, the one it landed from and the one it landed on.)
            float ownStop = Clip.Strikes != null ? Clip.Strikes.HitStop : 0f;
            clipStopLeft = Mathf.Max(0f, clipStopLeft - dt);
            HitStop = Mathf.Max(ownStop, clipStopLeft);
            bool frozen = HitStop > 0f;
            Clip.Move(dt, want, yaw, opponent, frozen);
            if (Clip.Strikes != null)
            {
                // (Its blade sweeps every other body on the board: its target, the others in the way, its own side.)
                clipBodies.Clear();
                foreach (var other in all)
                {
                    if (other != this && other != null && other.Clip != null)
                    {
                        clipBodies.Add(other.Clip);
                    }
                }

                Clip.Strikes.Step(dt, clipBodies);
            }

            // (Bodies never pass through each other: the lab's rule, each pair once a step. Two who fight keep its least gap;
            // two of a side stand side by side.)
            foreach (var other in all)
            {
                if (NoKeepApart || other == null || other.Clip == null || other.unitNumber <= unitNumber || !other.IsAlive)
                {
                    continue;
                }

                float least = other.Team == Team
                    ? ClipAllyGap * (Battle.FightLoop.BodyRadius(this) + Battle.FightLoop.BodyRadius(other))
                    : ClipFighter.LeastGap * ClipFighter.FightingDistance(Clip.Size, other.Clip.Size);
                KeepClear(other, least);
            }

            Poise = Clip.Poise;
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
                attacking = IsAttacking,
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

            // (Its shape, for the parries of those it fights: its body, shield and blade, kept off and met.)
            ClipShape = new OpponentShape(Anim, scale, poser.CarryL == LocoBody.Carry.Shield ? Rig.SlotWeaponL : null, 0.27f)
            {
                WeaponSlot = poser.CarryR == LocoBody.Carry.Weapon ? Rig.SlotWeaponR : null,
                WeaponLength = poser.WeaponLength,
            };

            // (Its strikes, the lab's for its body, swept from its weapon: step 5 of the plan.)
            var hits = Clip.EnableStrikes(main != null && !main.IsShield ? main : null);
            hits.Comboing = () => Clip.Combo != null && Clip.Combo.Playing && Clip.Combo.Cores;
            hits.Struck += OnClipStruck;
            hits.IsAlly = body => body != null && body.FightUnit != null && body.FightUnit != this && body.FightUnit.Team == Team;
            Clip.Follow = clip => ClipTarget != null && ClipTarget.Clip != null && ClipTarget.IsAlive ? Clip.AimAt(ClipTarget.Clip, clip) : (Vector3?)null;
        }

        private static SwordParry HeldAs(SwordParry parry, WeaponDefinition weapon) =>
            weapon != null && weapon.kind == WeaponKind.Club ? parry.AsClub() : parry;

        // ---- The plan "Fighting off several attackers", steps 2 and 3: every blow coming, and the one to meet ----------

        /// <summary>A blow coming at it: whose, its read, whether it has seen it, and since when.</summary>
        public sealed class ClipThreat
        {
            public UnitController Attacker;
            public BlowRead Read;
            public int Serial = -1;
            public bool Seen;
            public float SeenAt = -1f, StartedAt;
            public bool Fresh;
        }

        private readonly System.Collections.Generic.List<ClipThreat> clipThreats = new System.Collections.Generic.List<ClipThreat>();

        /// <summary>The blows coming at it now (from every attacker striking at it).</summary>
        public System.Collections.Generic.IReadOnlyList<ClipThreat> ClipThreats => clipThreats;

        /// <summary>The attacker whose blow it is meeting, if any.</summary>
        public UnitController Watched { get; private set; }

        /// <summary>Its shape (body, shield, blade), for the parries of those who fight it.</summary>
        public OpponentShape ClipShape { get; private set; }

        /// <summary>A blow it is meeting is given up for another only when that one lands this much sooner (seconds).</summary>
        public const float ClipSwitchSooner = 0.12f;

        /// <summary>Its guard reaches a blow from this far round from where it faces (degrees, each way): beyond, it must turn first.</summary>
        public const float ClipGuardArc = 100f;

        /// <summary>
        /// Every blow coming at it, from every enemy striking at it (the old fight's threats, for clip blows): each kept
        /// with its read, seen once it is in view (or felt, right beside it); and of those it has seen and its guard can
        /// reach, the one landing first is the one it meets, given up only for one landing clearly sooner.
        /// </summary>
        private void UpdateClipThreats()
        {
            foreach (var t in clipThreats)
            {
                t.Attacker = t.Attacker != null && t.Attacker.IsAlive ? t.Attacker : null;
            }

            foreach (var other in all)
            {
                if (other == null || other == this || other.Team == Team || !other.IsAlive || other.Clip == null || other.Clip.Strikes == null
                    || other.ClipTarget != this || !other.Clip.Striking || !other.Clip.Strikes.Read(out var read))
                {
                    continue;
                }

                var threat = clipThreats.Find(t => t.Attacker == other);
                if (threat == null)
                {
                    threat = new ClipThreat { Attacker = other };
                    clipThreats.Add(threat);
                }

                if (threat.Serial != read.Serial)
                {
                    threat.Serial = read.Serial;
                    threat.Seen = false;
                    threat.SeenAt = -1f;
                    threat.StartedAt = LocalTime;
                }

                threat.Read = read;
                threat.Fresh = true;
                if (!threat.Seen && Perception.CanReact(SightOf(other)))
                {
                    // (Seen: it feels the threat, as the old fight's units do.)
                    threat.Seen = true;
                    threat.SeenAt = LocalTime;
                    Rhythm?.NoteThreat();
                    Tempo?.NoteIncoming(FromPress(other));
                }
            }

            clipThreats.RemoveAll(t => t.Attacker == null || !t.Fresh);
            foreach (var t in clipThreats)
            {
                t.Fresh = false;
            }

            // (The one to meet: seen, within its guard's reach, landing first.)
            ClipThreat meet = null, now = null;
            Vector3 ahead = Quaternion.Euler(0f, Clip.Loco.Facing, 0f) * Vector3.forward;
            foreach (var t in clipThreats)
            {
                if (t.Attacker == Watched)
                {
                    now = t;
                }

                Vector3 from = t.Attacker.Root.position - Root.position;
                from.y = 0f;
                if (!t.Seen || Vector3.Angle(ahead, from) > ClipGuardArc)
                {
                    continue;
                }

                if (meet == null || t.Read.ToContact < meet.Read.ToContact)
                {
                    meet = t;
                }
            }

            if (now != null && now.Seen && meet != null && meet != now && meet.Read.ToContact > now.Read.ToContact - ClipSwitchSooner)
            {
                meet = now;
            }

            Watched = meet?.Attacker;
            Clip.Watch(meet != null ? meet.Read : (BlowRead?)null);
        }

        /// <summary>
        /// Two bodies nearer than <paramref name="least"/> (root to root) are parted the way the feet allow: one walking or
        /// standing is pushed off, its walk taking the steps; one lunging into the other has the rest of its lunge cut;
        /// only one already deep in the other (inside <see cref="ClipDeepIn"/> of it) is moved straight out, feet and all.
        /// Moved straight out every time, the planted feet dragged up to 36 cm under the lunges.
        /// </summary>
        private void KeepClear(UnitController other, float least)
        {
            if (Clip.Fallen || other.Clip.Fallen)
            {
                return;
            }

            Vector3 gap = other.Clip.Loco.Position - Clip.Loco.Position;
            gap.y = 0f;
            float apart = gap.magnitude;
            if (apart >= least)
            {
                return;
            }

            if (apart < ClipDeepIn * least)
            {
                ClipDeepMoves++;
                ClipDeepMoved += ClipDeepIn * least - apart;
                ClipFighter.KeepApart(Clip, other.Clip, ClipDeepIn * least);
            }

            Vector3 way = apart > 1e-4f ? gap / apart : Quaternion.Euler(0f, Clip.Loco.Facing, 0f) * Vector3.forward;
            PartFrom(this, -way, least - apart);
            PartFrom(other, way, least - apart);
        }

        private static void PartFrom(UnitController u, Vector3 away, float into)
        {
            var loco = u.Clip.Loco;
            if (loco.Current == Locomotor.Mode.OneShot)
            {
                // (Lunging into the other: its root goes no further.)
                if (Vector3.Dot(loco.Velocity, away) < -0.05f)
                {
                    loco.StopShotTravel();
                }

                return;
            }

            if (u.LocalTime - u.clipPushedAt > 0.25f)
            {
                u.clipPushedAt = u.LocalTime;
                loco.Push(away * (Mathf.Clamp(into * 3f, 0.3f, 1.2f) * Mathf.Sqrt(u.Body.SizeFactor)));
            }
        }

        /// <summary>A body this far into another (a share of the least gap) is moved straight out of it.</summary>
        public const float ClipDeepIn = 0.75f;

        private float clipPushedAt = -10f;

        /// <summary>How often two bodies were found deep in each other and moved straight out, and how far in all (metres): for the logs.</summary>
        public static int ClipDeepMoves;

        public static float ClipDeepMoved;

        /// <summary>
        /// Starts a strike at <paramref name="target"/> (the fight loop's choice of when and at whom; the lab's of how): one of
        /// its strikes that lands from where it stands, or, when <paramref name="combo"/>, the lab's combo it opens. False when
        /// none of its strikes reaches, or it is busy.
        /// </summary>
        public bool ClipStrike(UnitController target, bool combo)
        {
            if (Clip == null || Clip.Strikes == null || Clip.Busy || !CanAct || target == null || target.Clip == null || !target.IsAlive)
            {
                return false;
            }

            // (Not with a friend in the swing's way: near it, between it and its target or beside the line. The fight's
            // own blows hit an ally now and then; the lab's combos, sweeping wide, did so one blow in four.)
            Vector3 toTarget = target.Root.position - Root.position;
            toTarget.y = 0f;
            foreach (var other in all)
            {
                if (other == this || other == target || other.Team != Team || !other.IsAlive || other.Clip == null)
                {
                    continue;
                }

                Vector3 toAlly = other.Root.position - Root.position;
                toAlly.y = 0f;
                if (toAlly.magnitude < ClipFighter.FightingDistance(Clip.Size, other.Clip.Size) && Vector3.Angle(toTarget, toAlly) < ClipAllyAngle)
                {
                    return false;
                }
            }

            var clip = Clip.StrikeThatLands(target.Clip);
            if (clip == null)
            {
                return false;
            }

            ClipTarget = target;
            string what;
            if (combo && Clip.ComboAt(target.Clip, clip, clipVariant++, out var opener, out var chain))
            {
                what = "a combo: " + opener.Name;
                foreach (var link in chain)
                {
                    what += " > " + link.To.Name;
                }
            }
            else
            {
                Clip.StrikeAt(target.Clip, clip);
                what = clip.Name;
            }

            ClipStrikeStarted?.Invoke(this, what, target);
            return true;
        }

        /// <summary>One of its blows met a body: that unit takes it as the fight does (its health, poise, death), and the logs hear of it.</summary>
        private void OnClipStruck(StrikeHits.StruckBy s)
        {
            var struck = s.Struck != null ? s.Struck.FightUnit : null;
            if (struck == null)
            {
                return;
            }

            var r = s.Result;
            r.Damage = s.Damage;
            r.PoiseDamage = s.PoiseDamage;
            var report = new HitReport
            {
                Attacker = this, Struck = struck, Part = s.Part, Shield = s.Shield, Parried = s.Parried, Point = s.Point, Speed = s.Speed, Result = r,
                Kind = s.Parried ? "parried" : s.Shield ? "blocked by the shield"
                    : struck == ClipTarget ? "the target" : struck.Team == Team ? "an ally (friendly fire)" : "someone in the way",
            };
            struck.ClipTakeHit(report, Clip.Strikes != null ? Clip.Strikes.HitStop : 0f);
            if (!report.Blocked)
            {
                Rhythm?.NoteLanded();
            }

            Hit?.Invoke(report);
        }

        /// <summary>A blow taken (its reaction is the clip fighter's own): health, poise, the tempo's and rhythm's notes, the hit stop, and death.</summary>
        private void ClipTakeHit(HitReport hit, float stop)
        {
            if (!IsAlive)
            {
                return;
            }

            var r = hit.Result;
            Health = Mathf.Max(0f, Health - r.Damage);
            Clip.Health = Health;
            Rhythm?.NoteHit(r.Damage / Mathf.Max(1f, profile.maxHealth));
            Tempo?.NoteHit(r.PoiseDamage / Mathf.Max(1f, profile.maxPoise));
            Poise = Clip.Poise;
            LastReaction = r.Reaction;
            clipStopLeft = Mathf.Max(clipStopLeft, stop);
            // (Struck, it knows who did it, seen or not: an attacker from behind is known from now on.)
            NoteStruck(hit);
            if (Health <= 0f)
            {
                ClipDie(hit);
            }
        }

        /// <summary>Killed: down for good, its body to physics (the clip fighter's fall), and the fight told.</summary>
        private void ClipDie(HitReport hit)
        {
            IsAlive = false;
            Health = 0f;
            Clip.Health = 0f;
            State = UnitState.Down;
            ClipWant = Vector3.zero;
            ClipTarget = null;
            Vector3 push = hit.Result.Impulse * (1f - hit.Result.PartShare);
            Clip.Die(push, push.magnitude / Mathf.Max(1f, Body.Mass));
            Died?.Invoke(this, hit);
        }

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

            if (definition == null)
            {
                return;
            }

            if (definition.prefab == null)
            {
                if (definition.IsShield)
                {
                    // (No model (headless): a board the shield's size to block with, as the lab's runners give theirs.)
                    var board = new GameObject(name + " shield board");
                    board.transform.SetParent(slot, false);
                    var box = board.AddComponent<BoxCollider>();
                    box.size = new Vector3(0.522f, 0.522f, 0.05f) * Clip.Size;
                    box.center = new Vector3(0f, 0f, 0.075f) * Clip.Size;
                    box.isTrigger = true;
                    Clip.ShieldColliders.Add(box);
                }

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
