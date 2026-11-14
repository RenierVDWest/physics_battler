using System.Collections.Generic;
using PhysicsBattler.Authority;
using PhysicsBattler.Motion;
using PhysicsBattler.Muscles;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// A fighter whose body moves from the mocap clips (the plan "Bringing the lab into the fight", step 1): one
    /// body that walks, guards, strikes and defends, the same in the lab and (to come) in the fight. Walked by its
    /// own <see cref="Locomotor"/> and posed by its <see cref="LocoBody"/>; physical (the legs carried, the torso,
    /// head and arms held by pins).
    /// <para>Attacking (<see cref="EnableStrikes"/>, <see cref="Strikes"/>, <see cref="Combo"/>): its strikes and combos, their blades swept
    /// against the bodies about it (<see cref="StrikeHits"/>), the weapon arm held to its pose through a blow.</para>
    /// <para>Defending (<see cref="Defends"/>): it watches the blow coming and meets it, open (no defence: every blow
    /// lands), with a shield (<see cref="Stances.Shield"/>) or its sword (<see cref="Stances.Sword"/>,
    /// <see cref="SwordParry"/>), each at its skill; a blow that lands knocks the part struck and softens its hold
    /// for a moment, and the rest pushes its walk back a step.</para>
    /// <para>A step is <see cref="Move"/> (the walk, the strike, the pose, the defence) then <see cref="Apply"/>
    /// (the arms' holds, the physical body driven to the pose); the lab's dummy steers itself with <see cref="Step"/>.
    /// Lab-only: its spot (<see cref="Home"/>) and standing as a training post (<see cref="Anchored"/>).</para>
    /// </summary>
    public sealed class ClipFighter
    {
        public readonly GameObject Unit;
        public readonly UnitRig Rig;
        public readonly AnimRig Anim;
        public readonly Locomotor Loco;
        public readonly LocoBody Poser;
        public AuthorityController Authority { get; }

        /// <summary>The colliders of what the dummy holds as a shield (a blade that meets one is blocked).</summary>
        public readonly HashSet<Collider> ShieldColliders = new HashSet<Collider>();

        /// <summary>Its shield's board: its radius at a Medium's size (metres; the lab's shield 0.29, the headless board's half width 0.26).</summary>
        public float ShieldRadius = 0.27f;

        private Vector3 boardLocal;
        private bool boardLocalKnown;

        /// <summary>Where its board's centre sits on the shield's slot (the slot's own frame, metres; fixed, so taken once).</summary>
        public bool BoardOnSlot(out Vector3 local)
        {
            if (!boardLocalKnown && Rig.SlotWeaponL != null && ShieldBoard(out var c, out _, out _))
            {
                boardLocal = Rig.SlotWeaponL.InverseTransformPoint(c);
                boardLocalKnown = true;
            }

            local = boardLocal;
            return boardLocalKnown;
        }

        /// <summary>Its shield's board now: the centre of its face, the way the face looks (the shield slot's forward, as the shield's prefab has it) and its radius (metres).</summary>
        public bool ShieldBoard(out Vector3 centre, out Vector3 normal, out float radius)
        {
            centre = normal = Vector3.zero;
            radius = ShieldRadius * Size;
            if (ShieldColliders.Count == 0 || Rig.SlotWeaponL == null)
            {
                return false;
            }

            var board = System.Linq.Enumerable.First(ShieldColliders);
            if (board == null)
            {
                return false;
            }

            centre = board is BoxCollider box ? box.transform.TransformPoint(box.center)
                : board is SphereCollider ball ? ball.transform.TransformPoint(ball.center)
                : board is CapsuleCollider cap ? cap.transform.TransformPoint(cap.center)
                : board.bounds.center;
            normal = Rig.SlotWeaponL.forward;
            return true;
        }

        private readonly Dictionary<Collider, PartId> parts = new Dictionary<Collider, PartId>();
        private readonly UnitProfile profile;
        private bool snapped;

        /// <summary>Where it stands: knocked off it, it steps back onto it (after <see cref="HomeAfter"/> seconds unstruck).</summary>
        public Vector3 Home;

        /// <summary>How long after a blow it starts back, from how far off its spot (leg lengths... a Medium's metres), and how fast (m/s).</summary>
        public const float HomeAfter = 1f, HomeFrom = 0.15f, HomeSpeed = 0.7f;

        private float clock, lastHit = -10f;

        /// <summary>The body's mass (kg) and its size against a Medium's.</summary>
        public readonly float Mass, Size;

        /// <summary>Its poise: blows wear it down, it comes back over time (as a unit's does in the fight).</summary>
        public float Poise, MaxPoise;

        /// <summary>Its health, as the fight's damage rules judge a blow by it (the lab's fighters never die: 1e6); a fight unit keeps it.</summary>
        public float Health = 1e6f;

        /// <summary>Killed: down for good, its body given to physics, never getting up.</summary>
        public bool Dead { get; private set; }

        /// <summary>The fight unit this fighter moves, if any.</summary>
        public Units.UnitController FightUnit;

        /// <summary>How the dummy is set up (the lab's dummy picker): open (its guard down, no defence: every blow lands),
        /// shield (sword and shield, meeting each blow with its shield), or sword (sword alone, parrying each blow with it).</summary>
        public enum Stances
        {
            Open,
            Shield,
            Sword,
        }

        private Stances stance = Stances.Shield;

        public Stances Stance
        {
            get => stance;
            set
            {
                stance = value;
                Loco.GuardWanted = value != Stances.Open;
            }
        }

        /// <summary>How well it blocks with a shield and parries with a sword (0 to 1; stage 1 of the sword parry: only 1 is made yet).</summary>
        public float ShieldSkill = 1f, ParrySkill = 1f;

        /// <summary>
        /// Its concentration (0 to 1; the plan "Fighting off several attackers", step 7): outnumbered it builds, and it
        /// blocks and parries better (<see cref="CombatFeel.ConcentrationGuard"/>). Set by the unit each step.
        /// </summary>
        public float Concentration;

        /// <summary>What the blow it meets has taken off its skill (its striker's concentration).</summary>
        private float BlowKeen => Blow.HasValue ? Blow.Value.Keen : 0f;

        /// <summary>Its skills against the blow it meets now: its own, its concentration added, the striker's taken off.</summary>
        public float ShieldSkillNow => Mathf.Clamp01(ShieldSkill + Concentration * CombatFeel.ConcentrationGuard - BlowKeen);

        public float ParrySkillNow => Mathf.Clamp01(ParrySkill + Concentration * CombatFeel.ConcentrationGuard - BlowKeen);

        /// <summary>What its concentration adds to the weight its guard takes (1: none).</summary>
        public float GuardHolds => 1f + Concentration * CombatFeel.ConcentrationGuard;

        /// <summary>Its sword parry (with <see cref="Stances.Sword"/>; made by whoever gives it its sword).</summary>
        public SwordParry Parry;

        /// <summary>The blow coming, as it reads it (null: none).</summary>
        public BlowRead? Blow;

        /// <summary>The parries it has made.</summary>
        public int Parries;

        /// <summary>The blow coming, read from the striker's hits (or none).</summary>
        /// <summary>The same, given the read of the one blow it meets (the fight: chosen from every attacker's), or none.</summary>
        public void Watch(BlowRead? read)
        {
            if (read.HasValue)
            {
                Blow = read;
                lastBlowAt = clock;
                IncomingBlade = read.Value.Blade;
                IncomingAim = read.Value.Aim;
                IncomingFrom = read.Value.ComesFrom;
            }
            else
            {
                Blow = null;
                IncomingBlade = IncomingAim = IncomingFrom = null;
            }
        }

        public void Watch(StrikeHits hits)
        {
            if (hits != null && hits.Read(out var read))
            {
                Blow = read;
                lastBlowAt = clock;
                IncomingBlade = read.Blade;
                IncomingAim = read.Aim;
                IncomingFrom = read.ComesFrom;
            }
            else
            {
                Blow = null;
                IncomingBlade = IncomingAim = IncomingFrom = null;
            }
        }

        /// <summary>
        /// It stands its ground (a training post): a blow throws its upper body and arms about but doesn't drive it
        /// back. Off (the lab's default): it is an enemy unit that doesn't fight back, driven back by what the
        /// struck part doesn't take, as the fight's rules have it.
        /// </summary>
        public bool Anchored;

        /// <summary>How near the other body it lets come (metres, root to root: the fighting distance); 0: no limit.</summary>
        public float KeepDistance;

        private float lastSeparate = -10f;

        /// <summary>How long after it was last struck, or a blow passed, it steps back to its standing distance (seconds).</summary>
        public const float StandBackAfter = 0.8f;

        /// <summary>How long after a blow it still steps back to its standing distance (seconds).</summary>
        public const float StandBackWithin = 1.5f;

        private float lastBlowAt = -10f;

        /// <summary>The fighting distance for two bodies (metres, root to root): their sizes against a Medium's, times this.</summary>
        public const float BodyGap = 1.2f;

        public static float FightingDistance(float sizeA, float sizeB) => 0.5f * (sizeA + sizeB) * BodyGap;

        /// <summary>
        /// The nearest two bodies come, whatever they do (a share of their fighting distance): two lunging into each other
        /// at once each cut its lunge short of where the other stood, and passed through each other (spar: 0.51 m).
        /// </summary>
        public const float LeastGap = 0.85f;

        /// <summary>
        /// Two bodies kept apart: nearer than <see cref="LeastGap"/> of their fighting distance (root to root), each is
        /// moved straight off the other at once, the lighter the further, as two bodies meeting push each other off.
        /// </summary>
        public static void KeepApart(ClipFighter a, ClipFighter b) => KeepApart(a, b, a != null && b != null ? LeastGap * FightingDistance(a.Size, b.Size) : 0f);

        /// <summary>The same, no nearer than <paramref name="least"/> (metres, root to root: two of a side stand closer than two who fight).</summary>
        public static void KeepApart(ClipFighter a, ClipFighter b, float least)
        {
            if (a == null || b == null || a.Fallen || b.Fallen)
            {
                return;
            }

            Vector3 gap = b.Loco.Position - a.Loco.Position;
            gap.y = 0f;
            float apart = gap.magnitude;
            if (apart >= least)
            {
                return;
            }

            Vector3 way = apart > 1e-4f ? gap / apart : Quaternion.Euler(0f, a.Loco.Facing, 0f) * Vector3.forward;
            float into = least - apart, shareA = b.Mass / Mathf.Max(1f, a.Mass + b.Mass);
            RootShoves[0] += into;
            a.Loco.Position -= way * (into * shareA);
            b.Loco.Position += way * (into * (1f - shareA));
            a.CarryBy(-way * (into * shareA));
            b.CarryBy(way * (into * (1f - shareA)));
        }

        /// <summary>How far apart it stands from the other body between blows (metres, root to root; 0: as near as <see cref="KeepDistance"/>): out of reach of a blow without a step, the guards' points short of the other's shield.</summary>
        public float StandDistance;

        /// <summary>The standing distance for two bodies: their sizes against a Medium's, times this (1.4 m for two Mediums).</summary>
        public const float StandGap = 1.6f;

        public static float StandingDistance(float sizeA, float sizeB) => 0.5f * (sizeA + sizeB) * StandGap;

        /// <summary>A blow that would set it moving faster than this (m/s) drives it back with steps (the walk's own); a lighter one plays a reaction clip where it stands.</summary>
        public const float DrivenFrom = 0.35f;

        /// <summary>The blow coming: where the striker's weapon is now, and where its blow is aimed (world); null when none comes.</summary>
        public Vector3? IncomingBlade, IncomingAim;

        /// <summary>The way the blow coming will come in at its contact (world, unit), read from its wind-up; null: not read.</summary>
        public Vector3? IncomingFrom;

        /// <summary>How much the shield goes by the blow's line as read (the rest by where the weapon is now).</summary>
        public const float ReadLine = 0.75f;

        /// <summary>How long a blow has been coming (seconds), and how long before the shield moves to meet it.</summary>
        private float incomingFor;

        /// <summary>How long it takes to see a blow coming (seconds), and the fastest it moves its shield (m/s at a Medium's size).</summary>
        public const float SeeBlowAfter = 0.12f, ShieldSpeed = 6f;

        // ---- Shield skill (the defence plan's table): 0 a novice, 1 an expert ---------------------------------------

        /// <summary>
        /// The levers at shield skill 0 (at 1: 0.12 s, none, none, 6 m/s, every step, covers the legs, slanted, 0.25 s): it
        /// moves only 0.35 s into a blow, reads where it lands up to 25 cm off and the line it comes on up to 30 degrees
        /// off, its shield comes at 2 m/s (and builds up to its speed, not jumping), it keeps to its first read, it holds
        /// its shield at the chest against a low cut, it takes a blow square on (all of it), and its shield is back in
        /// the guard 0.6 s after.
        /// </summary>
        public const float NoviceSeeAfter = 0.35f, NoviceMisread = 0.25f, NoviceMisline = 30f, NoviceShieldSpeed = 2f, NoviceBackAfter = 0.6f,
            ExpertBackAfter = 0.25f, ExpertSlant = 30f, ExpertGlance = 0.5f;

        /// <summary>It moves its shield no sooner than this before contact (seconds): an expert as soon as it reads the blow, a novice only once the blade is on its way (between them on a steep curve: how much sooner is what decides most blocks).</summary>
        public const float ExpertSeesBefore = 2f, NoviceSeesBefore = 0.05f;

        /// <summary>The fastest the shield's face turns toward a blow (degrees a second), an expert's and a novice's.</summary>
        public const float ShieldTurnSpeed = 720f, NoviceShieldTurnSpeed = 200f;

        /// <summary>
        /// How long before contact it moves, for how good it is (0 to 1 past the curve): a block is won or lost between
        /// 0.05 and 0.2 s before contact, so most of the scale is spent there; only the best read the whole wind-up.
        /// </summary>
        private static float SeesBefore(float good) => good < 0.75f
            ? Mathf.Lerp(NoviceSeesBefore, NearlySeesBefore, good / 0.75f)
            : Mathf.Lerp(NearlySeesBefore, ExpertSeesBefore, (good - 0.75f) / 0.25f);

        /// <summary>Where the steep part of <see cref="SeesBefore"/> ends (seconds before contact).</summary>
        public const float NearlySeesBefore = 0.2f;

        private Vector3 shieldFace;
        private bool shieldFaceKnown;

        /// <summary>How far from an expert, 0 to 1 (on the curve the parry uses: skill 0.5 midway).</summary>
        private float ShieldLack => 1f - ShieldSkillNow * ShieldSkillNow;

        /// <summary>How much of a blow a slanted shield turns aside (0 square on: all of it taken); StrikeHits takes it off the knock.</summary>
        public float ShieldGlance => ExpertGlance * (1f - ShieldLack);

        private readonly System.Random shieldLuck = new System.Random(20261104);
        private Vector3 shieldMisread, shieldHeldAim, shieldHeldLine;
        private Quaternion shieldMisline = Quaternion.identity;
        private bool coversLow = true, shieldReadHeld;
        private float lastComing = -1f, shieldReread, shieldSpeedNow, backIn;
        private int lastShieldSerial = -1;

        /// <summary>Blows its shield has read (for the logs).</summary>
        public int ShieldBlowsRead;

        /// <summary>
        /// How far from an expert it is against this blow (the plan "Shield and blade contact", step 5): a blow on its
        /// own is easier to read than a combo's next one, coming on the heels of the last (within <see cref="ComboGap"/>).
        /// Each on its own curve of the skill (the scene's Combat Feel), so a middling shield (skill 0.5) misses about a
        /// quarter of single blows and half a combo's.
        /// </summary>
        private float blowLack = 1f;

        /// <summary>A blow read within this long of the last one passing is a combo's next blow (seconds).</summary>
        public const float ComboGap = 0.6f;

        private float lastBlowSeenAt = -10f;

        /// <summary>Blows it read as a combo's next (for the logs).</summary>
        public int ShieldComboBlowsRead;

        private void NewShieldBlow()
        {
            ShieldBlowsRead++;
            float skill = ShieldSkillNow;
            bool follow = clock - lastBlowSeenAt < ComboGap;
            ShieldComboBlowsRead += follow ? 1 : 0;
            blowLack = 1f - Mathf.Pow(skill, follow ? CombatFeel.ShieldComboCurve : CombatFeel.ShieldSingleCurve);
            float lack = blowLack;
            Vector3 r = new Vector3((float)shieldLuck.NextDouble() * 2f - 1f, (float)shieldLuck.NextDouble() * 2f - 1f, (float)shieldLuck.NextDouble() * 2f - 1f);
            shieldMisread = (r.sqrMagnitude > 1e-4f ? r.normalized : Vector3.up) * (NoviceMisread * lack * (0.4f + 0.6f * (float)shieldLuck.NextDouble()) * Size);
            Vector3 axis = new Vector3((float)shieldLuck.NextDouble() * 2f - 1f, (float)shieldLuck.NextDouble() * 2f - 1f, (float)shieldLuck.NextDouble() * 2f - 1f);
            shieldMisline = Quaternion.AngleAxis(NoviceMisline * lack * (0.4f + 0.6f * (float)shieldLuck.NextDouble()), axis.sqrMagnitude > 1e-4f ? axis.normalized : Vector3.up);
            coversLow = shieldLuck.NextDouble() >= lack;
            shieldReadHeld = false;
            shieldReread = 0f;
            shieldSpeedNow = 0f;
        }

        private Vector3 shieldAt, shieldAtV;
        private bool shieldAtKnown;

        // Which way the shield arm's elbow points across the line from its shoulder to its hand while the shield is moved (the body's frame).
        private Vector3 shieldElbow;
        private bool shieldElbowKnown;

        /// <summary>The fastest the shield arm's elbow turns about the line from its shoulder to its hand while the shield is moved to meet a blow (degrees a second).</summary>
        public const float ShieldElbowTurn = 720f;
        private float interceptWeight;

        /// <summary>The shield arm is held exactly to its pose now (meeting a blow): the physical arm doesn't trail it.</summary>
        public bool ShieldArmHeld => interceptWeight > 0.05f || blockWeight > 0.05f;

        /// <summary>The reaction it played last ("" before any), for the logs.</summary>
        public string LastReaction = "";

        /// <summary>It meets blows with its shield.</summary>
        public bool Blocking => stance == Stances.Shield;

        /// <summary>Where the blow coming is aimed (world): the shield goes between it and the striker.</summary>
        public Vector3? BlockAt;

        private float blockWeight;

        /// <summary>How fast the shield comes up and goes down again (seconds for all of it).</summary>
        public const float BlockRaise = 0.15f;

        /// <summary>Poise back a second (share of the most).</summary>
        public const float PoiseBack = 0.15f;

        public int Hits, Blocks;

        // Each region's hold, softened by a blow and coming back.
        private readonly float[] pinFrom = new float[5], pinTime = new float[5], pinLeft = new float[5];

        /// <param name="unit">The unit's body, just built (its own controller taken off), in its rest pose.</param>
        public ClipFighter(GameObject unit, UnitRig rig, UnitProfile profile, Vector3 at, float facing, float groundY, string guardPrefix, bool physical)
        {
            Unit = unit;
            Rig = rig;
            this.profile = profile;
            if (physical)
            {
                // (The body set up as a unit sets up its own (its joints in their frames, its limits, its
                // friction), while it is still in its rest pose.)
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

            Anim = new AnimRig(unit.transform, rig);
            float mass = 0f;
            foreach (var part in rig.Parts)
            {
                if (part.body != null)
                {
                    part.body.isKinematic = true;
                    part.body.interpolation = RigidbodyInterpolation.Interpolate;
                    mass += part.body.mass;
                }

                if (part.partCollider != null)
                {
                    parts[part.partCollider] = part.id;
                }
            }

            Mass = Mathf.Max(20f, mass);
            foreach (var id in PartInfo.All)
            {
                rig.Part(id).transform.SetParent(unit.transform, true);
            }

            if (physical)
            {
                Physics.SyncTransforms();
                var all = rig.Parts;
                for (int i = 0; i < all.Length; i++)
                {
                    for (int j = i + 1; j < all.Length; j++)
                    {
                        if (all[i].partCollider != null && all[j].partCollider != null)
                        {
                            Physics.IgnoreCollision(all[i].partCollider, all[j].partCollider, true);
                        }
                    }
                }

                Authority = new AuthorityController(rig, Anim);
                for (int r = 0; r < 5; r++)
                {
                    Hold((BodyRegion)r, 1f);
                }

                PhysicsSteps.BeforeSubstep += BeforeSubstep;
                PhysicsSteps.AfterStep += AfterStep;
            }

            float leg = Anim.RestLocalPosition(PartId.ShinL).magnitude + Anim.RestLocalPosition(PartId.FootL).magnitude;
            Size = leg / 0.845f;
            Loco = new Locomotor(leg);
            Poser = new LocoBody(Anim, Loco) { GroundY = groundY };
            Poser.SoleFrom(rig);
            Loco.SetGuard(guardPrefix);
            Loco.GuardWanted = true;
            Loco.Place(new Vector3(at.x, groundY, at.z), facing);
            Home = new Vector3(at.x, groundY, at.z);
            MaxPoise = Poise = profile.maxPoise;
            groundLevel = groundY;
            if (Authority != null)
            {
                // (The fight's own get-up, as a unit has it: its body's measure, a stance for its feet's spots, a root.)
                body = BodyMeasure.Of(unit.transform, Anim, rig);
                getUpStance = new Stance();
                getUpStance.Configure(profile.style != null ? profile.style : CombatStyle.CreateDefault(), body);
                getUpStance.GroundY = groundY;
                getUpRoot = new GameObject(unit.name + " get-up root").transform;
                getUpRoot.SetParent(unit.transform.parent, false);
                getUp = new GetUp(getUpRoot, Anim, body, getUpStance) { Rig = rig };
            }
        }

        // ---- Leg hits and falls (the defence plan: a hard blow to a leg takes the leg out) --------------------------

        /// <summary>Standing; stumbling on a swept leg; down on the ground; getting up (the fight's own get-up).</summary>
        public enum Footings
        {
            Standing,
            Down,
            GettingUp,
        }

        public Footings Footing { get; private set; } = Footings.Standing;

        /// <summary>Down or getting up: not standing to defend.</summary>
        public bool Fallen => Footing != Footings.Standing;

        /// <summary>
        /// A blow on a leg against the weight it carries (m/s of knock to the whole body, the blow over its mass, for a leg
        /// carrying half the weight): it stumbles from <see cref="StumbleFrom"/> and falls from <see cref="FallFrom"/>, under the
        /// fight's own for a push (0.8 and 2.2 m/s): a leg swept takes less than a body pushed; a leg holding all the weight (the other
        /// foot off the ground) is swept from under it from <see cref="SweptFrom"/>; a foot in the air is knocked away from
        /// <see cref="KnockedAwayFrom"/>.
        /// </summary>
        public const float StumbleFrom = 0.6f, FallFrom = 4f, SweptFrom = 1.2f, KnockedAwayFrom = 0.25f;

        /// <summary>A foot in the air knocked away drops the body from this blow (m/s): a Large's, not a Medium's sword.</summary>
        public const float KnockedDownFrom = 1.8f;

        /// <summary>The swept leg's speed along the blow when it falls, and the upper body's (m/s at a Medium's size).</summary>
        public const float SweptLegSpeed = 3f, ThrownBodySpeed = 0.9f;

        /// <summary>Down at least this long and still this long before it gets up, at most this long down (seconds).</summary>
        public const float LeastDown = 1f, StillFor = 0.3f, MostDown = 2.5f;

        public int Stumbles, Falls;

        /// <summary>Each fall measured (the plan "Controlled falls", step 1): what reached the ground when, and how fast.</summary>
        public readonly List<FallMeasure> FallsMeasured = new List<FallMeasure>();

        /// <summary>The dead and those that stay down are measured for this long, the rest until they begin to get up (seconds).</summary>
        public const float FallMeasuredFor = 3f;

        private FallMeasure falling;

        /// <summary>
        /// A fall keeps the one physical body (the plan "Controlled falls", steps 2 to 4): its hips are let go and it
        /// falls as the body it was, its muscles drawing it to a pose that gets low, reaches for the ground and keeps
        /// the head off it (<see cref="FallPose"/>), with a person's strength. Off: the physical body is thrown away
        /// at the fall and a limp ragdoll takes its place, as before 14 November.
        /// </summary>
        public static bool OneBodyFalls = true;

        /// <summary>How much of a person's strength a falling body's muscles work with on the way down (1: all; 0: limp), the share of that left once it has landed, and what the dead have.</summary>
        public static float FallMuscle = 1f;
        public const float LandedMuscle = 0.4f, DeadMuscle = 0.03f;

        /// <summary>A body thrown much faster than its legs can catch goes down that much faster: up to this many times the speed of one only just felled, at <see cref="ThrownHardAt"/> (m/s at a Medium's size).</summary>
        public const float ThrownHarder = 2.8f, ThrownHardAt = 14f;

        /// <summary>A falling body's planted feet are held where they stood for this long at most (seconds), or until its trunk has tipped this far (degrees) or its hips are down.</summary>
        public const float FeetHeldFor = 0.5f, FeetHeldTo = 50f;

        /// <summary>Falls in the one body (for the reports), and it is in one now.</summary>
        public int OneBodyFallen;
        private bool fallBody;
        private float landedAt = -1f, fallThrown;

        /// <summary>
        /// A fall can be caught (the plan "Controlled falls", step 8): a fighter thrown only a little faster than its
        /// legs can catch (up to <see cref="CaughtUpTo"/> times that) goes down on legs that hold, to a knee and a
        /// hand, and if its hips have not reached the ground <see cref="CaughtFor"/> seconds on it rises straight from
        /// there, a third of a get-up. If they have, it has fallen after all.
        /// </summary>
        public static bool FallsCanBeCaught = true;

        /// <summary>A caught fall: how much faster than its legs can catch it may be thrown (times), how long before it rises (seconds), the hips' height under which it has fallen after all (metres at a Medium's size), and how its legs and the rest hold against a fall's (times).</summary>
        public const float CaughtUpTo = 1.35f, CaughtFor = 0.5f, CaughtLost = 0.3f, CaughtLegs = 1.6f, CaughtMuscle = 1.3f;

        /// <summary>A caught fall is lost when the trunk has gone over this far (degrees from upright): it lies. Short of it the fighter sits or kneels, and rises from there.</summary>
        public const float CaughtLies = 60f;

        /// <summary>The share of a blow's force (the hit force) a body keeps when that blow fells it.</summary>
        public const float FelledHitForce = 0.3f;

        /// <summary>Falls caught on a knee and a hand, and those begun so that went down after all (for the reports).</summary>
        public int Caught, CaughtAndLost;
        private bool catching, caughtSitting;

        /// <summary>The fall goes on in the physical body it had: let go, and sent the way of the blow.</summary>
        private void FallInBody(int leg, Vector3 way, float knock, float thrown)
        {
            fallBody = true;
            OneBodyFallen++;
            landedAt = -1f;
            Upper.Tone = FallMuscle;
            // (The blow that felled it: most of its force is taken back. The fall has its own speed.)
            Upper.EaseStrike(FelledHitForce);
            Upper.LetGo(Size);
            if (catching)
            {
                // (Its legs hold: it goes down on them, not through them.)
                Upper.LegsHave = CaughtLegs;
            }

            float k = Mathf.Sqrt(Size);
            float harder = Mathf.Lerp(1f, ThrownHarder, Mathf.InverseLerp(CombatFeel.LegsCatch * k, ThrownHardAt * k, thrown));
            foreach (var id in Upper.Own)
            {
                bool swept = leg == 0 ? id == PartId.ThighL || id == PartId.ShinL || id == PartId.FootL
                    : leg == 1 && (id == PartId.ThighR || id == PartId.ShinR || id == PartId.FootR);
                float speed = swept ? SweptLegSpeed * (id == PartId.ThighL || id == PartId.ThighR ? 0.5f : 1f)
                    : AuthorityController.RegionOf(id) == BodyRegion.Legs ? 0.2f * ThrownBodySpeed : ThrownBodySpeed;
                Upper.Send(id, way * (speed * k * harder * Mathf.Clamp(knock / 0.5f, 0.6f, 1.6f)));
            }
        }

        /// <summary>A step of a fall in the one body: how hard its muscles work, and the pose they draw it to.</summary>
        private void StepFallBody(float dt)
        {
            if (Upper == null)
            {
                fallBody = false;
                return;
            }

            upperDt = dt;
            var hips = Upper.Link(PartId.Pelvis).transform;
            if (landedAt < 0f && hips.position.y - groundLevel < 0.3f * Size)
            {
                landedAt = footingTime;
            }

            // (All it has going down; easing off once it is down; the dead: a start, then nothing.)
            Upper.Tone = Dead ? Mathf.Lerp(FallMuscle, DeadMuscle, Mathf.Clamp01((footingTime - 0.12f) / 0.3f))
                : catching ? CaughtMuscle * FallMuscle
                : landedAt < 0f ? FallMuscle : FallMuscle * Mathf.Lerp(1f, LandedMuscle, Mathf.Clamp01((footingTime - landedAt) / 0.5f));
            // (Its feet are held where they stood until it has gone over far enough to take them with it.)
            if (Upper.FeetHeld && (footingTime > FeetHeldFor || Vector3.Angle(hips.up, Vector3.up) > FeetHeldTo || landedAt >= 0f))
            {
                Upper.LetFeetGo();
            }

            float into = Mathf.Clamp01((footingTime - 0.08f) / 0.3f);
            FallPose.Pose(Anim, hips, Upper.SpeedOf(PartId.Pelvis), groundLevel, Size, into * into * (3f - 2f * into), stance != Stances.Open);
            Upper.BeginStep(dt);
        }

        /// <summary>Times a blow threw it faster than its legs can catch, and it went down (for the reports).</summary>
        public int ThrownDown;

        /// <summary>Leg hits this close together are one blow's (seconds).</summary>
        public const float OneSweepWithin = 0.2f;

        private float lastLegHit = -1f, lastStumble = -1f;
        private Vector3 legSweep;

        private float groundLevel, footingTime, stillFor;
        private BodyMeasure body;
        private Stance getUpStance;
        private Transform getUpRoot;
        private GetUp getUp;
        private readonly PoseBuffer lying = new PoseBuffer(), standingPose = new PoseBuffer();

        /// <summary>
        /// Leg sweeps on: a blow on a leg can make it stumble or drop it (shelved after the first playtests: the falls
        /// and the get-up didn't read well; off, a leg hit is a body hit, the leg twisted by it).
        /// </summary>
        public static bool LegFalls;

        /// <summary>What a blow on a leg does: knocks it, makes the body stumble, or takes the leg out from under it.</summary>
        private string SweepLeg(PartId part, Vector3 impulse)
        {
            if (!LegFalls || Anchored || Fallen)
            {
                return "";
            }

            int leg = part == PartId.ThighL || part == PartId.ShinL || part == PartId.FootL ? 0 : 1;
            bool struckDown = FootDown(leg), otherDown = FootDown(1 - leg);
            // (One blow sweeping through both legs, or a thigh and a shin, sweeps them as one: its parts' knocks added.)
            Vector3 flat = new Vector3(impulse.x, 0f, impulse.z);
            if (clock - lastLegHit < OneSweepWithin)
            {
                flat += legSweep;
            }

            lastLegHit = clock;
            legSweep = flat;
            float knock = flat.magnitude / Mass;
            Vector3 way = flat.sqrMagnitude > 1e-6f ? flat.normalized : -(Quaternion.Euler(0f, Loco.Facing, 0f) * Vector3.forward);
            bool falls, stumbles;
            string why;
            if (struckDown && !otherDown)
            {
                // (All its weight on the leg struck: swept from under it, it has nothing to stand on. A Medium's sword
                // only makes it stumble: the playtest, it went down to every thigh cut.)
                falls = knock >= SweptFrom;
                stumbles = !falls && knock >= KnockedAwayFrom;
                why = "its weight all on that leg";
            }
            else if (struckDown)
            {
                float swept = knock / 0.5f;
                falls = swept >= FallFrom;
                stumbles = !falls && swept >= StumbleFrom;
                why = "on both feet";
            }
            else
            {
                // (A foot in the air, knocked away: it stands on the other.)
                falls = knock >= KnockedDownFrom;
                stumbles = !falls && knock >= KnockedAwayFrom;
                why = "that foot in the air";
            }

            string side = leg == 0 ? "left" : "right";
            if (falls)
            {
                Fall(leg, way, knock);
                return $"the {side} leg taken out from under it ({why}, a {knock:0.0} m/s blow), down";
            }

            if (stumbles)
            {
                // (A catch step: the swept leg let go and the body knocked along the blow.)
                if (clock - lastStumble > OneSweepWithin)
                {
                    Stumbles++;
                }

                lastStumble = clock;
                Poser.ReleaseFoot(leg);
                pendingKnock = Vector3.ClampMagnitude(way * Mathf.Max(0.5f, 0.6f * knock / 0.5f), MostKnock);
                pushAt = clock;
                return $"stumbled on the {side} leg ({why}, a {knock:0.0} m/s blow)";
            }

            return "";
        }

        /// <summary>A foot carrying weight: held where it landed, or on the ground (a guard's small shuffles let go of a lock now and then).</summary>
        private bool FootDown(int f) =>
            Poser.Feet[f].Locked || Anim.Part(f == 0 ? PartId.FootL : PartId.FootR).position.y - groundLevel < FootDownWithin * Size;

        /// <summary>A foot this near the ground (its ankle, metres at a Medium's size) is standing on it.</summary>
        public const float FootDownWithin = 0.2f;

        /// <summary>Killed (the fight): it falls as a body knocked off its feet does, along <paramref name="way"/>, and never gets up.</summary>
        public void Die(Vector3 way, float knock)
        {
            if (Dead)
            {
                return;
            }

            Dead = true;
            Combo?.Stop();
            Blow = null;
            way.y = 0f;
            Fall(-1, way.sqrMagnitude > 1e-6f ? way.normalized : -(Quaternion.Euler(0f, Loco.Facing, 0f) * Vector3.forward), knock);
        }

        /// <summary>Down: the body given to physics, the swept leg thrown along the blow and the body over it, falling.</summary>
        private void Fall(int leg, Vector3 way, float knock)
        {
            Falls++;
            Footing = Footings.Down;
            footingTime = stillFor = 0f;
            Blow = null;
            if (Authority == null)
            {
                return;
            }

            LogGetUp();
            roseAt = -1f;
            EndFallMeasure();
            falling = new FallMeasure { Way = FallMeasure.WayOf(way, Loco.Facing), Why = LastReaction ?? "", Dead = Dead };
            fallBody = false;
            if (OneBodyFalls && Upper != null && !StaysDown)
            {
                bool whole = true;
                foreach (var id in Upper.Own)
                {
                    whole &= !Rig.IsSevered(id);
                }

                if (whole)
                {
                    FallInBody(leg, way, knock, fallThrown);
                    fallThrown = 0f;
                    return;
                }
            }

            fallThrown = 0f;

            // (Its physical upper body is let go as it goes down: the rig's own parts fall from where it was. A fallen
            // body is not stepped as a standing one is, and left to that its upper body stood on where it died.)
            if (Upper != null)
            {
                ReleaseUpper();
            }

            for (int r = 0; r < 5; r++)
            {
                Authority.Set((BodyRegion)r, AuthorityLevel.Ragdoll, 0f);
            }

            float k = Mathf.Sqrt(Size);
            foreach (var p in Rig.Parts)
            {
                if (p.body == null)
                {
                    continue;
                }

                p.body.isKinematic = false;
                bool swept = leg == 0 ? p.id == PartId.ThighL || p.id == PartId.ShinL || p.id == PartId.FootL
                    : leg == 1 && (p.id == PartId.ThighR || p.id == PartId.ShinR || p.id == PartId.FootR);
                var region = AuthorityController.RegionOf(p.id);
                float speed = swept ? SweptLegSpeed * (p.id == PartId.ThighL || p.id == PartId.ThighR ? 0.5f : 1f)
                    : region == BodyRegion.Legs ? 0.2f * ThrownBodySpeed : ThrownBodySpeed;
                p.body.linearVelocity = way * (speed * k * Mathf.Clamp(knock / 0.5f, 0.6f, 1.6f));
                p.body.angularVelocity = Vector3.zero;
            }
        }

        /// <summary>The most a blow sends a part of a fallen body (m/s at a Medium's size).</summary>
        public const float MostFallenPartSpeed = 2.5f;

        /// <summary>A blow on a fallen body: the part it struck knocked, nothing else (its holds are the fall's or the get-up's).</summary>
        private void PushFallen(PartId part, Vector3 impulse, float share)
        {
            if (fallBody && Upper != null && Footing == Footings.Down)
            {
                Upper.Send(part, Vector3.ClampMagnitude(impulse * Mathf.Clamp01(share) / Mathf.Max(0.5f, Rig.Part(part).body != null ? Rig.Part(part).body.mass : 1f), MostFallenPartSpeed * Mathf.Sqrt(Size)));
                return;
            }

            var struck = Authority != null ? Rig.Part(part).body : null;
            if (struck == null || struck.isKinematic)
            {
                return;
            }

            Vector3 kick = impulse * Mathf.Clamp01(share) / Mathf.Max(0.5f, struck.mass);
            struck.AddForce(Vector3.ClampMagnitude(kick, MostFallenPartSpeed * Mathf.Sqrt(Size)), ForceMode.VelocityChange);
        }

        /// <summary>A step down or getting up: the body lies until it is still, then gets up as a unit does.</summary>
        private void StepFallen(float dt)
        {
            footingTime += dt;
            if (Dead)
            {
                // (Dead: it lies where it fell; its place goes with its body.)
                if (Authority != null)
                {
                    Authority.Apply(dt);
                    if (fallBody)
                    {
                        StepFallBody(dt);
                    }

                    var lying = Rig.Part(PartId.Pelvis).body;
                    Loco.Position = new Vector3(lying.position.x, groundLevel, lying.position.z);
                    StepFallMeasure(dt);
                }

                return;
            }

            if (Authority == null)
            {
                // (Headless, no body to fall: down a moment, then standing.)
                if (footingTime > LeastDown + 0.5f && !StaysDown)
                {
                    Footing = Footings.Standing;
                }

                return;
            }

            var pelvis = Rig.Part(PartId.Pelvis).body;
            if (Footing == Footings.Down)
            {
                float hipsGo = fallBody && Upper != null ? Upper.SpeedOf(PartId.Pelvis).magnitude : pelvis.linearVelocity.magnitude;
                stillFor = hipsGo < 0.25f * Mathf.Sqrt(Size) ? stillFor + dt : 0f;
                Authority.Apply(dt);
                if (fallBody)
                {
                    StepFallBody(dt);
                }

                if (catching && fallBody && Upper != null && !Dead)
                {
                    var hips = Upper.Link(PartId.Pelvis).transform;
                    float up = hips.position.y - groundLevel;
                    if (Vector3.Angle(hips.up, Vector3.up) > CaughtLies)
                    {
                        // (Its trunk has gone over: it has fallen after all.)
                        catching = false;
                        CaughtAndLost++;
                        Upper.LegsHave = UpperBodyArticulation.LegTone;
                    }
                    else if (footingTime >= CaughtFor && hipsGo < 0.8f * Mathf.Sqrt(Size))
                    {
                        // (Upright still, on a knee or sitting: it rises from there.)
                        caughtSitting = up < CaughtLost * Size;
                        Caught++;
                        Loco.Position = new Vector3(pelvis.position.x, groundLevel, pelvis.position.z);
                        StepFallMeasure(dt);
                        EndFallMeasure();
                        BeginGetUp();
                        return;
                    }
                }
                else
                {
                    catching &= fallBody && !Dead;
                }

                // (Its place goes with the body on the ground: what keeps its distance from it, or aims at it, follows.)
                Loco.Position = new Vector3(pelvis.position.x, groundLevel, pelvis.position.z);
                StepFallMeasure(dt);
                if (!StaysDown && ((footingTime >= LeastDown && stillFor >= StillFor) || footingTime >= MostDown))
                {
                    EndFallMeasure();
                    BeginGetUp();
                }

                return;
            }

            // Getting up: the fight's key poses from how it lies, and its guard taking over at the end.
            PoseBuffer stand = null;
            if (footingTime > getUp.StandFrom)
            {
                Loco.Step(dt, Vector3.zero, Loco.Facing);
                Poser.Pose(dt);
                standingPose.Capture(Anim);
                stand = standingPose;
            }

            getUp.Compose(footingTime, stand);
            MeasureGetUpSeen(dt);
            if (fallBody && Upper != null)
            {
                // (The one body it fell in is drawn to the get-up's pose, held as a standing body is.)
                upperDt = dt;
                Upper.BeginStep(dt);
            }
            float u = Mathf.Clamp01(footingTime / Mathf.Max(0.1f, getUp.Duration));
            for (int r = 0; r < 5; r++)
            {
                Authority.Set((BodyRegion)r, AuthorityLevel.Pinned, Mathf.Lerp(0.4f, 1f, u) * (r == (int)BodyRegion.Legs ? profile.torsoPin : BasePin((BodyRegion)r)));
            }

            Authority.Apply(dt);
            if (footingTime >= getUp.Duration)
            {
                Footing = Footings.Standing;
                for (int r = 0; r < 5; r++)
                {
                    Hold((BodyRegion)r, 1f);
                }

                fallBody = false;
                GetUpFastest = getUp.FastestEarly;
                GetUpMiss = getUp.ReachMiss;
                GetUpSlid = getUp.Slid;
                // (Its arms come out of the get-up into what a standing fighter's arms do over a moment: the guard, the
                // free hand's place, the shield. Taken up at once they were whipped there, the free arm at 89 rad/s.)
                rose.Capture(Anim);
                roseAt = clock;
                getUpDuration = getUp.Duration;
                getUpLogAt = clock + RoseEase + 0.2f;
                LastReaction = "got up";
            }
        }

        private void StepFallMeasure(float dt)
        {
            if (falling == null)
            {
                return;
            }

            if (fallBody && Upper != null)
            {
                falling.Step(Rig, groundLevel, Size, dt, id => Upper.SpeedOf(id).magnitude);
            }
            else
            {
                falling.Step(Rig, groundLevel, Size, dt);
            }

            if ((Dead || StaysDown) && falling.Lasted >= FallMeasuredFor)
            {
                EndFallMeasure();
            }
        }

        /// <summary>The fall being measured is done: kept, and said in the playtest log.</summary>
        private void EndFallMeasure()
        {
            if (falling == null)
            {
                return;
            }

            falling.Dead |= Dead;
            FallsMeasured.Add(falling);
            if (Diagnostics.PlaytestTrace.Enabled)
            {
                Diagnostics.PlaytestTrace.Note(FightUnit, "fall", falling.Text + (fallBody ? $" [in the one body, muscle {FallMuscle:0.00}{(FallPose.ProtectsHead ? "" : ", the head not kept up")}{(FallPose.Reaches ? "" : ", the arms not reaching")}]" : " [a ragdoll, as before]"));
            }

            falling = null;
        }

        /// <summary>
        /// The get-ups of the plan "Getting up" (14 November): from how the body really lies, lopsided through a knee
        /// with a hand on the ground, turned toward what it fights (<see cref="GetUp.Natural"/>). Off: as they were.
        /// </summary>
        public static bool NaturalGetUps = true;

        /// <summary>Natural get-ups: a body whose front is this near level lies on a side (the sine of how far its front is from level).</summary>
        public const float OnSideWithin = 0.35f;

        /// <summary>Natural get-ups: each is this much faster or slower than the last at most (a share of its length), so fighters felled together do not rise in step.</summary>
        public const float GetUpVaries = 0.08f;

        /// <summary>Get-ups, and for the reports the last one's measures: its route, how far it turned to its threat (degrees), the fastest a part moved early (m/s), the furthest a planted hand or foot was from its key (metres), how far planted ones slid (metres).</summary>
        public int GetUps;
        public readonly List<string> GetUpLog = new List<string>();
        public string GetUpRoute = "";
        public float GetUpTurned, GetUpFastest, GetUpMiss, GetUpSlid;

        /// <summary>How long a fighter's arms take to go from where the get-up left them to what a standing fighter's arms do (seconds).</summary>
        public const float RoseEase = 0.4f;

        private readonly PoseBuffer rose = new PoseBuffer(), roseNow = new PoseBuffer(), roseMix = new PoseBuffer();
        private float roseAt = -1f, getUpLogAt = -1f, getUpDuration;

        private static bool IsArm(PartId id) =>
            id == PartId.UpperArmL || id == PartId.ForearmL || id == PartId.HandL || id == PartId.UpperArmR || id == PartId.ForearmR || id == PartId.HandR;

        /// <summary>Just up: the arms eased from where the get-up left them into the pose the fight has for them now, and what is seen of them measured a moment longer.</summary>
        private void EaseArmsFromGetUp(float dt)
        {
            if (roseAt >= 0f)
            {
                float w = (clock - roseAt) / RoseEase;
                // (Not through a blow of its own: a strike has its arm at once.)
                if (w >= 1f || clock < weaponArmHeldUntil)
                {
                    roseAt = -1f;
                }
                else
                {
                    w = w * w * (3f - 2f * w);
                    roseNow.Capture(Anim);
                    roseMix.CopyFrom(rose);
                    roseMix.MixToward(roseNow, id => IsArm(id) ? w : 1f);
                    roseMix.Apply(Anim, false);
                }
            }

            MeasureArmsSeen(dt);
            if (getUpLogAt >= 0f && clock >= getUpLogAt)
            {
                LogGetUp();
            }
        }

        /// <summary>The last get-up said, with what was seen of it up to a moment after it stood.</summary>
        private void LogGetUp()
        {
            if (getUpLogAt < 0f)
            {
                return;
            }

            getUpLogAt = -1f;
            GetUpLog.Add($"got up {GetUpRoute} in {getUpDuration:0.00} s, turned {GetUpTurned:0} degrees toward what it fights; the fastest a part moved early {GetUpFastest:0.0} m/s ({getUp.Fastest}); a planted hand or foot at most {100f * GetUpMiss:0} cm from where its pose wanted it ({getUp.Missed}), and they slid {100f * GetUpSlid:0} cm in all; an arm in its own body at most {100f * getUp.ArmDeepest:0} cm, more than 2 cm in for {getUp.ArmInFor:0.00} s; what it holds at most {100f * getUp.BladeDeepest:0} cm, for {getUp.BladeInFor:0.00} s; as seen (the physical body): an arm in its body for {GetUpArmSeenIn:0.00} s, what it holds for {GetUpBladeSeenIn:0.00} s");
            if (Diagnostics.PlaytestTrace.Enabled)
            {
                Diagnostics.PlaytestTrace.Note(FightUnit, "getup", GetUpLog[GetUpLog.Count - 1]);
            }
        }

        private static readonly PartId[] TrunkSolids = { PartId.Pelvis, PartId.Spine, PartId.Chest, PartId.Head, PartId.ThighL, PartId.ThighR };
        private static readonly PartId[] BladeSolids = { PartId.Pelvis, PartId.Spine, PartId.Chest, PartId.Head, PartId.ThighL, PartId.ThighR, PartId.ShinL, PartId.ShinR };

        /// <summary>For the reports: how long through the last get-up the body as it is seen (the physical one) had a forearm or hand inside its own trunk, head or thighs, and what its right hand holds inside its trunk, head or legs (seconds).</summary>
        public float GetUpArmSeenIn, GetUpBladeSeenIn;

        /// <summary>Is any of some points along a line inside any of some of its own parts' solids, as the physical body has them.</summary>
        private bool SeenInside(Vector3 from, Vector3 to, PartId[] solids)
        {
            foreach (var id in solids)
            {
                var part = Rig.Part(id);
                if (part == null || part.partCollider == null || Rig.IsSevered(id))
                {
                    continue;
                }

                for (int i = 0; i <= 8; i++)
                {
                    Vector3 p = Vector3.Lerp(from, to, i / 8f);
                    if ((part.partCollider.ClosestPoint(p) - p).sqrMagnitude < 1e-8f)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>For the reports: how long a standing fighter has had a forearm or hand inside its own trunk, head or thighs, and what its right hand holds inside its body, as seen (seconds), and the longest such stretch.</summary>
        public float ArmSeenIn, BladeSeenIn, ArmSeenInLongest;
        private float armInFor, bladeInFor;

        /// <summary>Standing: what is seen of its arms against its own body, measured while a playtest is recorded (and headless).</summary>
        public static bool MeasuresArms;

        private void MeasureArmsSeen(float dt)
        {
            if (!(MeasuresArms || Diagnostics.PlaytestTrace.Enabled) || Authority == null)
            {
                return;
            }

            float armWas = GetUpArmSeenIn, bladeWas = GetUpBladeSeenIn;
            MeasureGetUpSeen(dt);
            bool arm = GetUpArmSeenIn > armWas, blade = GetUpBladeSeenIn > bladeWas;
            if (getUpLogAt < 0f)
            {
                // (Not a get-up's: put back.)
                GetUpArmSeenIn = armWas;
                GetUpBladeSeenIn = bladeWas;
            }

            ArmSeenIn += arm ? dt : 0f;
            BladeSeenIn += blade ? dt : 0f;
            Stretch(ref armInFor, arm, dt, "a forearm or hand");
            Stretch(ref bladeInFor, blade, dt, "what its right hand holds");
        }

        private void Stretch(ref float lasted, bool inside, float dt, string what)
        {
            if (inside)
            {
                lasted += dt;
                return;
            }

            if (lasted >= 0.15f)
            {
                ArmSeenInLongest = Mathf.Max(ArmSeenInLongest, lasted);
                if (Diagnostics.PlaytestTrace.Enabled)
                {
                    Diagnostics.PlaytestTrace.Note(FightUnit, "arms", $"{what} was inside its own body for {lasted:0.00} s, as seen ({Loco.Current} {(Loco.Shot != null ? Loco.Shot.Name : "")}; {LastReaction})");
                }
            }

            lasted = 0f;
        }

        private void MeasureGetUpSeen(float dt)
        {
            bool arm = false;
            for (int a = 0; a < 2; a++)
            {
                Vector3 elbow = Rig.Part(a == 0 ? PartId.ForearmL : PartId.ForearmR).transform.position;
                Vector3 wrist = Rig.Part(a == 0 ? PartId.HandL : PartId.HandR).transform.position;
                arm |= SeenInside(Vector3.Lerp(elbow, wrist, 0.3f), wrist + (wrist - elbow).normalized * (0.08f * Size), TrunkSolids);
            }

            GetUpArmSeenIn += arm ? dt : 0f;
            var slot = Rig.SlotWeaponR;
            if (stance != Stances.Open && slot != null)
            {
                Vector3 along = slot.rotation * Vector3.forward;
                GetUpBladeSeenIn += SeenInside(slot.position + along * (0.15f * Size), slot.position + along * Poser.WeaponLength, BladeSolids) ? dt : 0f;
            }
        }

        private void BeginGetUp()
        {
            GetUpArmSeenIn = GetUpBladeSeenIn = 0f;
            var pelvis = Rig.Part(PartId.Pelvis).body;
            Vector3 front = pelvis.rotation * Vector3.forward;
            var kind = front.y > 0f ? GetUp.Kind.FaceUp : GetUp.Kind.FaceDown;
            getUp.Natural = NaturalGetUps;
            getUp.Side = 0f;
            Vector3? threat = null;
            if (NaturalGetUps)
            {
                // (On a side: the route from its front, rolling onto it as it pushes up.)
                if (Mathf.Abs(front.y) < OnSideWithin)
                {
                    kind = GetUp.Kind.FaceDown;
                    getUp.Side = (pelvis.rotation * Vector3.right).y < 0f ? 1f : -1f;
                }

                // (What it fights: whoever it is on, or for a target of the sandbox's the way it is set to face.)
                if (FightUnit != null)
                {
                    var foe = FightUnit.Opponent;
                    threat = foe != null && foe.Clip != null ? foe.Clip.Loco.Position : FightUnit.DummyFace;
                }

                // (Each a little faster or slower than the one before, by turns.)
                getUp.Vary = 1f + GetUpVaries * (((GetUps + Mathf.Abs(Mathf.RoundToInt(3f * pelvis.position.x + 5f * pelvis.position.z))) % 5) - 2) / 2f;
            }

            getUp.StartKey = 0;
            if (catching && NaturalGetUps)
            {
                // (Caught on a knee and a hand: it rises from the half kneel. Caught sitting: from sitting on its hand.
                // Facing as its hips face.)
                kind = caughtSitting ? GetUp.Kind.FaceUp : GetUp.Kind.FaceDown;
                getUp.Side = 0f;
                getUp.StartKey = caughtSitting ? GetUp.SittingKey : GetUp.HalfKneelKey;
            }

            GetUps++;
            GetUpRoute = catching && NaturalGetUps ? (caughtSitting ? "from sitting, its fall caught" : "from a knee and a hand, its fall caught") : !NaturalGetUps ? (kind == GetUp.Kind.FaceDown ? "from its front, as before" : "from its back, as before")
                : getUp.Side != 0f ? (getUp.Side > 0f ? "from its right side" : "from its left side") : kind == GetUp.Kind.FaceDown ? "from its front" : "from its back";
            Vector3 headward = Rig.Part(PartId.Head).body.position - pelvis.position;
            headward.y = 0f;
            if (headward.sqrMagnitude < 0.01f * Size * Size)
            {
                headward = pelvis.rotation * Vector3.up;
                headward.y = 0f;
            }

            if (catching && NaturalGetUps)
            {
                // (Upright on a knee, a body has no length along the ground to stand up along: it stands up the way its hips face.)
                Vector3 ahead = front;
                ahead.y = 0f;
                headward = ahead.sqrMagnitude > 1e-4f ? ahead.normalized : Quaternion.Euler(0f, Loco.Facing, 0f) * Vector3.forward;
                if (caughtSitting)
                {
                    // (From its back a body stands up toward its feet: sitting, they are before it.)
                    headward = -headward;
                }
            }

            GetUp.Place(kind, pelvis.position, headward, body.SizeFactor, NaturalGetUps, threat, out var position, out float yaw, out GetUpTurned);
            position.y = groundLevel;
            getUpRoot.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
            getUpStance.Plant(position, yaw);
            // (Where it will stand: its walk placed there, the feet let go to land on their spots.)
            Loco.Place(position, yaw);
            Poser.ReleaseFeet();
            lying.Capture(Rig);
            // (A weapon is in its right hand with a shield or without: laid clear of its body as it gets up. With no
            // shield its free hand is the one that goes to the ground, and the sword stays up before it.)
            getUp.HoldsMain = stance != Stances.Open;
            getUp.BladeLength = stance != Stances.Open ? Poser.WeaponLength : 0f;
            getUp.PostWith = stance == Stances.Sword ? PhysicsBattler.Motion.Stance.Left : -1;
            getUp.HoldsOff = stance == Stances.Shield;
            getUp.Begin(kind, lying);
            if (fallBody && Upper != null)
            {
                // (Taken up where it lies: the pose put on how it lies, and the body held to it again. After the
                // get-up's keys are made: making them leaves the pose standing, and a body taken up before that was
                // carried to its feet for one step and back. Seen in Unity on 14 November, at every get-up.)
                lying.Apply(Anim, false);
                Upper.TakeUp();
                foreach (var id in Upper.Own)
                {
                    var m = MuscleOf(id, 1f);
                    Upper.SetMuscle(id, m.hz, m.most);
                    Upper.SetTight(id, 1f);
                }
            }
            else
            {
                fallBody = false;
            }

            Footing = Footings.GettingUp;
            footingTime = 0f;
            catching = false;
            LastReaction = $"getting up {GetUpRoute}";
        }

        /// <summary>The body part a collider is, if it is one of the dummy's.</summary>
        public bool PartOf(Collider collider, out PartId id) => parts.TryGetValue(collider, out id);

        /// <summary>Where a part of it is now, as posed (world).</summary>
        public Vector3 PartAt(PartId id) => Fallen && Authority != null && Rig.Part(id).body != null ? Rig.Part(id).body.position : Anim.Part(id).position;

        /// <summary>Where to strike it, low, in the middle or high (0, 1, 2): its hips, the middle of its chest, its face; at its legs (-1): its knees.</summary>
        public Vector3 TargetAt(int height) =>
            (height < 0 ? 0.5f * (PartAt(PartId.ShinL) + PartAt(PartId.ShinR))
            : height == 0 ? PartAt(PartId.Pelvis)
            : height == 1 ? 0.5f * (PartAt(PartId.Spine) + PartAt(PartId.Chest))
            : PartAt(PartId.Head) + Vector3.up * (0.08f * Size)) - Voided;

        private float BasePin(BodyRegion region) =>
            region == BodyRegion.ArmL || region == BodyRegion.ArmR ? profile.armPin : profile.torsoPin;

        private void Hold(BodyRegion region, float share)
        {
            if (Authority == null)
            {
                return;
            }

            if (region == BodyRegion.Legs)
            {
                Authority.Set(BodyRegion.Legs, AuthorityLevel.Kinematic, 1f);
                return;
            }

            Authority.Set(region, AuthorityLevel.Pinned, BasePin(region) * share);
        }

        /// <summary>
        /// One step of the lab's dummy, steering itself: it stands in its guard facing <paramref name="faceToward"/>
        /// (held still through a hit stop, <paramref name="frozen"/>), back to its spot after a blow and to its
        /// standing distance, the shield raised if it blocks, the physical body held to it.
        /// </summary>
        public void Step(float dt, Vector3 faceToward, bool frozen)
        {
            clock += dt;
            if (Fallen)
            {
                if (!frozen)
                {
                    StepFallen(dt);
                }

                return;
            }

            Vector3 want = Vector3.zero;
            float yaw = Loco.Facing;
            if (!frozen)
            {
                Steer(faceToward, out want, out yaw);
            }

            MoveOn(dt, want, yaw, faceToward, frozen);
            Apply(dt);
        }

        /// <summary>
        /// The dummy's own steering: facing <paramref name="faceToward"/>, back onto its spot a while after a blow,
        /// and back to its standing distance just after one (as a fencer recovers the measure).
        /// </summary>
        private void Steer(Vector3 faceToward, out Vector3 want, out float yaw)
        {
            Vector3 to = faceToward - Loco.Position;
            yaw = new Vector2(to.x, to.z).sqrMagnitude > 0.01f ? Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg : Loco.Facing;
            // (Knocked off its spot, it steps back onto it once it has stood a moment, as a sparring partner would.)
            Vector3 home = Home - Loco.Position;
            home.y = 0f;
            want = Vector3.zero;
            Vector3 fromOpponent = Loco.Position - faceToward;
            fromOpponent.y = 0f;
            float apart = fromOpponent.magnitude;
            Vector3 away = apart > 1e-3f ? fromOpponent / apart : -(Quaternion.Euler(0f, Loco.Facing, 0f) * Vector3.forward);
            if (Loco.Current != Locomotor.Mode.OneShot && clock - lastHit > HomeAfter && home.magnitude > HomeFrom * Size)
            {
                want = Vector3.ClampMagnitude(home * 2f, HomeSpeed * Mathf.Sqrt(Size));
                // (Not back onto its spot through the other body: no nearer it than the standing distance.)
                float keepOff = Mathf.Max(KeepDistance + 0.15f * Size, StandDistance);
                if (keepOff > 0f && apart < keepOff && Vector3.Dot(want, away) < 0f)
                {
                    want -= away * Vector3.Dot(want, away);
                }
            }

            // (Closer than it stands between blows just after a blow: it steps back to there, as a fencer recovers
            // the measure. Not otherwise: one walking in to strike is let come.)
            float sinceBlow = clock - lastBlowAt;
            if (StandDistance > 0f && apart < StandDistance - 0.05f * Size && Loco.Current != Locomotor.Mode.OneShot && !Blow.HasValue
                && clock - lastHit > StandBackAfter && sinceBlow > StandBackAfter && sinceBlow < StandBackWithin)
            {
                want = away * Mathf.Clamp((StandDistance - apart) * 2.5f, 0.35f, 1.2f) * Mathf.Sqrt(Size);
            }

            // (Its own blows: a strike turns the body as it was aimed, and a combo pressing after the one it drove back
            // walks it in.)
            if (Combo != null && Combo.Pressing)
            {
                want = Combo.Want;
                yaw = Combo.WantFacing;
            }
            else if (Striking)
            {
                want = Vector3.zero;
                yaw = Loco.Facing;
            }
        }

        /// <summary>
        /// The first half of a step, steered from outside (the lab's walker, a fight unit): the walk toward
        /// <paramref name="want"/> (m/s) facing <paramref name="yaw"/> (degrees), the strike or combo playing, the
        /// pose, and (when it <see cref="Defends"/>) the defence against <paramref name="opponent"/> (where the
        /// one it faces is; world). Held still through a hit stop (<paramref name="frozen"/>). Then the second half:
        /// <see cref="Apply"/>.
        /// </summary>
        public void Move(float dt, Vector3 want, float yaw, Vector3 opponent, bool frozen)
        {
            clock += dt;
            if (Fallen)
            {
                if (!frozen)
                {
                    StepFallen(dt);
                }

                return;
            }

            MoveOn(dt, want, yaw, opponent, frozen);
        }

        /// <summary>What paces its steps near its enemy (the plan "Grounded legs", step 1): stand, step, stand.</summary>
        public readonly StepPacer Pacer = new StepPacer();

        /// <summary>
        /// For the logs, what its legs are doing (stand and step): 0 standing still, 1 stepping in its stance, 2 a step
        /// walked with the guard's walk, 3 walking as fast as it is asked, 4 walking off a shove, 5 in a clip, 6 down
        /// (fallen, or not on its pose).
        /// </summary>
        public int StepState
        {
            get
            {
                if (Fallen || !snapped)
                {
                    return 6;
                }

                if (Loco.Current == Locomotor.Mode.OneShot)
                {
                    return 5;
                }

                if (Loco.Current == Locomotor.Mode.Idle)
                {
                    return Loco.Going.sqrMagnitude > 0.02f ? 1 : 0;
                }

                return Loco.Shoved ? 4 : Loco.Paced ? 2 : 3;
            }
        }

        /// <summary>For the reports: called each step with the velocity the steering asked and the one the walk was asked.</summary>
        public static System.Action<ClipFighter, Vector3, Vector3> PaceTrace;

        /// <summary>The legs' opinion of the body as the physics last left it (the plan "Grounded legs", step 0): read each step, it moves nothing.</summary>
        public readonly Grounding Ground = new Grounding();

        private float groundReadAt = -10f;

        private float restAnkle = -1f;

        private void ReadGround(float dt)
        {
            if (restAnkle < 0f)
            {
                restAnkle = Anim.RestLocalPosition(PartId.Pelvis).y + Anim.RestLocalPosition(PartId.ThighL).y + Anim.RestLocalPosition(PartId.ShinL).y + Anim.RestLocalPosition(PartId.FootL).y;
            }

            // (Or not read for a while: down, it is not read at all, and read again as it got up, each foot was taken to
            // have slid in one step from where it stood before it fell: the legs' overlay flashed a slide, and the
            // reports counted every knock-down as metres of sliding in the clip it got up into.)
            if (Fallen || !snapped || clock - groundReadAt > 2.5f * dt)
            {
                Ground.Forget();
            }

            groundReadAt = clock;
            Ground.Read(Rig, groundLevel, Size, restAnkle, dt);
            // (The weight over the feet, the plan's step 3: standing on both feet with its weight off what they hold, the
            // hips are told which way it has to go.)
            Poser.WeightOff = Vector3.zero;
            if (Ground.Known && Ground.FeetDown == 2 && Ground.WeightOut > 0f)
            {
                var weight = new Vector2(Ground.Weight.x, Ground.Weight.z);
                var onto = Grounding.Nearest(Ground.Support, weight);
                // (And a little further: brought only to the edge of what they hold, it stood on the edge.)
                Vector2 off = onto - weight;
                off *= 1f + 0.02f * Size / Mathf.Max(1e-4f, off.magnitude);
                Poser.WeightOff = new Vector3(off.x, 0f, off.y);
            }
        }

        private void MoveOn(float dt, Vector3 want, float yaw, Vector3 opponent, bool frozen)
        {
            ReadGround(dt);
            if (frozen)
            {
                return;
            }

            GiveGround(dt);
            PressedBack(dt);

            // (Too near the other body: it steps back to the fighting distance, as anyone would.)
            Vector3 fromOpponent = Loco.Position - opponent;
            fromOpponent.y = 0f;
            float apart = fromOpponent.magnitude;
            Vector3 away = apart > 1e-3f ? fromOpponent / apart : -(Quaternion.Euler(0f, Loco.Facing, 0f) * Vector3.forward);
            if (KeepDistance > 0f && apart < KeepDistance && Loco.Current != Locomotor.Mode.OneShot && pushAt < 0f && clock - lastSeparate > 0.25f)
            {
                Loco.Push(away * Mathf.Clamp((KeepDistance - apart) * 3f, 0.3f, 1.2f) * Mathf.Sqrt(Size));
                lastSeparate = clock;
            }

            // (Not while its feet are both off the ground: a push would cut its jump off in the air for the walk's catch
            // steps, the body drifting down to the floor. It is driven back as it lands.)
            if (pushAt >= 0f && clock >= pushAt && !Loco.ShotAirborne(0f))
            {
                // (Thrown back stumbling: that one is walked off, as it was; with the legs deciding, only one too fast
                // for the stance to step after is.)
                Loco.Push(pendingKnock, !CombatFeel.LegsDecide && clock - thrownAt < 0.5f);
                pushAt = -1f;
                pendingKnock = Vector3.zero;
            }

            HurrySideHit();
            Vector3 asked = want;
            // (Stand and step: near its enemy the steering's small velocity is not walked in slow motion; it stands until
            // a step's worth is asked, and steps it.)
            if (Loco.Current != Locomotor.Mode.OneShot && Loco.Guarding)
            {
                want = Pacer.Pace(dt, want, Loco.Velocity * dt, Size, Locomotor.PacedHalfLife / 0.6931f, !Loco.Shoved);
                // (The walk takes a step's velocity up, and comes to rest from it, in a step's time; a shove is walked
                // off as it was, see Locomotor.Paced.)
                Loco.Paced = StepPacer.On && !Pacer.Walking;
            }
            else
            {
                Pacer.Forget();
                Loco.Paced = false;
            }

            Loco.QuickSteps = StepPacer.On;
            PaceTrace?.Invoke(this, asked, want);

            Loco.Step(dt, want, yaw);
            CarryBy(Loco.SnappedThisStep);
            if (Combo != null)
            {
                Combo.Step(dt);
            }

            // (Its own strike taking the sword arm from the parry: the parry's arms and trunk, as they were last step,
            // faded into the strike's, not dropped in a frame.)
            if (Defends && stance == Stances.Sword && Parry != null && Striking && !parryWaited)
            {
                CaptureHandOff();
            }

            // (A strike alone follows its target too, as far as its pull goes.)
            if (Follow != null && (Combo == null || !Combo.Playing) && Loco.Current == Locomotor.Mode.OneShot && Loco.Shot != null && Loco.Shot.ContactFrame >= 0)
            {
                var at = Follow(Loco.Shot);
                if (at.HasValue)
                {
                    Poser.Retarget(at.Value);
                }
            }

            Poser.Pose(dt);
            MarkArms("the clip's arms and the strike");
            HandOff(dt);
            MarkArms("the hand-off");
            if (!Defends)
            {
                return;
            }

            if (stance == Stances.Sword && Parry != null)
            {
                // (The body's sway first, then the parry solved on it: swayed after, the sword went off its line.)
                React(dt);
                MarkArms("the reaction to a blow");
                if (Striking)
                {
                    // (Its own strike has the sword arm: the parry waits, and takes it up from where the strike leaves it.)
                    parryWaited = true;
                }
                else
                {
                    if (parryWaited)
                    {
                        Parry.Resync();
                        parryWaited = false;
                    }

                    Parry.Skill = ParrySkillNow;
                    Parry.Step(dt, Blow, opponent);
                    MarkArms("the parry and the free arm");
                    MaybeRetire(opponent);
                }
            }
            else
            {
                RaiseShield(dt, opponent);
                MarkArms("the shield");
                MaybeRetire(opponent);
                CountArmCut();
                React(dt);
                MarkArms("the reaction to a blow");
            }

            Recover(dt);
            MarkArms("the recovery");
            Poise = Mathf.Min(MaxPoise, Poise + PoiseBack * MaxPoise * dt);
        }

        /// <summary>How far past a body's range the pose had each arm this step, before the gate brought it back (degrees; left, right; 0: not touched).</summary>
        public readonly float[] ArmHeldBy = new float[2];

        /// <summary>For the reports: how far the gate moved each wrist from where the pose had it this step (metres), and how far it turned each hand (degrees): left, right.</summary>
        public readonly float[] ArmMovedBy = new float[2], HandTurnedBy = new float[2];

        /// <summary>For the reports: steps the gate held an arm's speed, by what it held (the elbow's place, its bend, its fold, the wrist, the forearm's roll, the hand on the forearm): the weapon arm in a strike, and every other arm and time.</summary>
        public readonly int[] ArmSlowedStriking = new int[6], ArmSlowedElse = new int[6];

        private readonly int[,] slowedSeen = new int[2, 6];

        /// <summary>
        /// The gate at the end of the pose (the plan "Arms that bend as a body does", step 2): after every stage has had
        /// its say (the clip, the strike's aim, the shield, the parry, the knocks, the free arm), each arm is read as a
        /// body's arm and brought into a body's range (<see cref="ArmLimits"/>). What a stage asked for that no arm does,
        /// it does not get: the hand stops short. A hand bound to a two-handed weapon is left to its grip.
        /// </summary>
        private readonly ArmGate[] armGates = { new ArmGate(), new ArmGate() };
        private readonly (Quaternion, Quaternion, Quaternion)[] armSent = new (Quaternion, Quaternion, Quaternion)[2], armGone = new (Quaternion, Quaternion, Quaternion)[2];
        private readonly bool[] armTurnsKnown = new bool[2];

        /// <summary>For the reports (<see cref="TraceArmJumps"/>): which stage of the pose first turned an arm's joint by a jump in a step, counted by stage and arm, and each written out.</summary>
        public readonly Dictionary<string, int> ArmJumpsBy = new Dictionary<string, int>();

        public System.Action<string> ArmJumpTrace;

        /// <summary>Whether the stages of the pose are watched for jumps of an arm's joints (the reports).</summary>
        public static bool TraceArmJumps;

        private readonly List<string> armStages = new List<string>();
        private readonly List<Quaternion[]> armStageTurns = new List<Quaternion[]>();
        private Dictionary<string, Quaternion[]> armGave = new Dictionary<string, Quaternion[]>(), armGaveLast = new Dictionary<string, Quaternion[]>();
        private readonly Quaternion[] armLeft = new Quaternion[6];
        private bool armLeftKnown;

        private static readonly PartId[] ArmJoints6 = { PartId.UpperArmL, PartId.ForearmL, PartId.HandL, PartId.UpperArmR, PartId.ForearmR, PartId.HandR };

        /// <summary>For the reports: the arms' joints as a stage of the pose has just left them.</summary>
        private void MarkArms(string stage)
        {
            if (!TraceArmJumps)
            {
                return;
            }

            var turns = new Quaternion[6];
            for (int i = 0; i < 6; i++)
            {
                turns[i] = Anim.Part(ArmJoints6[i]).localRotation;
            }

            armStages.Add(stage);
            armStageTurns.Add(turns);
        }

        /// <summary>
        /// For the reports: at the end of the pose's stages, whether an arm's joint was left turned by a jump from where
        /// the stages left it the step before, and if so which stage did it: the one whose own turn of the joint (what
        /// it left against what it was given) changed the most since the step before. (A stage that only hands on what
        /// another dropped, the hand-off from a parry to a strike, changes as much as the one that dropped it and the
        /// arm is left where it was: no jump, and none counted.)
        /// </summary>
        private void TraceJumps()
        {
            if (!TraceArmJumps || armStages.Count == 0)
            {
                armStages.Clear();
                armStageTurns.Clear();
                return;
            }

            armGave = new Dictionary<string, Quaternion[]>();
            for (int k = 0; k < armStages.Count; k++)
            {
                var gave = new Quaternion[6];
                for (int i = 0; i < 6; i++)
                {
                    gave[i] = k == 0 ? armStageTurns[k][i] : Quaternion.Inverse(armStageTurns[k - 1][i]) * armStageTurns[k][i];
                }

                armGave[armStages[k]] = gave;
            }

            var left = armStageTurns[armStageTurns.Count - 1];
            if (armLeftKnown)
            {
                for (int side = 0; side < 2; side++)
                {
                    float by = 0f;
                    int joint = 0;
                    for (int j = 0; j < 3; j++)
                    {
                        float turned = Quaternion.Angle(left[side * 3 + j], armLeft[side * 3 + j]);
                        if (turned > by)
                        {
                            by = turned;
                            joint = j;
                        }
                    }

                    if (by <= ArmJump)
                    {
                        continue;
                    }

                    string did = "?";
                    float most = -1f;
                    var stages = new HashSet<string>(armGave.Keys);
                    stages.UnionWith(armGaveLast.Keys);
                    foreach (string stage in stages)
                    {
                        float changed = 0f;
                        for (int j = 0; j < 3; j++)
                        {
                            var now = armGave.TryGetValue(stage, out var g) ? g[side * 3 + j] : Quaternion.identity;
                            var before = armGaveLast.TryGetValue(stage, out var l) ? l[side * 3 + j] : Quaternion.identity;
                            changed = Mathf.Max(changed, Quaternion.Angle(now, before));
                        }

                        if (changed > most)
                        {
                            most = changed;
                            did = stage;
                        }
                    }

                    bool isLeft = side == 0;
                    string key = did + ", " + (isLeft ? "left" : "right") + " " + (joint == 0 ? "upper arm" : joint == 1 ? "forearm" : "hand");
                    ArmJumpsBy[key] = ArmJumpsBy.TryGetValue(key, out int n) ? n + 1 : 1;
                    if (ArmJumpTrace != null)
                    {
                        Transform u = Anim.Part(isLeft ? PartId.UpperArmL : PartId.UpperArmR), f = Anim.Part(isLeft ? PartId.ForearmL : PartId.ForearmR), h = Anim.Part(isLeft ? PartId.HandL : PartId.HandR);
                        var read = ArmLimits.Read(Anim.Part(PartId.Chest).rotation, u.position, f.position, h.position, h.rotation, isLeft);
                        ArmJumpTrace(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                            "{0:0.00} s {1}: {2:0}° (upper {3:0} fore {4:0} hand {5:0}; the stage's own turn changed {6:0}°); {7}{8}{9}, struck {10:0.00} s ago; bent {11:0}, raised {12:0} toward {13:0}, turned {14:0}, rolled {15:0}, wrist {16:0}/{17:0}",
                            clock, key, by, Quaternion.Angle(left[side * 3], armLeft[side * 3]), Quaternion.Angle(left[side * 3 + 1], armLeft[side * 3 + 1]), Quaternion.Angle(left[side * 3 + 2], armLeft[side * 3 + 2]), most,
                            Loco.Current, Loco.Shot != null ? " " + Loco.Shot.Name : "", Striking ? " striking" : "", SinceStruck,
                            read.Bend, read.Raised, read.Toward, read.Turn, read.Roll, read.Flex, read.Thumbward));
                    }
                }
            }

            for (int i = 0; i < 6; i++)
            {
                armLeft[i] = left[i];
            }

            armLeftKnown = true;
            armGaveLast = armGave;
            armStages.Clear();
            armStageTurns.Clear();
        }

        /// <summary>A joint of an arm turned this far in a step counts as a jump (degrees; for the reports).</summary>
        public const float ArmJump = 60f;

        /// <summary>For the reports: steps an arm's joint jumped in the pose as it was sent to the gate, as it left it, and those the gate itself made (the pose sent had turned under half as far).</summary>
        public int ArmSentJumps, ArmGoneJumps, ArmGateJumps;

        /// <summary>For the reports: each jump the gate made, written out.</summary>
        public System.Action<string> ArmGateTrace;

        private void HoldArms(float dt)
        {
            MarkArms("what came after (the fight's own turns)");
            TraceJumps();
            ArmHeldBy[0] = ArmHeldBy[1] = 0f;
            ArmMovedBy[0] = ArmMovedBy[1] = HandTurnedBy[0] = HandTurnedBy[1] = 0f;
            if (!ArmLimits.On || !snapped)
            {
                armGates[0].Forget();
                armGates[1].Forget();
                armTurnsKnown[0] = armTurnsKnown[1] = false;
                return;
            }

            if (CombatFeel.ShieldFixed && Poser.CarryL == LocoBody.Carry.Shield && !Rig.IsSevered(PartId.HandL))
            {
                Transform su = Anim.Part(PartId.UpperArmL), sf = Anim.Part(PartId.ForearmL), sh = Anim.Part(PartId.HandL);
                KeepShieldOff(su, sf, sh, TwoBoneIK.BendWay(su, sf, sh));
            }

            var chest = Anim.Part(PartId.Chest);
            for (int side = 0; side < 2; side++)
            {
                bool left = side == 0;
                if ((left && Poser.TwoHanded) || Rig.IsSevered(left ? PartId.HandL : PartId.HandR))
                {
                    armGates[side].Forget();
                    armTurnsKnown[side] = false;
                    continue;
                }

                var clip = ArmLimits.ReadClip(Loco.RawPoints, (int)LocoBone.Neck, (int)LocoBone.Spine2, (int)LocoBone.LeftArm, (int)LocoBone.RightArm, left);
                Transform upper = Anim.Part(left ? PartId.UpperArmL : PartId.UpperArmR), fore = Anim.Part(left ? PartId.ForearmL : PartId.ForearmR), hand = Anim.Part(left ? PartId.HandL : PartId.HandR);
                var sentTurns = (upper.localRotation, fore.localRotation, hand.localRotation);
                var sentAs = ArmGateTrace != null ? ArmLimits.Read(chest.rotation, upper.position, fore.position, hand.position, hand.rotation, left, clip) : default;
                Vector3 wristSent = hand.position;
                var handSent = hand.rotation;
                ArmHeldBy[side] = armGates[side].Hold(Anim, left, clip, dt);
                ArmMovedBy[side] = Vector3.Distance(wristSent, hand.position);
                HandTurnedBy[side] = Quaternion.Angle(handSent, hand.rotation);
                // (For the reports: what of the arm's speed the gate held this step, the weapon arm in a strike apart.)
                for (int k = 0; k < 6; k++)
                {
                    int now = armGates[side].Slowed[k];
                    if (now != slowedSeen[side, k])
                    {
                        slowedSeen[side, k] = now;
                        (Striking && (side == 1 || Poser.TwoHanded) ? ArmSlowedStriking : ArmSlowedElse)[k]++;
                    }
                }
                // (For the reports: a joint the gate turned far in a step that the pose sent had hardly turned.)
                var goneTurns = (upper.localRotation, fore.localRotation, hand.localRotation);
                if (armTurnsKnown[side])
                {
                    float sentBy = Mathf.Max(Quaternion.Angle(sentTurns.Item1, armSent[side].Item1), Mathf.Max(Quaternion.Angle(sentTurns.Item2, armSent[side].Item2), Quaternion.Angle(sentTurns.Item3, armSent[side].Item3)));
                    float goneBy = Mathf.Max(Quaternion.Angle(goneTurns.Item1, armGone[side].Item1), Mathf.Max(Quaternion.Angle(goneTurns.Item2, armGone[side].Item2), Quaternion.Angle(goneTurns.Item3, armGone[side].Item3)));
                    ArmSentJumps += sentBy > ArmJump ? 1 : 0;
                    ArmGoneJumps += goneBy > ArmJump ? 1 : 0;
                    if (goneBy > ArmJump)
                    {
                        ArmGateJumps += sentBy < 0.5f * ArmJump ? 1 : 0;
                        if (ArmGateTrace != null)
                        {
                            var goneAs = ArmLimits.Read(chest.rotation, upper.position, fore.position, hand.position, hand.rotation, left, clip);
                            ArmGateTrace(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                                "{0:0.00} s {1}: joints turned {2:0}° (sent {3:0}°): upper {4:0} fore {5:0} hand {6:0}; sent raised {7:0} toward {8:0} turned {9:0} bent {10:0} folded {11:0.00} rolled {12:0} wrist {13:0}/{14:0} (past {15:0}); let go raised {16:0} toward {17:0} turned {18:0} bent {19:0} rolled {20:0} wrist {21:0}/{22:0}",
                                clock, left ? "left" : "right", goneBy, sentBy, Quaternion.Angle(goneTurns.Item1, armGone[side].Item1), Quaternion.Angle(goneTurns.Item2, armGone[side].Item2), Quaternion.Angle(goneTurns.Item3, armGone[side].Item3),
                                sentAs.Raised, sentAs.Toward, sentAs.Turn, sentAs.Bend, sentAs.Folded, sentAs.Roll, sentAs.Flex, sentAs.Thumbward, sentAs.Worst,
                                goneAs.Raised, goneAs.Toward, goneAs.Turn, goneAs.Bend, goneAs.Roll, goneAs.Flex, goneAs.Thumbward));
                        }
                    }
                }

                armSent[side] = sentTurns;
                armGone[side] = goneTurns;
                armTurnsKnown[side] = true;
            }
        }

        /// <summary>
        /// The second half of a step: the arms held to their pose where they must be (the weapon arm through a
        /// strike and a moment after, both two-handed; the shield arm meeting a blow; the sword arm parrying), the
        /// shield turned in the physical hand as in the posed one, and the physical body driven to the pose for the
        /// coming physics step (or, the first step, put on it).
        /// </summary>
        public void Apply(float dt)
        {
            if (Fallen)
            {
                return;
            }

            HoldArms(dt);
            EaseArmsFromGetUp(dt);

            if (Authority != null)
            {
                // (Through a strike and a moment after it the weapon arm is held exactly to its pose, kinematic, as
                // the fight holds it in a blow: pinned, the physical arm, and the weapon seen on it and swept for
                // hits, trailed a 16 m/s swing by tens of centimetres and passed by what the pose struck.)
                if (Strikes != null && Loco.Current == Locomotor.Mode.OneShot && Loco.Shot != null && Loco.Shot.ContactFrame >= 0)
                {
                    weaponArmHeldUntil = clock + WeaponArmHeldAfter;
                }

                bool striking = clock < weaponArmHeldUntil;
                // (Meeting a blow, the shield arm is held exactly to its pose, as the fight holds a weapon arm in a
                // blow: pinned, the physical arm and the shield on it would trail the pose.)
                if ((Defends && ShieldArmHeld) || (striking && Poser.TwoHanded))
                {
                    Authority.Set(BodyRegion.ArmL, AuthorityLevel.Kinematic, 1f);
                }
                else if (pinLeft[(int)BodyRegion.ArmL] <= 0f)
                {
                    Hold(BodyRegion.ArmL, 1f);
                }

                // (Parrying, the sword arm is held exactly to its pose: pinned, the physical sword would trail the parry.)
                if (striking && StrikeArm != AuthorityLevel.Kinematic)
                {
                    // (The plan "A different approach", step 2: the weapon arm physical through a blow, on its muscles.)
                    Authority.Set(BodyRegion.ArmR, StrikeArm, StrikeArmStrength);
                }
                else if (striking || (Defends && stance == Stances.Sword && Parry != null))
                {
                    Authority.Set(BodyRegion.ArmR, AuthorityLevel.Kinematic, 1f);
                }
                else if (pinLeft[(int)BodyRegion.ArmR] <= 0f)
                {
                    Hold(BodyRegion.ArmR, 1f);
                }
            }

            // (The shield as turned in the hand: the shield that is seen, and blocks, is on the physical hand's slot.)
            if (Rig.SlotWeaponL != null && Anim.SlotWeaponL != null && (Upper == null || !Upper.Turns(PartId.HandL)))
            {
                Rig.SlotWeaponL.localRotation = Anim.SlotWeaponL.localRotation;
            }

            if (Authority != null && snapped)
            {
                if (carried.sqrMagnitude > 1e-10f)
                {
                    // (Every part, the kinematic legs too: moved after their pose instead, they swept across the
                    // ground, and into the other body, at the speed of the snap.)
                    foreach (var p in Rig.Parts)
                    {
                        if (p.body != null)
                        {
                            p.body.position += carried;
                        }
                    }
                }

                if (TorsoHold.HasValue)
                {
                    Authority.Set(BodyRegion.Torso, TorsoHold.Value, TorsoStrength);
                    Authority.Set(BodyRegion.Head, TorsoHold.Value, TorsoStrength);
                }

                Authority.Apply(dt);
                if (carried.sqrMagnitude > 0.25f)
                {
                    Upper?.Teleport();
                }

                StepUpper(dt);
            }
            else
            {
                Carry();
                snapped = true;
            }

            carried = Vector3.zero;
        }

        /// <summary>The physical body has been put on its pose (the first <see cref="Apply"/> carries it there; after that it is driven).</summary>
        public bool Snapped => snapped;

        // ---- Attacking -------------------------------------------------------------------------------------------

        /// <summary>Its strikes' hits (null until <see cref="EnableStrikes"/>): their blades swept against the bodies about it.</summary>
        public StrikeHits Strikes { get; private set; }

        /// <summary>Its combos (set by whoever plays them); stepped with its walk.</summary>
        public ComboPlayer Combo
        {
            get => combo;
            set
            {
                combo = value;
                if (combo != null)
                {
                    combo.Carried = move =>
                    {
                        RootShoves[2] += move.magnitude;
                        CarryBy(move);
                    };
                }
            }
        }

        private ComboPlayer combo;

        /// <summary>
        /// The walk was moved this far at once, not walked (a combo's magnet or step in, two bodies kept apart, ground
        /// given): the physical body goes with it at the next <see cref="Apply"/>, a teleport keeping its speed. Left
        /// behind, the pinned upper body trailed the Large's snaps after a dummy driven off (up to 10 m/s) by 0.8 m
        /// over its kinematic legs, and the lab took it for a knock-down, throwing it onto the dummy.
        /// </summary>
        public void CarryBy(Vector3 move)
        {
            move.y = 0f;
            carried += move;
        }

        private Vector3 carried;

        /// <summary>
        /// How far roots have been moved at once, not walked, since the run began (metres, all fighters): two bodies kept
        /// apart, ground given to a blow, a combo's strike snapped onto its target, a body pressed back. For the reports: what the walk
        /// doesn't own.
        /// </summary>
        public static readonly float[] RootShoves = new float[4];

        /// <summary>A strike alone follows its target, as far as the arm's pull goes: where its target is now (null: none to follow).</summary>
        public System.Func<LocoClip, Vector3?> Follow;

        /// <summary>
        /// How the weapon arm is held through a blow: kinematic (exactly on its pose, as it has been: nothing can stop
        /// it), or on its muscles at <see cref="StrikeArmStrength"/> (the plan "A different approach", step 2: the
        /// experiment).
        /// </summary>
        public AuthorityLevel StrikeArm = AuthorityLevel.Kinematic;

        // ---- The physical upper body (the plan "A different approach", step 5) -------------------------------------

        /// <summary>
        /// Clip fighters' upper bodies are physical: the pelvis, torso, head and both arms one articulation on muscles
        /// (<see cref="UpperBodyArticulation"/>), a solid sword and shield in its hands, the legs still the pose's. Off:
        /// the upper body is held to its pose by pins, as it has been. (The Combat Feel's.)
        /// </summary>
        public static bool PhysicalUpper;

        /// <summary>Its physical upper body while it has one (standing, whole): null when down, dead, or with the switch off.</summary>
        public UpperBodyArticulation Upper { get; private set; }

        /// <summary>What its hands hold, for the physical body: the colliders and their weight (the unit fills it as it arms it).</summary>
        public readonly List<(PartId hand, Collider solid, float mass, bool blade)> Solids = new List<(PartId, Collider, float, bool)>();

        /// <summary>
        /// Its muscles, at a Medium's size: how quick the torso's, the neck's and each arm's are (Hz: what hangs from a
        /// joint would swing at this undamped), and the most the spine and chest turn with (N m; the neck a quarter,
        /// a shoulder as much as the spine, an elbow 0.7 and a wrist 0.25 of it). From the experiments: with these a
        /// blade is within 11 cm of its pose in a strike's wind-up and at contact, stops on what it meets and gives
        /// it a blow's push.
        /// </summary>
        public static float TorsoQuick = 30f, NeckQuick = 30f, WeaponArmQuick = 40f, ShieldArmQuick = 30f, MuscleStrength = 600f;

        /// <summary>A region held less firmly (struck: its pin eased) has its muscles this much slower at the least, for as long.</summary>
        public static float LeastQuick = 0.25f;

        private bool upperOff, solidsChanged;
        private int upperLost;
        private float upperDt = 1f / 60f;
        private readonly float[] upperEase = { 1f, 1f, 1f, 1f, 1f };

        /// <summary>What its hands hold changed (armed, disarmed): the physical body is told at its next step.</summary>
        public void SolidsChanged() => solidsChanged = true;

        /// <summary>
        /// The most this body's strongest muscles turn with (N m): <see cref="MuscleStrength"/> at a Medium's size
        /// and weight, and for another body as much more as what its muscles have to turn is heavier (its weight
        /// times its size squared). A Large's arm with its club is five times a Medium's to turn; given only its
        /// size squared (1.5 times) its muscles were at their limit in every swing, and a muscle at its limit no
        /// longer damps: its upper body whirled.
        /// </summary>
        private float Strong => MuscleStrength * Size * Size * Mathf.Max(0.25f, Mass / MediumMass);

        private const float MediumMass = 80f;

        /// <summary>
        /// Clip fighters' legs are of the physical body too (the scene's Combat Feel; needs <see cref="PhysicalUpper"/>).
        /// Off, as it was before 14 November: the legs go where the pose puts them and nothing stops or moves them.
        /// </summary>
        public static bool PhysicalLegs = true;

        /// <summary>The legs' muscles: how quick (Hz), and how strong against the trunk's.</summary>
        public static float LegQuick = 40f, LegStrength = 1.5f;

        private bool builtWithLegs, builtHeld;

        /// <summary>
        /// The physical body's hips are held where the pose has them by a spring of limited strength, not locked there
        /// (the plan "Grounded legs", step 7; <see cref="UpperBodyArticulation.HipsSupported"/>). Off: locked, as before.
        /// </summary>
        public static bool HeldHips = true;

        /// <summary>For the reports: the momentum a blow's force gave its body in all (N·s), and how many blows had one.</summary>
        public float HitForceGiven;
        public int HitForces;

        /// <summary>For the reports: the furthest its hips have been from where the pose had them (metres), and how far they are on average.</summary>
        public float HipsOffMost, HipsOffSum;
        public int HipsOffSteps;

        /// <summary>
        /// A leg a blow lands on is knocked aside for this long (seconds), its muscles as quick and as strong as this
        /// share of what they are (most just struck, easing back).
        /// </summary>
        public const float LegKnockFor = 0.45f, LegKnockEase = 0.3f, LegKnockGive = 0.3f;

        private static readonly PartId[] LeftLeg = { PartId.ThighL, PartId.ShinL, PartId.FootL }, RightLeg = { PartId.ThighR, PartId.ShinR, PartId.FootR };
        private readonly float[] legKnocked = new float[2], legEase = { 1f, 1f };
        private readonly bool[] legLoose = new bool[2];

        /// <summary>Times a blow knocked one of its physical legs aside (for the reports).</summary>
        public int LegsKnocked;

        /// <summary>A leg's muscles as a blow has left them: <paramref name="soft"/> 1 just struck, 0 as they are.</summary>
        private void EaseLeg(int f, float soft)
        {
            float ease = Mathf.Lerp(1f, LegKnockEase, soft), give = Mathf.Lerp(1f, LegKnockGive, soft);
            bool loose = soft > 0.02f;
            if (Mathf.Abs(ease - legEase[f]) <= 0.02f && loose == legLoose[f])
            {
                return;
            }

            legEase[f] = ease;
            legLoose[f] = loose;
            foreach (var id in f == 0 ? LeftLeg : RightLeg)
            {
                if (Upper.Holds(id))
                {
                    var m = MuscleOf(id, 1f);
                    Upper.SetMuscle(id, m.hz * ease, m.most * give);
                    // (Held to its pose it is brought back at once, however weak its muscles: let go, it is left where
                    // the blow put it and comes back as an arm knocked aside does, then is held again.)
                    Upper.SetTight(id, loose ? 0f : 1f);
                }
            }
        }

        /// <summary>A physical leg's part is struck: sent the way of the blow at <paramref name="kick"/> (m/s), the leg let go for a moment.</summary>
        private void KnockLeg(PartId part, Vector3 kick)
        {
            if (Upper == null || !Upper.HasLegs || !Upper.Holds(part))
            {
                return;
            }

            int side = System.Array.IndexOf(LeftLeg, part) >= 0 ? 0 : 1;
            legKnocked[side] = LegKnockFor;
            LegsKnocked++;
            // (At once: the blow lands in this step's physics, and a leg still held as it stands takes it without moving.)
            EaseLeg(side, 1f);
            Upper.PinFoot(side, false, Poser.Feet[side].Target, Mathf.Max(0.25f, Mass / MediumMass), Size);
            Upper.Link(part).AddForce(Vector3.ClampMagnitude(kick, MostPartSpeed * Size), ForceMode.VelocityChange);
        }

        private (float hz, float most) MuscleOf(PartId id, float ease)
        {
            float strong = Strong;
            // (Real weapon weight: an arm that swings a weapon made heavier than its own weight is as much stronger, so
            // it still swings it as the clips do; with the muscles it has for the weapon as it is, a sword three times
            // as heavy came a third slower and trailed its swing by 30 cm.)
            float armed = CombatFeel.WeaponsHaveMass && CombatFeel.ArmFollowsMass ? Mathf.Max(1f, CombatFeel.WeaponMass) : 1f;
            float left = Poser != null && Poser.TwoHanded ? armed : 1f;
            switch (id)
            {
                case PartId.Spine:
                case PartId.Chest: return (TorsoQuick * ease, strong);
                case PartId.Head: return (NeckQuick * ease, 0.25f * strong);
                case PartId.UpperArmR: return (WeaponArmQuick * ease, strong * armed);
                case PartId.ForearmR: return (WeaponArmQuick * ease, 0.7f * strong * armed);
                case PartId.HandR: return (WeaponArmQuick * ease, 0.25f * strong * armed);
                // (The legs, when they are of the physical body (the plan "Grounded legs", step 6): as strong as the trunk
                // at the hip, less down the leg as an arm is down its length, and quick: a step puts a foot 30 cm on in a
                // fifth of a second.)
                case PartId.ThighL:
                case PartId.ThighR: return (LegQuick, LegStrength * strong);
                case PartId.ShinL:
                case PartId.ShinR: return (LegQuick, 0.7f * LegStrength * strong);
                case PartId.FootL:
                case PartId.FootR: return (LegQuick, 0.3f * LegStrength * strong);
                case PartId.UpperArmL: return (ShieldArmQuick * ease, strong * left);
                case PartId.ForearmL: return (ShieldArmQuick * ease, 0.7f * strong * left);
                default: return (ShieldArmQuick * ease, 0.25f * strong * left);
            }
        }

        private void StepUpper(float dt)
        {
            upperDt = dt;
            // (A part cut off since it was built: built again without it.)
            int lost = 0;
            foreach (var id in UpperBodyArticulation.Parts)
            {
                lost += Rig.IsSevered(id) ? 1 : 0;
            }

            if (PhysicalLegs)
            {
                foreach (var id in UpperBodyArticulation.LegParts)
                {
                    lost += Rig.IsSevered(id) ? 1 : 0;
                }
            }

            bool whole = !Rig.IsSevered(PartId.Spine) && !Rig.IsSevered(PartId.Chest);
            if (Upper != null && lost != upperLost)
            {
                ReleaseUpper();
            }

            upperLost = lost;

            // (Only with the game stepping the engine itself: its motors' targets are fed at each physics step.)
            bool want = PhysicalUpper && PhysicsSteps.Manual && !upperOff && whole && !Fallen && Authority.Level(BodyRegion.Torso) != AuthorityLevel.Ragdoll;
            if (want && Upper == null)
            {
                BuildUpper();
            }
            else if (!want && Upper != null)
            {
                ReleaseUpper();
            }

            if (Upper == null)
            {
                return;
            }

            // (A body the engine lost in the last step is built anew on its pose. Put back on it, it was out of the
            // way of the others for that step; but a body thrown out of the world can be left with speeds and places
            // that are no numbers at all, and nothing set on it afterwards mends those. A new one has none.)
            if (upperLostNow)
            {
                upperLostNow = false;
                Upper.Destroy();
                Upper = null;
                BuildUpper();
                if (Upper == null)
                {
                    return;
                }
            }

            // (The weapons' mass in the physics changed on the scene's Combat Feel: the body is built again with it.)
            if (builtWithMass != (CombatFeel.WeaponsHaveMass, CombatFeel.WeaponMass))
            {
                solidsChanged = true;
            }

            // (Or its legs taken into the physical body, or out of it.)
            if (builtWithLegs != PhysicalLegs)
            {
                solidsChanged = true;
            }

            // (Or its hips held where they were locked, or locked where they were held.)
            if (builtHeld != HeldHips)
            {
                solidsChanged = true;
            }

            if (solidsChanged)
            {
                // (What its hands hold has changed: built again with it. A part added to or taken from a body the
                // engine has stepped has the engine build the whole of it anew, where it stood at rest.)
                solidsChanged = false;
                ReleaseUpper();
                BuildUpper();
                if (Upper == null)
                {
                    return;
                }
            }

            // (Physical legs: a foot the pose has planted is pinned where it is planted, and let go as it lifts. A leg a
            // blow has just landed on lets go: its muscles eased and its foot unpinned for a third of a second, so it is
            // knocked aside and comes back, as an arm is.)
            if (Upper.HasLegs)
            {
                for (int f = 0; f < 2; f++)
                {
                    float left = Mathf.Clamp01(legKnocked[f] / LegKnockFor);
                    // (Let go for most of that time, and taken up again quickly at its end.)
                    float soft = left * (2f - left);
                    legKnocked[f] = Mathf.Max(0f, legKnocked[f] - dt);
                    EaseLeg(f, soft);
                    var lk = Poser.Feet[f];
                    Upper.PinFoot(f, lk.Locked && snapped && soft <= 0f, lk.Target, Mathf.Max(0.25f, Mass / MediumMass), Size);
                }
            }

            // (A region held less firmly, struck, lets its muscles go for as long.)
            for (int r = 1; r < 5; r++)
            {
                var region = (BodyRegion)r;
                float ease = Mathf.Lerp(LeastQuick, 1f, Mathf.Clamp01(Authority.Pin(region) / Mathf.Max(1e-3f, BasePin(region))));
                if (Authority.Level(region) == AuthorityLevel.Kinematic || Authority.Level(region) == AuthorityLevel.Muscled)
                {
                    ease = 1f;
                }

                // (An arm held off its pose, its blade stopped on a shield or its shield pressed back by a body: it
                // gives. The strength that swings a sword as the clips do is several times a man's; pressed on
                // against what stopped it, it would fold whoever stood there in two.)
                float give = 1f;
                // (Heavy weapons: through a strike the weapon arm is as much stronger as the scene's Combat Feel has it,
                // and gives to nothing: the weapon is the steady one, what it lands on gives way. A blow stopped by a
                // shield or a parry gives as before.)
                if (Swinging && (region == BodyRegion.ArmR || (region == BodyRegion.ArmL && Poser.TwoHanded)))
                {
                    give = CombatFeel.StrikeArmStrength;
                }
                // (Not the arm a shield is on: to hold against what presses on it is what a shield is for.)
                else if (region == BodyRegion.ArmR || (region == BodyRegion.ArmL && !Upper.Turns(PartId.HandL)))
                {
                    int side = region == BodyRegion.ArmR ? 0 : 1;
                    float off = side == 0 ? strainR : strainL;
                    float held = 1f - Mathf.InverseLerp(HeldUnder * Size, 2.5f * HeldUnder * Size, handSpeed[side]);
                    give = Mathf.Lerp(1f, BlockedStrength, held * Mathf.InverseLerp(BlockedFrom * Size, BlockedTo * Size, off));
                }

                if (Mathf.Abs(ease - upperEase[r]) > 0.02f || Mathf.Abs(give - upperGive[r]) > 0.02f)
                {
                    upperEase[r] = ease;
                    upperGive[r] = give;
                    foreach (var id in Upper.Own)
                    {
                        if (id != PartId.Pelvis && AuthorityController.RegionOf(id) == region)
                        {
                            var m = MuscleOf(id, ease);
                            Upper.SetMuscle(id, m.hz, m.most * give);
                        }
                    }
                }
            }

            HoldOrSlack();
            Upper.BeginStep(dt);
        }

        /// <summary>Its weapon arm is swinging a blow: a strike playing or just over (<see cref="WeaponArmHeldAfter"/>), not stopped by a shield, a parry or a friend.</summary>
        private bool Swinging => clock < weaponArmHeldUntil && Strikes != null && !Strikes.Stopped;

        private static readonly PartId[] Trunk = { PartId.Pelvis, PartId.Spine, PartId.Chest, PartId.Head };
        private static readonly PartId[][] ArmEnds = { new[] { PartId.ForearmL, PartId.HandL }, new[] { PartId.ForearmR, PartId.HandR } };
        private static readonly (PartId part, PartId on)[][] ArmJoints =
        {
            new[] { (PartId.UpperArmL, PartId.Chest), (PartId.ForearmL, PartId.UpperArmL), (PartId.HandL, PartId.ForearmL) },
            new[] { (PartId.UpperArmR, PartId.Chest), (PartId.ForearmR, PartId.UpperArmR), (PartId.HandR, PartId.ForearmR) },
        };

        /// <summary>An arm's forearm, hand or what the hand holds this deep in its own trunk or head (metres) is not left there: the arm is held to its pose.</summary>
        public const float InOwnBody = 0.03f;

        /// <summary>A joint this far further past its range than the pose has it (degrees) is not left there: it is held to its pose.</summary>
        public const float PastRange = 8f;

        /// <summary>Times an arm was held to its pose for being in its own body, and for a joint past its range (steps; for the reports).</summary>
        public int HeldForBody { get; private set; }

        public int HeldForRange { get; private set; }

        /// <summary>
        /// Which joints of the physical upper body have slack and which are held to the pose
        /// (<see cref="UpperBodyArticulation.Ask"/>). The arms have slack: knocked aside they stay aside and come
        /// back over a second or so, doing meanwhile what the animation does. The spine, chest and head are held
        /// (what the feet give way to and the body falls by is how far the chest is off its pose). And an arm is
        /// held while its slack would leave it somewhere it can't be: its forearm, hand, sword or shield inside
        /// its own trunk or head (its own parts pass through each other), or one of its joints bent further past
        /// what a joint does than the animation has it.
        /// </summary>
        private void HoldOrSlack()
        {
            foreach (var id in Trunk)
            {
                Upper.SetTight(id, 1f);
            }

            for (int side = 0; side < 2; side++)
            {
                bool held = false;
                if (Upper.Holds(ArmEnds[side][1]) && Upper.Deepest(ArmEnds[side], Trunk) > InOwnBody * Size)
                {
                    held = true;
                    HeldForBody++;
                }

                foreach (var (id, on) in ArmJoints[side])
                {
                    if (!Upper.Holds(id))
                    {
                        continue;
                    }

                    var part = Rig.Part(id);
                    float past = JointMath.PastLimits(part.JointTurn(Rig.Part(on).transform.rotation, part.transform.rotation), part.limits, out _, out _, out _);
                    bool over = past > PastRange
                        && past > PastRange + JointMath.PastLimits(part.JointTurn(Anim.Part(on).rotation, Anim.Part(id).rotation), part.limits, out _, out _, out _);
                    if (over)
                    {
                        HeldForRange++;
                    }

                    // (And an arm swinging its weapon has no slack: it is held to its swing.)
                    Upper.SetTight(id, held || over || (Swinging && (side == 1 || Poser.TwoHanded)) ? 1f : 0f);
                }

                StopArm(side);
            }
        }

        /// <summary>The physical arms have stops (the scene's Combat Feel, "Arms Have Stops"): see <see cref="StopArm"/>.</summary>
        public static bool ArmStops = true;

        /// <summary>A physical joint this far past what a body's does (degrees) is at its stop, and from <see cref="StopFull"/> past it is held with all a stop has.</summary>
        public const float StopFrom = 2f, StopFull = 10f;

        private readonly ArmGate stopGate = new ArmGate();

        /// <summary>For the reports: steps a physical arm was at a stop, and the furthest past its range a stop found one (degrees): left, right.</summary>
        public readonly int[] ArmStopped = new int[2];

        public readonly float[] ArmStoppedMost = new float[2];

        /// <summary>
        /// The physical arm's stops (the plan "Arms that bend as a body does", step 5). The arm as it is in the
        /// physics (knocked by a blow, pressed by a shield, left behind by its muscles) is read as the pose's arm is
        /// read by the gate, against the same ranges: where the elbow is, which way it folds, how far it is bent, how
        /// the forearm is rolled and the wrist bent. A joint past its range is told where its range ends nearest where
        /// it is (<see cref="UpperBodyArticulation.StopAt"/>): it does not give way further, and is held there with
        /// more than its muscle has. Inside its range nothing is changed: the arm gives and comes back as it did.
        /// </summary>
        /// <param name="side">0: the left arm; 1: the right.</param>
        private void StopArm(int side)
        {
            bool left = side == 0;
            var joints = ArmJoints[side];
            if (!ArmStops || !ArmLimits.On || !snapped || !Upper.Holds(joints[2].part) || (left && Poser.TwoHanded) || Rig.IsSevered(joints[2].part))
            {
                foreach (var (id, _) in joints)
                {
                    Upper.NoStop(id);
                }

                return;
            }

            Transform chest = Rig.Part(PartId.Chest).transform, upper = Rig.Part(joints[0].part).transform, fore = Rig.Part(joints[1].part).transform, hand = Rig.Part(joints[2].part).transform;
            var clip = ArmLimits.ReadClip(Loco.RawPoints, (int)LocoBone.Neck, (int)LocoBone.Spine2, (int)LocoBone.LeftArm, (int)LocoBone.RightArm, left);
            float past = ArmLimits.Read(chest.rotation, upper.position, fore.position, hand.position, hand.rotation, left, clip).Worst;
            stopGate.Forget();
            if (past < StopFrom
                || !stopGate.Hold(chest.rotation, upper.position, fore.position, hand.position, hand.rotation, left, clip, 1f / 60f, out var turnUpper, out var turnFore, out var turnHand))
            {
                foreach (var (id, _) in joints)
                {
                    Upper.NoStop(id);
                }

                return;
            }

            ArmStopped[side]++;
            ArmStoppedMost[side] = Mathf.Max(ArmStoppedMost[side], past);
            // (The arm as the gate would have it, each bone carried by the one it hangs from, and each joint's turn in it.)
            Quaternion u = turnUpper * upper.rotation, f = turnFore * (turnUpper * fore.rotation), h = turnHand * (turnFore * (turnUpper * hand.rotation));
            var then = new[] { Quaternion.Inverse(chest.rotation) * u, Quaternion.Inverse(u) * f, Quaternion.Inverse(f) * h };
            var is_ = new[] { Quaternion.Inverse(chest.rotation) * upper.rotation, Quaternion.Inverse(upper.rotation) * fore.rotation, Quaternion.Inverse(fore.rotation) * hand.rotation };
            for (int j = 0; j < 3; j++)
            {
                float by = Quaternion.Angle(is_[j], then[j]);
                if (by >= StopFrom)
                {
                    Upper.StopAt(joints[j].part, then[j], ArmLimits.Smooth(StopFrom, StopFull, by), MuscleOf(joints[j].part, 1f).most);
                }
                else
                {
                    Upper.NoStop(joints[j].part);
                }
            }
        }

        private void BuildUpper()
        {
            ArticulationConventions.EnsureProbed();
            if (!ArticulationConventions.Usable)
            {
                upperOff = true;
                return;
            }

            UpperBodyArticulation.WithLegs = PhysicalLegs;
            builtWithLegs = PhysicalLegs;
            UpperBodyArticulation.HipsSupported = HeldHips;
            builtHeld = HeldHips;
            legEase[0] = legEase[1] = 1f;
            legLoose[0] = legLoose[1] = false;
            legKnocked[0] = legKnocked[1] = 0f;
            Upper = new UpperBodyArticulation(Rig, Anim, Rig.Part(PartId.Pelvis).transform.parent, id => MuscleOf(id, 1f)) { GripStrength = Strong };
            if (Upper.HasLegs)
            {
                // (Held to the pose, as the trunk is: no slack. Where a foot stands is the pose's to say.)
                foreach (var id in UpperBodyArticulation.LegParts)
                {
                    if (Upper.Holds(id))
                    {
                        Upper.SetTight(id, 1f);
                    }
                }
            }

            foreach (var id in Upper.Own)
            {
                var part = Rig.Part(id);
                Authority.Skipped[(int)id] = true;
                part.body.isKinematic = true;
                // (Its shadow: found by what looks for a body's parts, in the way of nothing.)
                if (part.partCollider != null)
                {
                    part.partCollider.isTrigger = true;
                }
            }

            for (int r = 0; r < 5; r++)
            {
                upperEase[r] = 1f;
                upperGive[r] = 1f;
            }

            strainR = strainL = 0f;
            handKnown[0] = handKnown[1] = false;
            handSpeed[0] = handSpeed[1] = 0f;
            awayFor = 0f;
            spinFor = 0f;
            pressed = Vector3.zero;
            pressedOff = 0f;
            jointJumpsSeen = 0;
            HoldSolids();
            Upper.Settle();
            if (UpperBodyArticulation.Debugging)
            {
                var sb = new System.Text.StringBuilder($"UPPERBUILD {Rig.name} size {Size:0.00} mass {Mass:0}:");
                foreach (var id in Upper.Own)
                {
                    if (id == PartId.Pelvis) continue;
                    var h = Upper.Hangs(id);
                    sb.Append($" {id} I {Mathf.Max(h.x, Mathf.Max(h.y, h.z)):0.00} most {MuscleOf(id, 1f).most:0} m {Upper.Link(id).mass:0.0};");
                }

                foreach (var (hand, solid, mass, blade) in Solids) sb.Append($" holds {solid.name} {mass:0.0} kg on {hand};");
                Debug.Log(sb.ToString());
            }
            solidsChanged = false;
        }

        private void HoldSolids()
        {
            foreach (var (hand, solid, mass, blade) in Solids)
            {
                if (solid == null)
                {
                    continue;
                }

                if (blade)
                {
                    // (A sword balances a hand's breadth past its guard and turns about that like a rod a third its length.)
                    var slot = hand == PartId.HandR ? Rig.SlotWeaponR : Rig.SlotWeaponL;
                    Vector3 balance = Rig.Part(hand).transform.InverseTransformPoint(slot.position + slot.forward * (0.18f * Size));
                    Upper.Hold(hand, solid, WeighsInBody(mass, true), balance, 0.26f * Size);
                }
                else if (!CombatFeel.ShieldFixed && hand == PartId.HandL && Rig.SlotWeaponL != null && Anim.SlotWeaponL != null)
                {
                    // (A shield that is turned in its hand to face a blow: a part of its own, turned by the grip. Fixed in the
                    // fist (the scene's Combat Feel, as the body is built), it is one piece with the hand, as a blade is: no
                    // blow turns it in the grip.)
                    var posedHand = Anim.Part(PartId.HandL);
                    Quaternion rest = Quaternion.Inverse(posedHand.rotation) * (Anim.SlotWeaponL.parent != null ? Anim.SlotWeaponL.parent.rotation : posedHand.rotation)
                        * (Poser.ShieldGripRest ?? Anim.SlotWeaponL.localRotation);
                    Upper.HoldTurned(hand, solid, WeighsInBody(mass, false), Anim.SlotWeaponL, Rig.SlotWeaponL, rest);
                }
                else
                {
                    Upper.Hold(hand, solid, WeighsInBody(mass, false));
                }
            }

            builtWithMass = (CombatFeel.WeaponsHaveMass, CombatFeel.WeaponMass);
        }

        private (bool, float) builtWithMass;

        /// <summary>
        /// What a held thing weighs in the physical body (kg). Real weapon weight (the scene's Combat Feel, "Weapons
        /// Have Their Mass"): its own weight at its bearer's size (weight goes with volume: a Large's club is a bigger
        /// club), and a weapon <see cref="CombatFeel.WeaponMass"/> times that. Off: its weight as written, whatever the
        /// bearer's size (as before).
        /// </summary>
        private float WeighsInBody(float mass, bool weapon) =>
            CombatFeel.WeaponsHaveMass ? mass * Size * Size * Size * (weapon ? Mathf.Max(0.1f, CombatFeel.WeaponMass) : 1f) : mass;

        /// <summary>The physical upper body let go (down, dead, a limb off): the rig's own parts take over where it was, at its speeds.</summary>
        private void ReleaseUpper()
        {
            foreach (var id in Upper.Own)
            {
                var part = Rig.Part(id);
                var link = Upper.Link(id);
                Authority.Skipped[(int)id] = false;
                if (part.partCollider != null)
                {
                    part.partCollider.isTrigger = false;
                }

                part.transform.SetPositionAndRotation(link.transform.position, link.transform.rotation);
                part.body.isKinematic = false;
                part.body.linearVelocity = link.linearVelocity;
                part.body.angularVelocity = link.angularVelocity;
            }

            Upper.Destroy();
            Upper = null;
        }

        private void BeforeSubstep(int k, int n, float sub)
        {
            Authority?.Substep(k, n);
            Upper?.Substep(k, n, upperDt);
        }

        private void AfterStep()
        {
            if (Upper == null)
            {
                return;
            }

            if (fallBody && Footing == Footings.Down)
            {
                // (Falling in its one body: nothing of a standing body's is asked of it. Thrown out of the world by the
                // engine, it is given up for the ragdoll it would have been.)
                if (Upper.Gone(Loco.Position, 40f))
                {
                    LostBody++;
                    fallBody = false;
                    foreach (var id in Upper.Own)
                    {
                        var part = Rig.Part(id);
                        var posed = Anim.Part(id);
                        Authority.Skipped[(int)id] = false;
                        if (part.partCollider != null)
                        {
                            part.partCollider.isTrigger = false;
                        }

                        part.transform.SetPositionAndRotation(new Vector3(Loco.Position.x, posed.position.y, Loco.Position.z), posed.rotation);
                        part.body.isKinematic = false;
                        part.body.linearVelocity = Vector3.zero;
                        part.body.angularVelocity = Vector3.zero;
                    }

                    Upper.Destroy();
                    Upper = null;
                    for (int r = 0; r < 5; r++)
                    {
                        Authority.Set((BodyRegion)r, AuthorityLevel.Ragdoll, 0f);
                    }

                    return;
                }

                Upper.MoveShadows();
                return;
            }

            if (Upper.Supported)
            {
                float off = Upper.HipsOff;
                if (!float.IsNaN(off))
                {
                    HipsOffMost = Mathf.Max(HipsOffMost, off);
                    HipsOffSum += off;
                    HipsOffSteps++;
                }
            }

            // (A body the engine has lost, in the step it loses it: its pelvis torn off what carries it, a part thrown
            // metres from its pose. Seen in Unity on 13 November: a spinning jump's leg, which goes where its pose puts
            // it, hooked a standing fighter's sword arm and drew it out to its full length; two steps on that body was
            // thrown out of the world, the five others with it in the same step, and all six were then taken for borne
            // down and got up where they had been thrown. It is put back on its pose at once, before anything reads
            // where it is: not its shadows, not what its arms give by, not what its feet give way to.)
            if (UpperBodyArticulation.PutsBackAtOnce && Upper.OutOfHand(Size, out string lost))
            {
                LostBody++;
                upperLostNow = true;
                Upper.Teleport();
                foreach (var id in Upper.Own)
                {
                    var posed = Anim.Part(id);
                    Rig.Part(id).transform.SetPositionAndRotation(posed.position, posed.rotation);
                }

                strainR = strainL = 0f;
                handKnown[0] = handKnown[1] = false;
                awayFor = spinFor = 0f;
                pressed = Vector3.zero;
                pressedOff = 0f;
                if (LostBody <= 40)
                {
                    Debug.Log($"[Upper] {Rig.name}: out of hand ({lost}); {Loco.Current} {(Loco.Shot != null ? Loco.Shot.Name : "")}; {LastReaction}: put back on its pose at once ({LostBody})");
                }

                return;
            }

            Upper.MoveShadows();
            if (Upper.JointJumps != jointJumpsSeen)
            {
                SnappedBack += Mathf.Max(0, Upper.JointJumps - jointJumpsSeen);
                jointJumpsSeen = Upper.JointJumps;
                if (UpperBodyArticulation.Debugging)
                {
                    Debug.Log($"JOINTJUMP {Time.time:0.000} {Rig.name}: {Upper.LastJointJump} ({Loco.Current} {(Loco.Shot != null ? Loco.Shot.Name : "")}; {LastReaction})");
                }
            }
            // (How far each hand ended from where the pose had it this step: what its arm gives by in the next.)
            strainR = Upper.Holds(PartId.HandR) ? Upper.Off(PartId.HandR) : 0f;
            strainL = Upper.Holds(PartId.HandL) ? Upper.Off(PartId.HandL) : 0f;
            // (And how fast each hand moves against its own hips: a hand off its pose and still is held by
            // something; one off its pose and moving is on its way, behind or coming back, and is not to give.)
            var hips = Upper.Link(PartId.Pelvis).transform;
            for (int side = 0; side < 2; side++)
            {
                PartId hand = side == 0 ? PartId.HandR : PartId.HandL;
                if (!Upper.Holds(hand))
                {
                    handKnown[side] = false;
                    continue;
                }

                Vector3 at = hips.InverseTransformPoint(Upper.Link(hand).transform.position);
                handSpeed[side] = handKnown[side] && upperDt > 1e-6f ? (at - handAt[side]).magnitude / upperDt : 0f;
                handAt[side] = at;
                handKnown[side] = true;
            }
            // (A body that has left its pose and stays away: a hand a metre from where the pose has it, or the head
            // half a metre, for half a second. Whatever did it (a limb caught round another's, something the engine
            // did that the headless one does not), it is put back on its pose rather than left to flail.)
            bool away = strainR > RanAwayHand * Size || strainL > RanAwayHand * Size
                || (Upper.Holds(PartId.Head) && Upper.Off(PartId.Head) > RanAwayHead * Size);
            // (Counted down when it is back, not dropped: a body thrown about passes its pose again and again.)
            awayFor = away ? awayFor + upperDt : Mathf.Max(0f, awayFor - 0.5f * upperDt);
            // (Or one of its joints turning faster than any clip turns one, and staying so: it is out of hand.)
            float fastestJoint = Upper.FastestJoint(out var fastestPart);
            spinFor = fastestJoint > TraceJointSpeed ? spinFor + upperDt : Mathf.Max(0f, spinFor - upperDt);
            if (awayFor >= RanAwayAfter || spinFor >= OutOfHandAfter)
            {
                awayFor = 0f;
                spinFor = 0f;
                RanAway++;
                Upper.Teleport();
                strainR = strainL = 0f;
                handKnown[0] = handKnown[1] = false;
            }

            // (For the playtest log: a body out of hand, as it stands, a few times a second at most. What the
            // engine does in Unity that the headless one does not is only to be seen there.)
            if (TraceUpper && upperTraced < 80 && clock >= upperTraceNext)
            {
                float fastest = fastestJoint;
                var which = fastestPart;
                float head = Upper.Holds(PartId.Head) ? Upper.Off(PartId.Head) : 0f;
                if (fastest > TraceJointSpeed || head > 0.25f * Size || strainR > 0.6f * Size || strainL > 0.6f * Size)
                {
                    upperTraced++;
                    upperTraceNext = clock + 0.2f;
                    Debug.Log($"[Upper] {Rig.name}: {which} turning at {fastest:0} rad/s; head {head * 100f:0} cm, hands {strainR * 100f:0} / {strainL * 100f:0} cm off the pose; "
                        + $"{Loco.Current} {(Loco.Shot != null ? Loco.Shot.Name : "")}; {LastReaction}; put back {RanAway}, joints put {SnappedBack}\n  {Upper.DescribeAll()}");
                }
            }

            // (And its chest: what its feet give way to.)
            pressed = Upper.Link(PartId.Chest).transform.position - Anim.Part(PartId.Chest).position;
            pressedOff = pressed.magnitude;
            pressed.y = 0f;
        }

        /// <summary>
        /// An arm held off its pose gives: the share of its strength it presses on with (<see cref="BlockedStrength"/>)
        /// once its hand is <see cref="BlockedTo"/> from where the pose has it, all of it up to <see cref="BlockedFrom"/>
        /// (metres at a Medium's size; an arm nothing stops follows its pose within 0.10 m).
        /// </summary>
        public static float BlockedStrength = 0.2f, BlockedFrom = 0.10f, BlockedTo = 0.22f;

        /// <summary>A hand this far from its pose, or the head this far (metres at a Medium's size), for this long (seconds): the body is put back on its pose.</summary>
        public const float RanAwayHand = 0.9f, RanAwayHead = 0.5f, RanAwayAfter = 0.5f;

        /// <summary>A physical upper body out of hand is written to the log as it stands (a joint turning faster than <see cref="TraceJointSpeed"/>, its head or a hand far off its pose).</summary>
        public static bool TraceUpper = true;

        /// <summary>A joint turning faster than this is out of hand (rad/s; the fastest clip turns one at 44).</summary>
        public const float TraceJointSpeed = 60f;

        private int upperTraced;
        private float upperTraceNext;

        /// <summary>A joint turning faster than <see cref="TraceJointSpeed"/> for this long in all (seconds): the body is put back on its pose.</summary>
        public const float OutOfHandAfter = 0.2f;

        private float spinFor;

        /// <summary>Times its physical upper body left its pose and stayed away, and was put back on it (for the reports).</summary>
        public int RanAway;

        /// <summary>Times the engine lost its physical upper body (<see cref="UpperBodyArticulation.OutOfHand"/>) and it was put back on its pose in the same step (for the reports).</summary>
        public int LostBody;

        private bool upperLostNow;

        private float awayFor;

        /// <summary>A hand slower than this against its own hips is held (m/s at a Medium's size); at 2.5 times it, it is on its way.</summary>
        public const float HeldUnder = 1f;

        private readonly Vector3[] handAt = new Vector3[2];
        private readonly float[] handSpeed = new float[2];
        private readonly bool[] handKnown = new bool[2];
        private float strainR, strainL;
        private readonly float[] upperGive = { 1f, 1f, 1f, 1f, 1f };

        public float StrikeArmStrength = 1f;

        /// <summary>
        /// The torso and head held another way than by their pins (null: by their pins, as they have been): kinematic, or
        /// on their joints' motors from the carried pelvis (the experiment's whole physical upper body).
        /// </summary>
        public AuthorityLevel? TorsoHold;

        public float TorsoStrength = 1f;

        /// <summary>The weapon arm is held to its pose this long after a strike (seconds).</summary>
        public const float WeaponArmHeldAfter = 0.4f;

        private float weaponArmHeldUntil = -1f;

        /// <summary>It strikes with <paramref name="weapon"/> (null: a plain sword of its poser's weapon length): its blade's hits, swept from the physical hand's slot when it is physical.</summary>
        /// <summary>Its weapon gone (its weapon arm cut off): no more strikes.</summary>
        public void DisableStrikes()
        {
            Combo?.Stop();
            Strikes = null;
        }

        /// <summary>
        /// Down for good though alive (a leg cut off): the body to physics as a fall, and it never gets up. Its wounds
        /// bleed it out.
        /// </summary>
        public bool StaysDown { get; private set; }

        /// <summary>
        /// Knocked off its feet by what is no blade (a body thrown into it, its own body borne down): down as from
        /// a leg swept from under it, and it gets up again.
        /// </summary>
        public void KnockOver(Vector3 way, float knock, string why, float thrown = 0f)
        {
            if (Dead || StaysDown || Fallen)
            {
                return;
            }

            fallThrown = thrown;
            catching = FallsCanBeCaught && NaturalGetUps && thrown > 0f && thrown <= CaughtUpTo * CombatFeel.LegsCatch * Mathf.Sqrt(Size);

            Combo?.Stop();
            way.y = 0f;
            LastReaction = why;
            Fall(-1, way.sqrMagnitude > 1e-6f ? way.normalized : -(Quaternion.Euler(0f, Loco.Facing, 0f) * Vector3.forward), Mathf.Max(0.5f, knock));
        }

        public void Collapse(Vector3 way, float knock)
        {
            if (Dead || StaysDown)
            {
                return;
            }

            StaysDown = true;
            Combo?.Stop();
            Blow = null;
            way.y = 0f;
            if (Footing != Footings.Down)
            {
                Fall(-1, way.sqrMagnitude > 1e-6f ? way.normalized : -(Quaternion.Euler(0f, Loco.Facing, 0f) * Vector3.forward), knock);
            }
        }

        public StrikeHits EnableStrikes(Combat.WeaponDefinition weapon)
        {
            Strikes = new StrikeHits(Loco, Poser, Anim) { Weapon = weapon, Scale = Size, Slot = Authority != null ? Rig.SlotWeaponR : null, Mass = Mass, Owner = this };
            StrikeClips = StrikesOf(Poser.BodyKey);
            Strikes.Finishing = () => Combo != null && Combo.LastBlow;
            Strikes.Comboing = () => Combo != null && Combo.Playing && Combo.Cores;
            return Strikes;
        }

        /// <summary>
        /// It meets the blows coming at it and reacts to those that land (off: its pose is the clip's and nothing else,
        /// as the lab's walker was before anything struck back).
        /// </summary>
        public bool Defends = true;

        /// <summary>A strike lands when its blade passes within this of its aim from where the body stands, the arm's pull taken (leg lengths... a Medium's metres).</summary>
        public const float LandsNear = 0.1f;

        /// <summary>The strikes a body knows (its clips with the strike role, as <see cref="LocoLibrary.StrikeFor"/> allows it).</summary>
        public static List<LocoClip> StrikesOf(string body)
        {
            string prefix = LocoLibrary.CombatPrefix(body);
            var names = new List<string>();
            foreach (var pair in LocoLibrary.Clips)
            {
                if (pair.Value.IsGuard && pair.Value.Role == "strike" && pair.Key.StartsWith(prefix) && LocoLibrary.StrikeFor(body, pair.Key))
                {
                    names.Add(pair.Key);
                }
            }

            names.Sort(string.CompareOrdinal);
            var strikes = new List<LocoClip>();
            foreach (var name in names)
            {
                strikes.Add(LocoLibrary.Get(name));
            }

            return strikes;
        }

        /// <summary>Its strikes (<see cref="StrikesOf"/> its body, set by <see cref="EnableStrikes"/>).</summary>
        public List<LocoClip> StrikeClips = new List<LocoClip>();

        /// <summary>A strike is under way: one playing up to its end, or a combo.</summary>
        public bool Striking => (Loco.Current == Locomotor.Mode.OneShot && Loco.Shot != null && Loco.Shot.ContactFrame >= 0) || (Combo != null && Combo.Playing);

        /// <summary>Busy with something it can't break off to strike: a strike, a reaction, down.</summary>
        public bool Busy => Fallen || Striking || Loco.Current == Locomotor.Mode.OneShot || Stumbling;

        /// <summary>Thrown back stumbling and not yet on its feet again (<see cref="CombatFeel.StumbleRecovery"/>): it can't strike.</summary>
        public bool Stumbling => thrownAt >= 0f && clock - thrownAt < CombatFeel.StumbleRecovery;

        private float thrownAt = -1f;

        /// <summary>Where on <paramref name="target"/> a strike goes: its part (hips, chest or face) nearest the height the strike reaches.</summary>
        public Vector3 AimAt(ClipFighter target, LocoClip clip)
        {
            float h = target.TargetAt(1).y;
            if (clip != null && StrikeReach.TryGet(Poser.BodyKey, clip.Name, out var grip, out var way))
            {
                h = (grip + way * (Poser.WeaponLength * Poser.SweetSpot)).y + Poser.GroundY;
            }

            int best = 0;
            for (int k = 1; k < 3; k++)
            {
                if (Mathf.Abs(target.TargetAt(k).y - h) < Mathf.Abs(target.TargetAt(best).y - h))
                {
                    best = k;
                }
            }

            return target.TargetAt(best);
        }

        /// <summary>Its lunges and its hand kept off <paramref name="target"/>'s body: no nearer it than the fighting distance.</summary>
        public void KeepOff(ClipFighter target)
        {
            Poser.KeepFrom = target != null ? target.Loco.Position : (Vector3?)null;
            Poser.KeepDistance = target != null ? FightingDistance(Size, target.Size) : 0f;
        }

        private readonly System.Random strikeLuck = new System.Random(20261105);

        /// <summary>
        /// A strike of its own that lands on <paramref name="target"/> from where it stands (its blade within
        /// <see cref="LandsNear"/> of the aim, the turn, lunge and arm's pull taken), one of them by chance; null when none
        /// reaches.
        /// </summary>
        public LocoClip StrikeThatLands(ClipFighter target)
        {
            if (target == null || StrikeClips.Count == 0)
            {
                return null;
            }

            KeepOff(target);
            var landing = new List<LocoClip>();
            foreach (var clip in StrikeClips)
            {
                if (Poser.MissAfterPull(clip, AimAt(target, clip)) <= LandsNear * Size)
                {
                    landing.Add(clip);
                }
            }

            return landing.Count > 0 ? landing[strikeLuck.Next(landing.Count)] : null;
        }

        /// <summary>Plays <paramref name="clip"/> at <paramref name="target"/> (aimed at its part nearest the strike's height); a strike alone.</summary>
        public Vector3 StrikeAt(ClipFighter target, LocoClip clip)
        {
            KeepOff(target);
            Vector3 at = AimAt(target, clip);
            Poser.PlayStrike(clip, at);
            return at;
        }

        /// <summary>
        /// Plays the combo <paramref name="first"/> is in at <paramref name="target"/> (Bam! Bam! BAM!: <paramref name="variant"/>
        /// picks among its combos as good as its best), each blow aimed at its part nearest the strike's height. Its
        /// combo player is made the first time. False when it has no follow-on: then nothing plays.
        /// </summary>
        public bool ComboAt(ClipFighter target, LocoClip first, int variant, out LocoClip opener, out List<StrikeLink> chain, int blows = 3)
        {
            opener = first;
            blows = Mathf.Clamp(blows, 2, 3);
            chain = first != null && first.ContactFrame >= 0 ? StrikeCombos.CoreChain(first, StrikeClips, blows, variant) : new List<StrikeLink>();
            if (target == null || chain.Count == 0)
            {
                return false;
            }

            if (Combo == null)
            {
                Combo = new ComboPlayer(Loco, Poser) { Strikes = StrikeClips };
            }

            comboTarget = target;
            heldClip = null;
            heldTarget = null;
            Combo.TargetFor = clip => AimAt(clip == heldClip && heldTarget != null ? heldTarget : comboTarget, clip);
            Combo.TargetVelocity = () => comboTarget.Loco.Velocity;
            Combo.PastContact = TurnCombo;
            KeepOff(target);
            Combo.Keep = first;
            Combo.Most = chain.Count + 1;
            opener = chain[0].From;
            Combo.Start(opener, chain);
            return true;
        }

        private ClipFighter comboTarget, heldTarget;
        private LocoClip heldClip;

        /// <summary>
        /// Who a combo's next blow is at, given who the last was at (the plan "Fighting off several attackers", step 8: a
        /// combo strung across the enemies on it); null or the same: the same one.
        /// </summary>
        public System.Func<ClipFighter, int, ClipFighter> NextComboTarget;

        /// <summary>A combo's next blow was turned on another (the new target).</summary>
        public event System.Action<ClipFighter> ComboTurned;

        private void TurnCombo(int played)
        {
            var next = NextComboTarget?.Invoke(comboTarget, played);
            if (next == null || next == comboTarget || next.Dead)
            {
                // (Its target down and none other to turn on: the combo ends with this blow. Its next blows fell on the
                // body, the playtest of 6 November.)
                if (comboTarget == null || comboTarget.Dead)
                {
                    Combo.EndAfterThis();
                }

                return;
            }

            // (The blow playing, past its contact, stays on whom it was at: only the next is turned.)
            heldClip = Loco.Shot;
            heldTarget = comboTarget;
            comboTarget = next;
            KeepOff(next);
            ComboTurned?.Invoke(next);
        }

        private bool parryWaited;

        // ---- Giving ground (the weight plan's step 1: a parry that can't get there in time steps back to meet it) ----

        /// <summary>
        /// How far it steps back to a blow its parry can't reach in time (metres at a Medium's size: a novice's, an
        /// expert's), and how fast the step is snapped in (m/s), as the combo's magnet snaps the attacker in. A combo's
        /// next blow comes into view only 0.15 to 0.3 s before its contact, often on the other side: no arm gets a sword
        /// from a high parry to a low one in that.
        /// </summary>
        public const float NoviceRetire = 0.25f, ExpertRetire = 0.45f, RetireSpeed = 4f;

        /// <summary>
        /// It gives ground only when its sword arm would be this late to the blow (seconds): a last resort. Given at the
        /// least lateness, every fresh parry stepped away from where its arm was putting the sword, and the arm, overreached,
        /// let more blows through (the Medium's combos: 22 parried and 11 through against 26 and 9).
        /// </summary>
        public const float RetireLate = 0.05f;

        /// <summary>
        /// Ground is given over the time there is before the blow (the scene's Combat Feel), no slower than
        /// <see cref="RetireLeast"/> (m/s at a Medium's size) and there <see cref="RetireThereBefore"/> seconds before
        /// the blow is. Off: snapped in at <see cref="RetireSpeed"/>, as before.
        /// </summary>
        public static bool RetiresInTime = true;

        public const float RetireLeast = 1f, RetireThereBefore = 0.04f;

        /// <summary>For the reports, all units: the time spent giving ground (seconds), and the fastest it was given (m/s at a Medium's size).</summary>
        public static float RetireSeconds, RetireFastest;

        /// <summary>How far it has stepped back from where the blow coming was aimed (world): the blow is still aimed there (<see cref="TargetAt"/>), a step back not being a body driven off by a blow that the attacker follows.</summary>
        public Vector3 Voided { get; private set; }

        private Vector3 retireLeft;
        private int retiredFor = -1, voidedFor = -1;

        /// <summary>Fighters step back to a blow they can't meet in time (the lab's and the runners' switch).</summary>
        public static bool GivesGround = true;

        /// <summary>Times it stepped back to meet a blow (for the logs).</summary>
        public int Retires;

        private void MaybeRetire(Vector3 opponent)
        {
            if (!GivesGround || !Blow.HasValue || Anchored || retiredFor == Blow.Value.Serial || (Parry == null && !Blocking))
            {
                return;
            }

            var b = Blow.Value;
            bool sword = Parry != null;
            // (The weight plan's step 5: it steps back, too, to a blow it reads coming heavy, to meet it earlier in its
            // arc, further out, before it is at full speed on the body; and, with a sword, to one coming in too deep to
            // parry on the strong of the blade, the attacker's hand inside the reach of its forte.)
            // (A parry steps back only from a blow that would beat it, heavier than its skill holds: stepping back, the body
            // leaves where its arm was putting the sword, and an expert's parried forehands from the Large landed.)
            float heavyFrom = sword ? Mathf.Max(CombatFeel.GiveGroundFrom, ParryHolds) : CombatFeel.GiveGroundFrom;
            float heavy = b.Momentum / Mathf.Max(1f, Mass) / Mathf.Max(0.05f, heavyFrom);
            float deep = 0f;
            if (sword && CombatFeel.GiveGroundDeep > 0f && b.GripPath != null && b.GripPath.Length > StrikeHits.ContactPoint)
            {
                Vector3 hand = b.GripPath[StrikeHits.ContactPoint] - Loco.Position;
                hand.y = 0f;
                deep = Mathf.Max(0f, CombatFeel.GiveGroundDeep * Size - hand.magnitude);
            }

            bool late = sword && Parry.Late > RetireLate;
            LastGroundWeight = heavy * heavyFrom;
            if (!late && heavy < 1f && deep <= 0f)
            {
                return;
            }

            // (In a clip of its own its feet can't step: the body would slide over them (the playtest of 6 November: a planted
            // foot dragged 30 cm, a late arm's step taken in a reaction clip). In its own strike it doesn't step back; in a
            // reaction a late arm's last resort still does, the reaction given up for the walk's steps.)
            if (Loco.Current == Locomotor.Mode.OneShot && (!late || Striking))
            {
                return;
            }

            float skill = sword ? ParrySkillNow : ShieldSkillNow;
            float lack = 1f - skill * skill;
            // (A novice reads it later: it steps nearer contact, if at all.)
            if (!late && b.ToContact > CombatFeel.GiveGroundLead * (1f - 0.6f * lack))
            {
                return;
            }

            // (An expert always gives the ground it needs; a novice only now and then, and less of it.)
            retiredFor = b.Serial;
            if ((late ? strikeLuck : groundLuck).NextDouble() < lack)
            {
                return;
            }

            float least = CombatFeel.LeastGiveGround, most = Mathf.Max(least, CombatFeel.MostGiveGround);
            float expertStep = late ? ExpertRetire
                : Mathf.Max(heavy >= 1f ? Mathf.Lerp(least, most, Mathf.Clamp01(heavy - 1f)) : 0f, deep > 0f ? Mathf.Clamp(least + deep / Size, least, most) : 0f);
            float step = Mathf.Lerp(expertStep, expertStep * (NoviceRetire / ExpertRetire), lack);
            Vector3 away = Loco.Position - opponent;
            away.y = 0f;
            away = away.sqrMagnitude > 1e-4f ? away.normalized : -(Quaternion.Euler(0f, Loco.Facing, 0f) * Vector3.forward);
            retireLeft = away * (step * Size);
            voidedFor = b.Serial;
            // (A step back to a heavy blow is followed: the blow is meant to meet the shield further out, not miss it (the
            // playtest of 6 November: the Large's blows fell short of a shield 0.5 m back). A late arm's step is a void.)
            retireVoids = late;
            groundEase = late ? 0f : CombatFeel.GiveGroundEases * Mathf.Clamp01(step / Mathf.Max(0.05f, most));
            Retires++;
            if (!late)
            {
                GroundSteps++;
            }

            GroundTrace?.Invoke(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}gave ground {1:0.00} m to blow {2}, {3:0.00} s before contact ({4}{5}{6}), the weight eased {7:0%}",
                Traced, step * Size, b.Serial, b.ToContact, late ? "its arm late" : "", heavy >= 1f ? string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}heavy: {1:0.00} m/s", late ? ", " : "", heavy * heavyFrom) : "",
                deep > 0f ? string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}deep: the hand {1:0.00} m in", late || heavy >= 1f ? ", " : "", deep) : "", groundEase));
            // (The walk steps after it, as a body driven back does; out of a reaction clip too.)
            Loco.Push(away * (0.6f * Mathf.Sqrt(Size)));
            HopBack(b.ToContact);
        }

        /// <summary>A step back is a hop (the scene's Combat Feel): both feet off the ground together and down together. Off: each is let go as the body leaves it and brought after, as before.</summary>
        public static bool StepBackHops;

        /// <summary>A step back shorter than this is stepped, not hopped (metres at a Medium's size).</summary>
        public const float HopFrom = 0.12f;

        /// <summary>
        /// The ground it has just been given to give (<see cref="retireLeft"/>) is hopped: its feet are in the air for
        /// as long as the body takes to get there, and land as it arrives.
        /// </summary>
        private void HopBack(float toContact)
        {
            float far = retireLeft.magnitude;
            if (!StepBackHops || far < HopFrom * Size)
            {
                return;
            }

            float most = RetireSpeed * Mathf.Sqrt(Size);
            float speed = RetiresInTime ? Mathf.Clamp(far / Mathf.Max(0.01f, toContact - RetireThereBefore), Mathf.Min(most, RetireLeast * Mathf.Sqrt(Size)), most) : most;
            // (A foot's place is fixed when it is four fifths of the way through its arc: the body is to be there by then.)
            Poser.Hop(far / speed / 0.8f);
        }

        /// <summary>
        /// For the debug panel: it steps back <paramref name="metres"/> (at a Medium's size) from <paramref name="from"/>
        /// now, as it does to a blow read <paramref name="before"/> seconds before it lands. False: it cannot (down, in a
        /// clip of its own, held where it is).
        /// </summary>
        public bool StepBackNow(Vector3 from, float metres, float before)
        {
            if (Fallen || Anchored || Loco.Current == Locomotor.Mode.OneShot)
            {
                return false;
            }

            Vector3 away = Loco.Position - from;
            away.y = 0f;
            away = away.sqrMagnitude > 1e-4f ? away.normalized : -(Quaternion.Euler(0f, Loco.Facing, 0f) * Vector3.forward);
            retireLeft = away * (metres * Size);
            retireVoids = false;
            voidedFor = -1;
            testRetireFor = before;
            Retires++;
            Loco.Push(away * (0.6f * Mathf.Sqrt(Size)));
            HopBack(before);
            return true;
        }

        private float testRetireFor = -1f;

        /// <summary>
        /// For the debug panel: its right shin is knocked sideways as a blow would knock it (<paramref name="speed"/>
        /// m/s at a Medium's size, to its left). False: its legs are not of the physical body (or it is down).
        /// </summary>
        public bool KnockLegNow(float speed)
        {
            if (Fallen || Upper == null || !Upper.HasLegs || !Upper.Holds(PartId.ShinR))
            {
                return false;
            }

            Vector3 left = Quaternion.Euler(0f, Loco.Facing, 0f) * Vector3.left;
            KnockLeg(PartId.ShinR, left * (speed * Mathf.Sqrt(Size)));
            return true;
        }

        /// <summary>For the debug panel: standing, it turns by <paramref name="degrees"/> now (+ right) with its turn's clip. False: it cannot (down, in a clip, held where it is).</summary>
        public bool TurnNow(float degrees) => !Fallen && !Anchored && Loco.TurnNow(degrees);

        /// <summary>The share of blow <paramref name="serial"/>'s weight its step back takes off as it is met (0: it didn't step back to it).</summary>
        public float GroundEase(int serial) => serial == voidedFor ? groundEase : 0f;

        /// <summary>Times it stepped back to a heavy or deep blow (not a late arm's), and the weight it last read coming (m/s; for the logs).</summary>
        public int GroundSteps;

        /// <summary>See <see cref="GroundSteps"/>.</summary>
        public float LastGroundWeight;

        /// <summary>Each step back given, said (the lab's log, the runners).</summary>
        public static System.Action<string> GroundTrace;

        private float groundEase;
        private bool retireVoids;
        private readonly System.Random groundLuck = new System.Random(20261106);

        /// <summary>
        /// A body pressed on gives way with its feet: its chest held off where the pose has it, by another body it
        /// stands against or a blade pressed on it, its root is carried the way it is pressed, the further off the
        /// faster. Its pelvis is carried by the pose and gives to nothing: without this all of it is taken up by the
        /// spine, a body pressed 0.3 m folded back over its own hips. From <see cref="PressedFrom"/> off (metres at
        /// a Medium's size), <see cref="PressedGain"/> m/s for each metre further, <see cref="PressedSpeed"/> at most.
        /// </summary>
        public static float PressedFrom = 0.04f, PressedGain = 6f, PressedSpeed = 2f;

        /// <summary>
        /// A body borne down falls: its chest held this far from where the pose has it (metres at a Medium's size; 0:
        /// never) for <see cref="BorneDownAfter"/> seconds, by what no muscle of its own will shift (another landing
        /// on it from a leap, a body driven through where it stands), it goes down as one knocked off its feet does
        /// and gets up again. Standing, it would be folded where it stood.
        /// </summary>
        public static float BorneDownFrom = 0.28f;

        public const float BorneDownAfter = 0.1f;

        /// <summary>Times a joint of its physical upper body was put where the pose turned it at once (for the reports).</summary>
        public int SnappedBack;

        private int jointJumpsSeen;

        /// <summary>Times it was borne down (for the reports).</summary>
        public int BorneDown;

        private Vector3 pressed;
        private float pressedOff, bornDownFor;

        private void PressedBack(float dt)
        {
            if (Upper == null)
            {
                bornDownFor = 0f;
                return;
            }

            bornDownFor = BorneDownFrom > 0f && pressedOff > BorneDownFrom * Size ? bornDownFor + dt : 0f;
            if (bornDownFor >= BorneDownAfter && !Dead && !StaysDown)
            {
                bornDownFor = 0f;
                BorneDown++;
                KnockOver(pressed, 0.5f, $"borne down, its body pressed {pressedOff * 100f:0} cm from where it stood");
                return;
            }

            if (PressedGain <= 0f)
            {
                return;
            }

            float off = pressed.magnitude, from = PressedFrom * Size;
            if (off <= from)
            {
                return;
            }

            Vector3 move = pressed / off * (Mathf.Min((off - from) * PressedGain, PressedSpeed * Mathf.Sqrt(Size)) * dt);
            RootShoves[3] += move.magnitude;
            Loco.Position += move;
            CarryBy(move);
        }

        /// <summary>It is giving ground to a blow now: its root is being moved back (for the logs).</summary>
        public bool GivingGround => retireLeft.sqrMagnitude > 1e-8f;

        private void GiveGround(float dt)
        {
            if (retireLeft.sqrMagnitude > 1e-8f)
            {
                float most = RetireSpeed * Mathf.Sqrt(Size);
                float speed = most;
                if (RetiresInTime)
                {
                    // (The plan "Grounded legs", step 2: what moved the body at once becomes something its feet can
                    // step after. Snapped in at 4 m/s the step back was over in three to seven physics steps, both
                    // feet let go at once and brought after it: a hop. It only has to be there when the blow is, so
                    // it takes the time there is until then (less a moment: there before the blow, not with it), no
                    // slower than a step in the stance goes and no faster than it was snapped in.)
                    float toContact = Blow.HasValue && Blow.Value.Serial == voidedFor ? Blow.Value.ToContact : 0f;
                    if (testRetireFor >= 0f)
                    {
                        // (Asked for from the debug panel: as to a blow that far off.)
                        toContact = testRetireFor;
                        testRetireFor = Mathf.Max(0f, testRetireFor - dt);
                    }

                    speed = Mathf.Clamp(retireLeft.magnitude / Mathf.Max(dt, toContact - RetireThereBefore), Mathf.Min(most, RetireLeast * Mathf.Sqrt(Size)), most);
                }

                Vector3 move = Vector3.ClampMagnitude(retireLeft, speed * dt);
                RetireSeconds += dt;
                RetireFastest = Mathf.Max(RetireFastest, move.magnitude / Mathf.Max(1e-5f, dt) / Mathf.Sqrt(Size));
                RootShoves[1] += move.magnitude;
                Loco.Position += move;
                CarryBy(move);
                retireLeft -= move;
                if (retireVoids)
                {
                    Voided += move;
                }

                if (retireLeft.sqrMagnitude <= 1e-8f)
                {
                    testRetireFor = -1f;
                }
            }
            else if (!Blow.HasValue || Blow.Value.Serial != voidedFor)
            {
                // (The blow it gave ground to is over: the next is aimed at where it stands now.)
                Voided = Vector3.zero;
            }
        }

        /// <summary>How long a pose handed over to the strike's (the parry's arms and trunk) takes to fade out (seconds).</summary>
        public const float HandOffOver = 0.2f;

        private static readonly PartId[] HandOffParts =
            { PartId.Spine, PartId.Chest, PartId.UpperArmL, PartId.ForearmL, PartId.HandL, PartId.UpperArmR, PartId.ForearmR, PartId.HandR };

        private readonly Quaternion[] handOff = new Quaternion[8];
        private float handOffLeft;

        private void CaptureHandOff()
        {
            for (int i = 0; i < HandOffParts.Length; i++)
            {
                handOff[i] = Anim.Part(HandOffParts[i]).localRotation;
            }

            handOffLeft = HandOffOver;
        }

        private void HandOff(float dt)
        {
            if (handOffLeft <= 0f)
            {
                return;
            }

            float u = handOffLeft / HandOffOver;
            float w = u * u * (3f - 2f * u);
            for (int i = 0; i < HandOffParts.Length; i++)
            {
                var part = Anim.Part(HandOffParts[i]);
                part.localRotation = Quaternion.Slerp(part.localRotation, handOff[i], w);
            }

            handOffLeft = Mathf.Max(0f, handOffLeft - dt);
        }

        /// <summary>How long since a blow last landed on it (seconds).</summary>
        public float SinceStruck => clock - lastHit;

        /// <summary>
        /// The shield meeting the blow: blocking (and, by half, in its guard) it watches the striker's weapon from the
        /// moment the blow starts, and after <see cref="SeeBlowAfter"/> moves its shield, at most
        /// <see cref="ShieldSpeed"/>, to where the weapon will come in: on the line from the part aimed at out toward
        /// the weapon, a forearm's length out, its face toward the weapon. With no blow coming, a blocking dummy
        /// holds its shield across the striker's line.
        /// </summary>
        /// <summary>How far before its chest a fist holds a shield at the least, how far over the base of its head at the most, how far out past its chest's middle on its own side and across it on the other (metres at a Medium's size).</summary>
        public const float FistAhead = 0.26f, FistOverNeck = 0.08f, FistOut = 0.6f, FistAcross = 0.2f;

        private Vector3 shieldBoardFrom;
        private bool shieldBoardKnown;

        /// <summary>How far the shield arm's elbow is brought from where the swung guard has it toward the lowest it can be, as the shield meets a blow (a share).</summary>
        public const float ShieldElbowLow = 0.5f;

        /// <summary>For the reports: each blow the shield moved to meet, how far through it was as the shield first moved (a share of its time from start to contact) and how long before its contact (seconds).</summary>
        public readonly List<float> ShieldReadAt = new List<float>(), ShieldReadBefore = new List<float>();

        /// <summary>
        /// The shield held fast in the fist (the plan "Arms that bend as a body does", the shield): the board does not
        /// turn in the hand and the wrist hardly bends (the scene's Combat Feel's wrist turn), so the board looks the way
        /// the back of the hand does, and it is the whole arm that moves it. The arm as the guard's clip holds it (its
        /// elbow down and a little out, its forearm across behind the board, the back of the hand to the enemy) is swung
        /// about the shoulder, as it is, until the fist points at <paramref name="target"/>, and then bent or
        /// straightened in its own plane to reach it: the elbow keeps its way against the arm, and the board goes
        /// round with the arm, looking out from the shoulder: up over a fist raised, down from one lowered, to the
        /// side of one swung wide. The wrist then bends a little toward <paramref name="face"/>.
        /// </summary>
        private void HoldShieldFast(float dt, Vector3 target, float weight, Vector3 face, float lack, Quaternion bodyTurn)
        {
            var upper = Anim.Part(PartId.UpperArmL);
            var fore = Anim.Part(PartId.ForearmL);
            var hand = Anim.Part(PartId.HandL);
            var bodyBack = Quaternion.Inverse(bodyTurn);
            Vector3 poseWay = TwoBoneIK.BendWay(upper, fore, hand);
            Vector3 was = hand.position - upper.position, to = target - upper.position;
            var swing = was.sqrMagnitude > 1e-8f && to.sqrMagnitude > 1e-8f ? Quaternion.FromToRotation(was, to) : Quaternion.identity;
            Vector3 now = swing * poseWay;
            if (now.sqrMagnitude < 1e-6f)
            {
                now = bodyTurn * new Vector3(-0.25f, -1f, -0.2f);
            }

            // (And the elbow brought some of the way toward the lowest it can be with the fist there, as an arm braced to
            // take a weight tucks it: round the line from the shoulder to the fist, which is near the way the board
            // looks, so the board turns about its own middle and looks where it did.)
            if (to.sqrMagnitude > 1e-8f)
            {
                Vector3 lowest = Vector3.ProjectOnPlane(Vector3.down, to.normalized);
                Vector3 carried = Vector3.ProjectOnPlane(now, to.normalized);
                if (lowest.sqrMagnitude > 1e-4f && carried.sqrMagnitude > 1e-6f)
                {
                    now = Vector3.Slerp(carried.normalized, lowest.normalized, ShieldElbowLow * weight);
                }
            }

            TwoBoneIK.Solve(upper, fore, hand, target, now);

            // (The board as the fist holds it; then the wrist bent a little toward where the board should look, no
            // further than a wrist gives under a shield's weight.)
            Poser.FaceShieldAt(Vector3.zero, 0f);
            var slot = Anim.SlotWeaponL;
            if (slot != null)
            {
                if (!shieldFaceKnown || shieldFace.sqrMagnitude < 1e-6f)
                {
                    shieldFace = bodyBack * slot.forward;
                    shieldFaceKnown = true;
                }

                Vector3 faceLocal = face.sqrMagnitude > 1e-6f ? bodyBack * face.normalized : shieldFace;
                Vector3 eased = Vector3.Slerp(shieldFace, faceLocal, 1f - Mathf.Exp(-dt * 3f / Mathf.Max(0.01f, CombatFeel.ShieldFaceSettle)));
                shieldFace = Vector3.RotateTowards(shieldFace, eased, Mathf.Lerp(ShieldTurnSpeed, NoviceShieldTurnSpeed, lack) * Mathf.Deg2Rad * dt, 0f);
                Vector3 should = bodyTurn * shieldFace;
                Vector3 looks = slot.forward;
                Vector3 about = Vector3.Cross(looks, should);
                float off = Vector3.Angle(looks, should);
                if (about.sqrMagnitude > 1e-8f && off > 0.01f)
                {
                    // (Eased in as it nears its most: a hand at the end of its turn doesn't flick as what it turns to goes round.)
                    float by = CombatFeel.ShieldWristTurn * (1f - Mathf.Exp(-off / Mathf.Max(1f, CombatFeel.ShieldWristTurn))) * weight;
                    hand.rotation = Quaternion.AngleAxis(by, about.normalized) * hand.rotation;
                }

                KeepShieldOff(upper, fore, hand, now);
                if (BoardOnSlot(out var boardLocal))
                {
                    shieldBoardFrom = bodyBack * (slot.TransformPoint(boardLocal) - hand.position);
                    shieldBoardKnown = true;
                }
            }
        }

        /// <summary>
        /// The shield's board kept out of its own head and trunk, however the arm came to have it there (raised to a
        /// blow, knocked back by one, carried through its own strike): a board the pose has in them is moved off, the fist
        /// with it, by as far as it is in and <see cref="ShieldClear"/> more, and the arm solved to there with its elbow
        /// the way it was.
        /// </summary>
        private void KeepShieldOff(Transform upper, Transform fore, Transform hand, Vector3 pole)
        {
            var slot = Anim.SlotWeaponL;
            if (slot == null || !BoardOnSlot(out var boardLocal))
            {
                return;
            }

            float reach = (Anim.RestLocalPosition(PartId.ForearmL).magnitude + Anim.RestLocalPosition(PartId.HandL).magnitude) * 0.97f;
            float radius = ShieldRadius * Size;
            Vector3 target = hand.position;
            float left = 0f;
            for (int pass = 0; pass < 4; pass++)
            {
                Vector3 centre = slot.TransformPoint(boardLocal), boardLooks = slot.forward;
                float deepest = 0f;
                Vector3 moveOff = Vector3.zero;
                foreach (var id in ShieldKeptOutOf)
                {
                    if (!PoseSolid(id, out Vector3 a, out Vector3 b, out float thick))
                    {
                        continue;
                    }

                    for (int i = 0; i <= 4; i++)
                    {
                        float gap = DiscGap(Vector3.Lerp(a, b, i / 4f), centre, boardLooks, radius, out Vector3 away) - thick - ShieldClear * Size;
                        if (gap < deepest)
                        {
                            deepest = gap;
                            moveOff = away * -gap;
                        }
                    }
                }

                left = deepest;
                if (deepest >= 0f || pass == 3)
                {
                    break;
                }

                ShieldKeptOut++;
                target = upper.position + Vector3.ClampMagnitude(target + moveOff - upper.position, reach);
                TwoBoneIK.Solve(upper, fore, hand, target, pole);
            }

            // (For the reports: how deep the posed board is still in its own body.)
            ShieldStillIn += left < -(ShieldClear + 0.01f) * Size ? 1 : 0;
            ShieldStillInMost = Mathf.Max(ShieldStillInMost, -left - ShieldClear * Size);
        }

        /// <summary>The parts of its own body its shield's board is kept out of, and by how much clear of them (metres at a Medium's size).</summary>
        private static readonly PartId[] ShieldKeptOutOf = { PartId.Head, PartId.Chest, PartId.Spine, PartId.Pelvis };

        public const float ShieldClear = 0.06f;

        /// <summary>For the reports: times the board was moved off its own body.</summary>
        public int ShieldKeptOut;

        /// <summary>For the reports: steps the posed board was still over a centimetre in its own body after being moved off, and the deepest (metres).</summary>
        public int ShieldStillIn;
        public float ShieldStillInMost;

        /// <summary>A part's solid as the pose has the part (world): a line from one end to the other and its thickness about that line; a ball's ends are one.</summary>
        private bool PoseSolid(PartId id, out Vector3 a, out Vector3 b, out float radius)
        {
            a = b = Vector3.zero;
            radius = 0f;
            var part = Rig.Part(id);
            var posed = Anim.Part(id);
            if (part == null || posed == null || part.partCollider == null)
            {
                return false;
            }

            var solid = part.partCollider;
            var t = solid.transform;
            float scale = Mathf.Max(Mathf.Abs(t.lossyScale.x), Mathf.Max(Mathf.Abs(t.lossyScale.y), Mathf.Abs(t.lossyScale.z)));
            Vector3 la, lb;
            if (solid is SphereCollider ball)
            {
                la = lb = ball.center;
                radius = ball.radius * scale;
            }
            else if (solid is CapsuleCollider capsule)
            {
                Vector3 axis = capsule.direction == 0 ? Vector3.right : capsule.direction == 1 ? Vector3.up : Vector3.forward;
                float half = Mathf.Max(0f, 0.5f * capsule.height - capsule.radius);
                la = capsule.center - axis * half;
                lb = capsule.center + axis * half;
                radius = capsule.radius * scale;
            }
            else if (solid is BoxCollider box)
            {
                // (A box as the ball that fits inside it along its longest side.)
                Vector3 size = box.size;
                float thin = Mathf.Min(size.x, Mathf.Min(size.y, size.z)), longest = Mathf.Max(size.x, Mathf.Max(size.y, size.z));
                Vector3 axis = longest == size.x ? Vector3.right : longest == size.y ? Vector3.up : Vector3.forward;
                float half = Mathf.Max(0f, 0.5f * (longest - thin));
                la = box.center - axis * half;
                lb = box.center + axis * half;
                radius = 0.5f * thin * scale;
            }
            else
            {
                return false;
            }

            // (From the physical part's own solid to the posed part: the same place on the part.)
            a = posed.TransformPoint(part.transform.InverseTransformPoint(t.TransformPoint(la)));
            b = posed.TransformPoint(part.transform.InverseTransformPoint(t.TransformPoint(lb)));
            return true;
        }

        /// <summary>How far a point is from a round board (its centre, the way its face looks, its radius), and the way the board would move to get further from it.</summary>
        private static float DiscGap(Vector3 point, Vector3 centre, Vector3 looks, float radius, out Vector3 away)
        {
            Vector3 d = point - centre;
            float over = Vector3.Dot(d, looks);
            Vector3 flat = d - looks * over;
            float out_ = flat.magnitude;
            Vector3 nearest = centre + (out_ > radius ? flat / out_ * radius : flat);
            Vector3 from = nearest - point;
            float gap = from.magnitude;
            away = gap > 1e-5f ? from / gap : looks * (over >= 0f ? -1f : 1f);
            return gap;
        }

        private void RaiseShield(float dt, Vector3 striker)
        {
            // (The guard holds the shield as its clip does; it moves only to meet a blow.)
            blockWeight = 0f;
            float coming = Blow.HasValue ? Blow.Value.Coming : -1f;
            if (Blow.HasValue && Blow.Value.Serial != lastShieldSerial)
            {
                lastShieldSerial = Blow.Value.Serial;
                NewShieldBlow();
                incomingFor = 0f;
            }

            if (Blow.HasValue)
            {
                lastBlowSeenAt = clock;
            }

            float lack = Blow.HasValue ? blowLack : ShieldLack;

            lastComing = coming;
            incomingFor = IncomingBlade.HasValue ? incomingFor + dt : 0f;
            // (A novice sees the blow late; once it has passed, the shield stays up a moment, longer for a novice.)
            // (An expert reads the wind-up; a novice moves only once the blade itself is coming, the last moment before contact.)
            float toContact = Blow.HasValue ? Blow.Value.ToContact : 0f;
            bool sees = IncomingBlade.HasValue && incomingFor > Mathf.Lerp(SeeBlowAfter, NoviceSeeAfter, lack)
                        && toContact <= Mathf.Min(SeesBefore(1f - lack), CombatFeel.ShieldMovesWithin) && Blocking && !GuardOpen;
            // (Not before the blow can be read: through the first part of it (the scene's Combat Feel's share of its time
            // from its start to its contact) it is a wind-up, and a shield that went to where it would land read the
            // striker's mind.)
            if (sees && Blow.HasValue && seenSerial != Blow.Value.Serial)
            {
                float whole = Blow.Value.Coming + Blow.Value.ToContact;
                sees = whole <= 1e-3f || Blow.Value.Coming >= CombatFeel.ShieldReadsFrom * whole;
                if (sees)
                {
                    ShieldReadAt.Add(whole > 1e-3f ? Blow.Value.Coming / whole : 1f);
                    ShieldReadBefore.Add(Blow.Value.ToContact);
                }
            }
            backIn = sees ? Mathf.Lerp(ExpertBackAfter, NoviceBackAfter, lack) : Mathf.Max(0f, backIn - dt);
            // (Knocked open: the shield meets nothing until the guard is back.)
            float wantIntercept = !GuardOpen && (sees || (backIn > 0f && Blocking && interceptWeight > 0f)) ? 1f : 0f;
            // (A poor shield lets its guard fall back once the blow it met has passed, as a combo's next comes on unseen: a
            // good one keeps it where it was until it sees where the next is going.)
            if (Blow.HasValue && !sees && seenSerial != Blow.Value.Serial && blowLack > 0.5f)
            {
                wantIntercept = 0f;
            }
            interceptWeight = Mathf.MoveTowards(interceptWeight, wantIntercept, dt / BlockRaise);
            float weight = Mathf.Max(blockWeight, interceptWeight);
            var hand = Anim.Part(PartId.HandL);
            if (weight <= 0f)
            {
                shieldAtKnown = false;
                shieldFaceKnown = false;
                shieldElbowKnown = false;
                shieldBoardKnown = false;
                return;
            }

            var upper = Anim.Part(PartId.UpperArmL);
            var fore = Anim.Part(PartId.ForearmL);
            Vector3 chest = Anim.Part(PartId.Chest).position;
            float reach = (Anim.RestLocalPosition(PartId.ForearmL).magnitude + Anim.RestLocalPosition(PartId.HandL).magnitude) * 0.9f;
            Vector3 want, face;
            // (The plan "Shield and blade contact", step 2: on the blade's own path, where it first comes within the guard
            // of the body, square to its motion there. What the blow would strike first, not where it is aimed: a chop
            // aimed at the hips passes the head on its way.)
            // (Held, not chased (the playtest: the board moved and turned back and forth as the blade came): planned once as
            // the blow is first read, kept in the body's own frame so it goes with the body, and planned again only when a
            // later read moves it or turns it by more than the scene's Combat Feel allows. Once
            // the blade is inside the guard, the plan stands.)
            Vector3 freshAt = Vector3.zero, freshNormal = Vector3.zero, freshMotion = Vector3.zero, freshWay = Vector3.zero;
            float meetIn = 0f;
            bool inside = false;
            // (A blow is planned for only once it is seen: until then a shield still up from the last block stays where that
            // was (a combo's next blow, which a novice sees late, finds it there or not), never tracking the new one unseen.)
            if (sees && Blow.HasValue)
            {
                seenSerial = Blow.Value.Serial;
            }

            bool seen = Blow.HasValue && seenSerial == Blow.Value.Serial;
            bool fresh = interceptWeight > 0f && seen && PlanMeet(Blow.Value, out freshAt, out freshNormal, out freshMotion, out freshWay, out meetIn, out inside);
            bool havePlan = interceptWeight > 0f && Blow.HasValue && planSerial == Blow.Value.Serial;
            bool holdLast = interceptWeight > 0f && !seen && planSerial >= 0 && easedSerial == planSerial;
            if (fresh || havePlan || holdLast)
            {
                var rootTurn = Quaternion.Euler(0f, Loco.Facing, 0f);
                var rootBack = Quaternion.Inverse(rootTurn);
                if (fresh && !(havePlan && inside))
                {
                    shieldReread -= dt;
                    if (!havePlan || shieldReread <= 0f)
                    {
                        // (How well it reads the blow: a novice's board goes where it guessed (off by its misread, across the
                        // blade's motion, and its face turned by its misline), keeps to that first guess a while, and holds at
                        // the chest against a low cut.)
                        float hips = Anim.Part(PartId.Pelvis).position.y;
                        if (!coversLow && freshAt.y < hips)
                        {
                            freshAt = new Vector3(freshAt.x, chest.y, freshAt.z);
                        }

                        Vector3 side = Vector3.ProjectOnPlane(shieldMisread, freshMotion.sqrMagnitude > 1e-8f ? freshMotion.normalized : freshNormal);
                        freshAt += side.sqrMagnitude > 1e-8f ? side.normalized * shieldMisread.magnitude : Vector3.zero;
                        freshNormal = (shieldMisline * freshNormal).normalized;
                        Vector3 heldAt = Loco.Position + rootTurn * planAt, heldNormal = rootTurn * planNormal;
                        if (havePlan && Vector3.Angle(freshNormal, heldNormal) > 120f)
                        {
                            freshNormal = -freshNormal;
                        }

                        if (!havePlan || Vector3.Distance(freshAt, heldAt) > CombatFeel.ShieldReplanBy * Size || Vector3.Angle(freshNormal, heldNormal) > CombatFeel.ShieldReplanTurn)
                        {
                            // (An expert's face slanted so a cut glances off it, away from the body: turned about the blade's
                            // own line, the way chosen once.)
                            Vector3 freshFace = freshNormal;
                            float slantBy = ExpertShieldSlant * (1f - lack);
                            if (slantBy > 0.5f && freshWay.sqrMagnitude > 1e-6f && !Blow.Value.Thrust)
                            {
                                Vector3 outward = Vector3.ProjectOnPlane(freshAt - chest, freshWay);
                                Vector3 tilted = Quaternion.AngleAxis(slantBy, freshWay) * freshNormal;
                                Vector3 slide = Vector3.ProjectOnPlane(freshMotion, tilted);
                                freshFace = Vector3.Dot(slide, outward) >= 0f ? tilted : Quaternion.AngleAxis(-slantBy, freshWay) * freshNormal;
                            }

                            planSerial = Blow.Value.Serial;
                            planAt = rootBack * (freshAt - Loco.Position);
                            planNormal = rootBack * freshNormal;
                            planFace = rootBack * freshFace;
                            if (!havePlan)
                            {
                                firstPlanAt = planAt;
                                firstPlanToContact = Blow.Value.ToContact;
                            }

                            Replans += havePlan ? 1 : 0;
                        }

                        shieldReread = lack >= 0.999f ? float.MaxValue : lack <= 0.001f ? 0f : 0.4f * lack;
                    }
                }

                // (A plan read again eases over to where it now goes, never a jump.)
                if (easedSerial != planSerial)
                {
                    easedSerial = planSerial;
                    easedAt = planAt;
                    easedNormal = planNormal;
                    easedFace = planFace;
                }
                else
                {
                    // (Quicker the less time is left: eased at its full pace, a read just before contact never arrived.)
                    float ease = 1f - Mathf.Exp(-dt / Mathf.Clamp(0.3f * meetIn, 0.02f, PlanEase));
                    easedAt = Vector3.Lerp(easedAt, planAt, ease);
                    easedNormal = Vector3.Slerp(easedNormal, planNormal, ease);
                    easedFace = Vector3.Slerp(easedFace, planFace, ease);
                }

                Vector3 meetAt = Loco.Position + rootTurn * easedAt, meetNormal = rootTurn * easedNormal;
                face = rootTurn * easedFace;
                // (The board's centre on the meeting point: the hand behind it by the board's own offset in the fist, as the
                // face will be turned. From the clip's pose of the hand this step and the board's fixed place on its slot:
                // taken from the physical board as it was, the hand went back and forth between two places step to step.)
                Vector3 boardFromHand = -meetNormal * (0.08f * Size);
                var posedSlot = Anim.SlotWeaponL;
                if (CombatFeel.ShieldFixed && shieldBoardKnown)
                {
                    // (Fixed in the fist: where the board was from the hand as the arm last held it, which is where the arm
                    // that brings it there will have it, near enough; what is left is taken up step by step.)
                    boardFromHand = rootTurn * shieldBoardFrom;
                }
                else if (posedSlot != null && BoardOnSlot(out var boardLocal))
                {
                    var slotThen = CombatFeel.ShieldFixed ? posedSlot.rotation : Quaternion.FromToRotation(posedSlot.forward, face) * posedSlot.rotation;
                    boardFromHand = (posedSlot.position - hand.position) + slotThen * boardLocal;
                }

                want = meetAt - boardFromHand;
                DebugWant = want;
                DebugFace = face;
                if (Blow.HasValue && seen)
                {
                    TraceMeet(Blow.Value, meetAt, meetNormal, meetIn);
                }
            }
            else
            {
                if (interceptWeight > 0f && Blow.HasValue && ShieldTrace != null && Blow.Value.Serial != tracedShieldSerial)
                {
                    tracedShieldSerial = Blow.Value.Serial;
                    ShieldTrace(Traced + string.Format(System.Globalization.CultureInfo.InvariantCulture, "blow {0}: {1:0.00} s to contact, its path comes no nearer the body than {2:0.00} m (the guard {3:0.00}): held across its line",
                        Blow.Value.Serial, Blow.Value.ToContact, LastMeetNearest, GuardShell * Size));
                }

                Vector3 aimed = IncomingAim ?? BlockAt ?? chest;
                Vector3 from = IncomingBlade.HasValue && interceptWeight > 0f ? IncomingBlade.Value : striker + Vector3.up * (aimed.y - striker.y);
                Vector3 toBlade = from - aimed;
                toBlade = toBlade.sqrMagnitude > 1e-4f ? toBlade.normalized : Quaternion.Euler(0f, Loco.Facing, 0f) * Vector3.forward;
                // (Read from the wind-up: across the line the blow will come in on, not after the weapon, which is
                // faster than any shield at the end.)
                // (A novice chases the blade where it is now instead of closing the line it will come in on.)
                if (IncomingFrom.HasValue && IncomingBlade.HasValue && interceptWeight > 0f)
                {
                    toBlade = Vector3.Slerp(toBlade, IncomingFrom.Value.normalized, ReadLine * (1f - lack));
                }

                // (Not behind itself: the shield stays in front of the body.)
                Vector3 ahead = Quaternion.Euler(0f, Loco.Facing, 0f) * Vector3.forward;
                if (Vector3.Dot(toBlade, ahead) < 0.2f)
                {
                    toBlade = (toBlade + ahead * (0.2f - Vector3.Dot(toBlade, ahead))).normalized;
                }

                want = aimed + toBlade * (0.32f * Size);
                // (The wrist straight along the forearm, the shield turned in the fist to face the blow: an expert's slanted
                // so a cut glances off, a novice's square on.)
                face = toBlade;
                float slant = ExpertSlant * (1f - lack);
                if (slant > 0.5f && IncomingFrom.HasValue)
                {
                    Vector3 across = Vector3.Cross(IncomingFrom.Value, Vector3.up);
                    if (across.sqrMagnitude > 1e-4f)
                    {
                        face = Quaternion.AngleAxis(slant, across.normalized) * face;
                    }
                }
            }

            float short_ = (want - upper.position).magnitude - reach;
            if (short_ > 0.05f * Size && Blow.HasValue)
            {
                ShortSteps++;
                MostShort = Mathf.Max(MostShort, short_);
            }

            want = upper.position + Vector3.ClampMagnitude(want - upper.position, reach);
            // (Held in the body's own frame, so it goes with the body as it steps and turns; and moved there as an arm
            // moves, speeding up and settling in (a critically damped spring, the scene's Combat Feel's settle time), never
            // faster than the arm can: it doesn't jerk from place to place.)
            var bodyTurn = Quaternion.Euler(0f, Loco.Facing, 0f);
            var bodyBack = Quaternion.Inverse(bodyTurn);
            Vector3 wantLocal = bodyBack * (want - Loco.Position);
            if (CombatFeel.ShieldFixed)
            {
                // (Where a fist holds a shield: before the chest, no higher than the head (the board's rim covers what is
                // above), from just across the body's middle to out past its own shoulder. Never over the head or behind it.)
                Vector3 chestLocal = bodyBack * (chest - Loco.Position), headLocal = bodyBack * (Anim.Part(PartId.Head).position - Loco.Position);
                wantLocal.z = Mathf.Max(wantLocal.z, chestLocal.z + FistAhead * Size);
                wantLocal.y = Mathf.Min(wantLocal.y, headLocal.y + FistOverNeck * Size);
                wantLocal.x = Mathf.Clamp(wantLocal.x, chestLocal.x - FistOut * Size, chestLocal.x + FistAcross * Size);
            }

            if (!shieldAtKnown)
            {
                shieldAt = bodyBack * (hand.position - Loco.Position);
                shieldAtV = Vector3.zero;
                shieldAtKnown = true;
            }

            float most = Mathf.Lerp(ShieldSpeed, NoviceShieldSpeed, lack) * Size;
            float settle = Mathf.Max(0.01f, CombatFeel.ShieldSettle);
            CriticalSpring(ref shieldAt, ref shieldAtV, wantLocal, 4f / settle, dt);
            shieldAtV = Vector3.ClampMagnitude(shieldAtV, most);
            Vector3 target = Vector3.Lerp(hand.position, Loco.Position + bodyTurn * shieldAt, weight);
            if (CombatFeel.ShieldFixed)
            {
                HoldShieldFast(dt, target, weight, face, lack, bodyTurn);
                return;
            }

            var turn = bodyTurn;
            // (The elbow behind the board, away from its face, as it bends: the arm then goes into the board's back, not
            // out through its face (the playtest: raised high, the board cut through the forearm). Its face is kept; only
            // what this leaves through the board turns it, at the end.)
            Vector3 elbowWay = turn * new Vector3(-1f, -0.6f, 0f);
            Vector3 faceNow = bodyTurn * shieldFace;
            if (faceNow.sqrMagnitude > 1e-6f)
            {
                elbowWay = Vector3.Lerp(elbowWay.normalized, -faceNow.normalized, ElbowBehindBoard * weight);
            }

            // (And out from the body, never into its own chest: at least this much of its way out to the side of the shoulder.)
            Vector3 outSide = Vector3.ProjectOnPlane(upper.position - Anim.Part(PartId.Chest).position, Vector3.up);
            if (outSide.sqrMagnitude > 1e-6f && elbowWay.sqrMagnitude > 1e-6f)
            {
                outSide.Normalize();
                elbowWay.Normalize();
                float side = Vector3.Dot(elbowWay, outSide);
                if (side < ElbowOut)
                {
                    elbowWay = (elbowWay + outSide * (ElbowOut - side)).normalized;
                }
            }

            // (Which way the elbow points as the pose has the arm: where it goes over to the shield's way from.)
            Vector3 poseWay = TwoBoneIK.BendWay(upper, fore, hand);
            TwoBoneIK.Solve(upper, fore, hand, target, elbowWay);
            // (Still in its own chest (a board held across the body): solved again with the elbow turned well out to the side,
            // and once more straight out, until it is clear.)
            if (outSide.sqrMagnitude > 0.5f)
            {
                for (int tries = 1; tries <= 2 && ElbowGap() < ElbowClear * Size; tries++)
                {
                    elbowWay = (elbowWay + outSide * (tries * 1.5f)).normalized;
                    TwoBoneIK.Solve(upper, fore, hand, target, elbowWay);
                }
            }

            // (The elbow goes over to there as the hand does to its place, and comes back with it, turning about the
            // line from the shoulder to the hand no faster than an elbow does. Put there at once, in the step the
            // shield began to move and again in the step it was let go, and whenever the arm was solved again to
            // clear the chest, the elbow jumped to the other side of the arm, up to 47 cm in a step, the hand hardly
            // moving: a limb that spun.)
            Vector3 line = target - upper.position;
            if (line.sqrMagnitude > 1e-8f)
            {
                line.Normalize();
                Vector3 to = Vector3.ProjectOnPlane(elbowWay, line), from = Vector3.ProjectOnPlane(poseWay, line);
                if (to.sqrMagnitude > 1e-6f)
                {
                    Vector3 wanted = from.sqrMagnitude > 1e-6f ? Vector3.ProjectOnPlane(Vector3.Slerp(from.normalized, to.normalized, weight), line) : to;
                    wanted = wanted.sqrMagnitude > 1e-6f ? wanted.normalized : to.normalized;
                    Vector3 was = shieldElbowKnown ? Vector3.ProjectOnPlane(bodyTurn * shieldElbow, line) : from;
                    was = was.sqrMagnitude > 1e-6f ? was.normalized : wanted;
                    Vector3 now = Vector3.ProjectOnPlane(Vector3.RotateTowards(was, wanted, ShieldElbowTurn * Mathf.Deg2Rad * dt, 0f), line);
                    now = now.sqrMagnitude > 1e-6f ? now.normalized : wanted;
                    shieldElbow = bodyBack * now;
                    shieldElbowKnown = true;
                    TwoBoneIK.Solve(upper, fore, hand, target, now);
                }
            }

            // (And turned no faster than the arm can turn it: a novice's shield can't whip round to catch a blade at the last
            // moment.)
            if (!shieldFaceKnown || shieldFace.sqrMagnitude < 1e-6f)
            {
                // (From where the guard holds it: started from straight ahead, the board snapped round 35 degrees in the
                // step it began to move.)
                shieldFace = bodyBack * (Anim.SlotWeaponL != null ? Anim.SlotWeaponL.forward : bodyTurn * Vector3.forward);
                shieldFaceKnown = true;
            }

            // (Turned smoothly, settling in as the hand does, and no faster than the arm can turn it; in the body's frame.)
            // (Its arm kept behind it as it turns: the way to turn to is the one that keeps it so, eased into like any other.)
            Vector3 faceLocal = bodyBack * KeepArmBehind(face);
            Vector3 eased = Vector3.Slerp(shieldFace, faceLocal, 1f - Mathf.Exp(-dt * 3f / Mathf.Max(0.01f, CombatFeel.ShieldFaceSettle)));
            shieldFace = Vector3.RotateTowards(shieldFace, eased, Mathf.Lerp(ShieldTurnSpeed, NoviceShieldTurnSpeed, lack) * Mathf.Deg2Rad * dt, 0f);
            Poser.FaceShieldAt(KeepArmBehind(bodyTurn * shieldFace), Mathf.Max(weight, Poser.ClipArms * Poser.ShieldTurned));
        }



        private int planSerial = -1, seenSerial = -1;
        private Vector3 planAt, planNormal, planFace;
        private int easedSerial = -1;
        private Vector3 easedAt, easedNormal, easedFace;

        /// <summary>How quickly a plan read again eases over to its new place (seconds, the e-folding).</summary>
        public const float PlanEase = 0.06f;

        private Vector3 firstPlanAt;
        private float firstPlanToContact;

        /// <summary>How far an expert's shield face is slanted so a cut glances off it (degrees).</summary>
        public const float ExpertShieldSlant = 30f;

        /// <summary>The shield's plan for the blow it is meeting (world, from its body's own frame now): the first and the one it holds, and how long before contact it was first made (false: no plan for this blow).</summary>
        public bool ShieldPlan(int serial, out Vector3 first, out Vector3 held, out float madeBefore)
        {
            var turn = Quaternion.Euler(0f, Loco.Facing, 0f);
            first = Loco.Position + turn * firstPlanAt;
            held = Loco.Position + turn * easedAt;
            madeBefore = firstPlanToContact;
            return planSerial == serial && easedSerial == serial;
        }

        /// <summary>Plans moved by a later read of the same blow (for the logs).</summary>
        public int Replans;

        /// <summary>Where the shield's hand is going and the way its face is to look, this step (for the runners' traces).</summary>
        public Vector3 DebugWant, DebugFace;

        /// <summary>How far from the body's line (hips to the top of the head) the shield meets a blade (metres at a Medium's size): where the board is held, about a forearm out from the body's front.</summary>
        public const float GuardShell = 0.42f;

        /// <summary>
        /// Where the blow's blade will first come within <see cref="GuardShell"/> of the body (the plan "Shield and blade
        /// contact", step 2), read from its path (<see cref="BlowRead.GripPath"/> to <see cref="BlowRead.TipPath"/>): the
        /// point on the blade nearest the body then, the face square to the blade's motion there (pointing out, back up
        /// the blow), that motion, the blade's way, and how soon (seconds). False: the path never comes that near.
        /// </summary>
        private bool PlanMeet(in BlowRead b, out Vector3 at, out Vector3 normal, out Vector3 motion, out Vector3 bladeWay, out float inSeconds, out bool inside)
        {
            at = normal = motion = bladeWay = Vector3.zero;
            inSeconds = 0f;
            inside = false;
            if (b.GripPath == null || b.TipPath == null || b.GripPath.Length < 2)
            {
                return false;
            }

            // (The finer path about contact where there is one: a fast blow's blade sweeps a metre between two of the coarse
            // path's points, and the board went where the line between them crossed, not the blade.)
            bool fine = UseFinePath && b.FineGrip != null && b.FineTip != null && b.FineGrip.Length >= 2;
            var G = fine ? b.FineGrip : b.GripPath;
            var T = fine ? b.FineTip : b.TipPath;
            var read = b;
            float StepSeconds(float along) => fine ? Mathf.Max(0f, read.FineStart + along * read.FineSeconds) : read.SecondsTo(along);

            Vector3 lo = Anim.Part(PartId.Pelvis).position, hi = Anim.Part(PartId.Head).position + Vector3.up * (0.1f * Size);
            float shell = GuardShell * Size;
            Vector3 shoulder = Anim.Part(PartId.UpperArmL).position;
            // (How far from the shoulder the board's centre can be: the arm's reach and the board's own offset in front of
            // the fist.)
            float reach = (Anim.RestLocalPosition(PartId.ForearmL).magnitude + Anim.RestLocalPosition(PartId.HandL).magnitude) * 0.9f + 0.08f * Size;
            float prevD = float.MaxValue, prevS = 0f;
            Vector3 prevAt = Vector3.zero;
            int last = fine ? G.Length - 1 : Mathf.Min(G.Length - 1, StrikeHits.ContactPoint + 2);
            LastMeetNearest = float.MaxValue;
            int found = -1;
            float foundU = 1f, foundS = 0f;
            for (int k = 0; k <= last; k++)
            {
                Vector3 g = G[k], t = T[k];
                float d = StrikeHits.SegmentsNearest(g, t, lo, hi, out float sb, out _, out Vector3 onBlade, out _);
                LastMeetNearest = Mathf.Min(LastMeetNearest, d);
                if (found < 0 && d <= shell)
                {
                    float u = k > 0 && prevD < float.MaxValue && prevD > d ? Mathf.Clamp01((prevD - shell) / (prevD - d)) : 1f;
                    found = k;
                    foundU = u;
                    foundS = k > 0 ? Mathf.Lerp(prevS, sb, u) : sb;
                    at = k > 0 ? Vector3.Lerp(prevAt, onBlade, u) : onBlade;
                    // (Out of the arm's reach there (a chop from high above, a cut low at the legs): further along the blade's
                    // way in, the first place the arm can reach, else the nearest to it; the board meets the blade there.)
                    if (Vector3.Distance(at, shoulder) > reach)
                    {
                        float best = Vector3.Distance(at, shoulder);
                        for (int j = k; j <= last; j++)
                        {
                            float dj = StrikeHits.SegmentsNearest(G[j], T[j], lo, hi, out float sj, out _, out Vector3 pj, out _);
                            // (Not inside the body: the blade there has already struck it.)
                            if (dj < 0.18f * Size)
                            {
                                break;
                            }

                            // (The point on the blade nearest the shoulder, so far as it is still between the blade's way in
                            // and the body.)
                            StrikeHits.SegmentsNearest(G[j], T[j], shoulder, shoulder, out float sn, out _, out Vector3 pn, out _);
                            float s2 = Mathf.Clamp(sn, Mathf.Min(sj, 0.5f), 1f);
                            Vector3 pick = Vector3.Lerp(G[j], T[j], s2);
                            float dist = Vector3.Distance(pick, shoulder);
                            if (dist < best)
                            {
                                best = dist;
                                at = pick;
                                found = j;
                                foundU = 1f;
                                foundS = s2;
                            }

                            if (dist <= reach)
                            {
                                break;
                            }
                        }
                    }

                    break;
                }

                prevD = d;
                prevS = sb;
                prevAt = onBlade;
            }

            if (found < 0)
            {
                return false;
            }

            // (The blade's motion there: the same point along it, a step apart.)
            int k0 = Mathf.Max(0, found - 1), k1 = Mathf.Min(k0 + 1, G.Length - 1);
            motion = Vector3.Lerp(G[k1], T[k1], foundS) - Vector3.Lerp(G[k0], T[k0], foundS);
            bladeWay = Vector3.Lerp(T[k0] - G[k0], T[found] - G[found], foundU);
            bladeWay = bladeWay.sqrMagnitude > 1e-8f ? bladeWay.normalized : Vector3.up;
            Vector3 square = Vector3.ProjectOnPlane(-motion, bladeWay);
            normal = !b.Thrust && square.sqrMagnitude > 0.04f * motion.sqrMagnitude ? square.normalized
                : motion.sqrMagnitude > 1e-10f ? -motion.normalized : (b.Blade - at).normalized;
            // (Out from the body, toward the blow.)
            Vector3 axisNear = lo + (hi - lo) * Mathf.Clamp01(Vector3.Dot(at - lo, hi - lo) / Mathf.Max(1e-6f, (hi - lo).sqrMagnitude));
            if (Vector3.Dot(normal, at - axisNear) < 0f && Vector3.Dot(normal, -motion) < 0.2f * motion.magnitude)
            {
                normal = -normal;
            }

            inSeconds = StepSeconds(Mathf.Max(0, found - 1) + (found > 0 ? foundU : 0f));
            inside = found == 0;
            return true;
        }

        /// <summary>Steps the shield's hand was wanted further than the arm reaches (by over 5 cm), and the most (metres).</summary>
        public int ShortSteps;
        public float MostShort;

        /// <summary>
        /// The board's face turned no further than keeps its own arm behind it (the playtest: raised high, the board cut
        /// through the forearm): the forearm and the upper arm must point into the board's back, not out through its face.
        /// The face is turned toward the arm as little as that takes.
        /// </summary>
        private Vector3 KeepArmBehind(Vector3 face)
        {
            if (face.sqrMagnitude < 1e-8f)
            {
                return face;
            }

            face = face.normalized;
            Vector3 shoulder = Anim.Part(PartId.UpperArmL).position, elbow = Anim.Part(PartId.ForearmL).position, wrist = Anim.Part(PartId.HandL).position;
            Vector3 upper = wrist - shoulder, fore = wrist - elbow;
            if (upper.sqrMagnitude > 1e-8f)
            {
                face = Behind(face, upper.normalized, UpperArmIntoBoard);
            }

            if (fore.sqrMagnitude > 1e-8f)
            {
                face = Behind(face, fore.normalized, ForearmIntoBoard);
            }

            return face;
        }

        /// <summary>How far the shield arm's elbow is turned to lie behind the board as it bends (0 its guard's way, 1 straight away from the board's face).</summary>
        public const float ElbowBehindBoard = 0.6f;

        /// <summary>How near the shield arm's elbow may come to its own body's line (metres at a Medium's size: the chest is about 0.13 across from it).</summary>
        public const float ElbowClear = 0.16f;

        /// <summary>How far the shield arm's elbow is from its own body's line (hips to the neck), as posed now.</summary>
        private float ElbowGap()
        {
            Vector3 lo = Anim.Part(PartId.Pelvis).position, hi = Anim.Part(PartId.Chest).position + Vector3.up * (0.12f * Size);
            Vector3 e = Anim.Part(PartId.ForearmL).position;
            Vector3 axis = lo + (hi - lo) * Mathf.Clamp01(Vector3.Dot(e - lo, hi - lo) / Mathf.Max(1e-6f, (hi - lo).sqrMagnitude));
            return Vector3.Distance(e, axis);
        }

        /// <summary>How far out to the side at least the shield arm's elbow points as it bends (the cosine to straight out from the shoulder).</summary>
        public const float ElbowOut = 0.5f;

        /// <summary>How far the forearm and the line from the shoulder to the wrist must point into the board's back (the cosine to its face's way): at 0 they lie flat behind it.</summary>
        public const float ForearmIntoBoard = 0.2f, UpperArmIntoBoard = 0.1f;

        private static Vector3 Behind(Vector3 face, Vector3 arm, float least)
        {
            float into = Vector3.Dot(face, arm);
            if (into >= least)
            {
                return face;
            }

            Vector3 across = Vector3.ProjectOnPlane(face, arm);
            across = across.sqrMagnitude > 1e-8f ? across.normalized : Vector3.Cross(arm, Vector3.up).normalized;
            return (across * Mathf.Sqrt(1f - least * least) + arm * least).normalized;
        }

        /// <summary>Steps its own shield arm went through its board (the playtest: the raised board cut through the forearm), and the deepest (metres past the board's back).</summary>
        public int ArmThroughBoard;
        public float ArmThroughBoardMost;

        /// <summary>The nearest the shield arm's elbow came to its own body's line (metres; a body is about 0.13 m across from it).</summary>
        public float ElbowNearestBody = float.MaxValue, ElbowNearestBlocking = float.MaxValue;

        /// <summary>Whether the shield arm (shoulder to elbow, elbow to wrist) goes through its own board, as posed this step; counted.</summary>
        private void CountArmCut()
        {
            var slot = Anim.SlotWeaponL;
            if (slot == null || !BoardOnSlot(out var local))
            {
                return;
            }

            Vector3 c = slot.TransformPoint(local), n = slot.forward;
            float r = 0.95f * ShieldRadius * Size;
            float worst = ArmCut(Anim.Part(PartId.UpperArmL).position, Anim.Part(PartId.ForearmL).position, c, n, r);
            worst = Mathf.Max(worst, ArmCut(Anim.Part(PartId.ForearmL).position, Anim.Part(PartId.HandL).position, c, n, r));
            if (worst > 0.01f * Size)
            {
                ArmThroughBoard++;
                ArmThroughBoardMost = Mathf.Max(ArmThroughBoardMost, worst);
            }

            // (And its elbow kept out of its own chest: the line from the hips to the neck, a body's half width.)
            Vector3 lo = Anim.Part(PartId.Pelvis).position, hi = Anim.Part(PartId.Chest).position + Vector3.up * (0.12f * Size);
            Vector3 e = Anim.Part(PartId.ForearmL).position;
            Vector3 axis = lo + (hi - lo) * Mathf.Clamp01(Vector3.Dot(e - lo, hi - lo) / Mathf.Max(1e-6f, (hi - lo).sqrMagnitude));
            float elbowGap = Vector3.Distance(e, axis);
            ElbowNearestBody = Mathf.Min(ElbowNearestBody, elbowGap);
            if (interceptWeight > 0f)
            {
                ElbowNearestBlocking = Mathf.Min(ElbowNearestBlocking, elbowGap);
            }
        }

        /// <summary>How far a segment goes through a disc (centre, normal, radius) past its back: 0 if it doesn't cross it within the disc.</summary>
        private static float ArmCut(Vector3 p, Vector3 q, Vector3 c, Vector3 n, float r)
        {
            float dp = Vector3.Dot(p - c, n), dq = Vector3.Dot(q - c, n);
            if (dp * dq > 0f || Mathf.Abs(dp - dq) < 1e-6f)
            {
                return 0f;
            }

            Vector3 x = p + (q - p) * (dp / (dp - dq));
            return Vector3.Distance(x, c) <= r ? Mathf.Max(dp, dq) : 0f;
        }

        /// <summary>Whether the shield plans on the blow's finer path about contact (off: tried for combos, it met the blade further from the board's centre; on for experiments with SHIELD_FINE).</summary>
        public static bool UseFinePath = System.Environment.GetEnvironmentVariable("SHIELD_FINE") != null;

        /// <summary>How near the last blow's path came to the body's line (metres), as the shield read it.</summary>
        public float LastMeetNearest { get; private set; }

        /// <summary>The shield's reading of each blow, for the logs (the lab writes it into the session's log).</summary>
        public static System.Action<string> ShieldTrace;

        private int tracedShieldSerial = -1;

        /// <summary>Who this fighter is in a trace (the lab's "the walker", "the dummy"), or none.</summary>
        public string TraceName;

        private string Traced => string.IsNullOrEmpty(TraceName) ? "" : TraceName + ", ";
        private Vector3 tracedMeet;

        /// <summary>Its plan for a blow, traced when it is new and when it moves by more than 10 cm: where it will meet the blade, against its own body.</summary>
        private void TraceMeet(in BlowRead b, Vector3 at, Vector3 normal, float inSeconds)
        {
            if (ShieldTrace == null || (b.Serial == tracedShieldSerial && Vector3.Distance(at, tracedMeet) < 0.1f * Size))
            {
                return;
            }

            bool again = b.Serial == tracedShieldSerial;
            tracedShieldSerial = b.Serial;
            tracedMeet = at;
            var back = Quaternion.Inverse(Quaternion.Euler(0f, Loco.Facing, 0f));
            Vector3 local = back * (at - Loco.Position), n = back * normal;
            string V(Vector3 v) => string.Format(System.Globalization.CultureInfo.InvariantCulture, "({0:0.00},{1:0.00},{2:0.00})", v.x, v.y, v.z);
            ShieldTrace(Traced + string.Format(System.Globalization.CultureInfo.InvariantCulture, "blow {0}{1}: {2:0.00} s to contact, meets the blade in {3:0.00} s at {4}, the face {5}{6}",
                b.Serial, again ? " read again" : "", b.ToContact, inSeconds, V(local), V(n), b.Predicted ? " (read ahead)" : ""));
        }

        /// <summary>The softened holds coming back.</summary>
        private void Recover(float dt)
        {
            for (int r = 1; r < 5; r++)
            {
                if (pinLeft[r] <= 0f)
                {
                    continue;
                }

                pinLeft[r] = Mathf.Max(0f, pinLeft[r] - dt);
                float u = 1f - pinLeft[r] / Mathf.Max(0.01f, pinTime[r]);
                Hold((BodyRegion)r, Mathf.Lerp(pinFrom[r], 1f, u * u * (3f - 2f * u)));
            }
        }

        private void Soften(BodyRegion region, float share, float over)
        {
            int r = (int)region;
            if (region == BodyRegion.Legs || (pinLeft[r] > 0f && pinFrom[r] < share))
            {
                return;
            }

            pinFrom[r] = share;
            pinTime[r] = pinLeft[r] = over;
            Hold(region, share);
        }

        /// <summary>The most a blow sends a struck part on its own (m/s at a Medium's size), and a struck shield arm.</summary>
        public const float MostPartSpeed = 5f, MostShieldSpeed = 2f;

        /// <summary>The most a blow pushes the whole dummy back (m/s).</summary>
        /// <summary>The most a blow drives the body back (m/s; the scene's Combat Feel sets it).</summary>
        public static float MostKnock => CombatFeel.MostDrive;

        /// <summary>
        /// A blow lands: on <paramref name="part"/>, or on the shield. What the part takes knocks it (its
        /// hold softened, coming back), the rest pushes the walk back.
        /// </summary>
        /// <summary>
        /// A weapon touching it outside a blow (a wind-up or a recovery passing through, a guard held too close): the
        /// part it touches pushed by the weapon's weight at its speed (<paramref name="impulse"/>, N·s), no harm done.
        /// Nothing passes through the body without its feeling it.
        /// </summary>
        public void Touch(PartId part, bool shield, bool sword, Vector3 point, Vector3 impulse)
        {
            if (impulse.sqrMagnitude < 1e-6f)
            {
                return;
            }

            if (Fallen)
            {
                PushFallen(shield ? PartId.HandL : sword ? PartId.HandR : part, impulse, 0.5f);
                Touches++;
                return;
            }

            if (sword)
            {
                KnockSword(impulse, point);
            }
            else if (shield)
            {
                KnockArm(1, impulse, point);
                Recoil(impulse, point, PartId.HandL, 0.3f * ReactScale);
            }
            else
            {
                var region = AuthorityController.RegionOf(part);
                Recoil(impulse, point, part, 0.6f * ReactScale);
                if (region == BodyRegion.ArmL || region == BodyRegion.ArmR)
                {
                    KnockArm(region == BodyRegion.ArmR ? 0 : 1, impulse, point);
                }
                else if (region == BodyRegion.Legs)
                {
                    KnockLeg(part, impulse, point);
                }
            }

            Touches++;
        }

        public int Touches;

        /// <param name="drive">How much of the blow's weight drives the whole body back across the ground (1: all of it; less for a combo's blows before its last, which the combo has to follow).</param>
        public void TakeHit(PartId part, bool shield, Vector3 impulse, float partShare, float poiseDamage, Combat.Reaction reaction, Vector3 point, bool parried = false, bool bound = false, float drive = 1f,
            float heavier = -1f)
        {
            // (How much heavier than its weapon's own weight a blow on a shield is felt: the scene's Combat Feel's, or what
            // the striker's script has left of it once its weapon's mass in the physics has taken its share.)
            if (heavier < 0f)
            {
                heavier = CombatFeel.WeaponWeight;
            }

            lastHit = clock;
            if (Fallen)
            {
                // (Down or getting up: no standing reaction. Its clip and its softened holds would pin the ragdoll to a
                // standing pose: the playtest, it was hauled up into the air and dropped again. The blow only knocks
                // the part it struck.)
                PushFallen(shield ? PartId.HandL : parried ? PartId.HandR : part, impulse, partShare);
                if (!shield && !parried)
                {
                    Hits++;
                }

                LastReaction = Footing == Footings.Down ? "struck lying down" : "struck getting up";
                return;
            }

            // (A struck leg is knocked along the blow: the body's reaction comes on top.)
            string swept = "";
            if (!shield && !parried && AuthorityController.RegionOf(part) == BodyRegion.Legs && part != PartId.Pelvis)
            {
                KnockLeg(part, impulse, point);
                swept = SweepLeg(part, impulse);
                if (Fallen)
                {
                    LastReaction = swept;
                    return;
                }
            }

            // (The blow's force on the physical body, on top of what the weapon's touch does: on the part struck, on a
            // shield's forearm, on a parrying sword's hand, and a share on the parts joined to it.)
            if (Upper != null && CombatFeel.HitForce > 0f)
            {
                HitForceGiven += Upper.Strike(shield ? PartId.ForearmL : parried ? PartId.HandR : part, impulse * CombatFeel.HitForce, point);
                HitForces++;
            }

            if (parried)
            {
                // Parried: the blow on the sword knocks the sword arm and rocks the body; the rest drives it back, as a
                // shield's block does.
                Parries++;
                Blocks++;
                Recoil(impulse, point, PartId.HandR, 0.4f * ReactScale);
                // (Bound (the plan "Weight and momentum", step 4): the bind gives the sword back with the blow on it, the
                // two weapons together; knocked here, the sword left the weapon it was meeting.)
                if (!bound)
                {
                    KnockSword(impulse, point);
                }
                Poise = Mathf.Max(0f, Poise - poiseDamage);
                Vector3 push = impulse * KnockScale / Mass;
                push.y = 0f;
                if (!Anchored && push.magnitude > DrivenFrom)
                {
                    pendingKnock = Vector3.ClampMagnitude(push, MostKnock);
                    pushAt = clock;
                    LastReaction = $"parried with {Parry?.CurrentName}, then driven back at {pendingKnock.magnitude:0.0} m/s";
                }
                else
                {
                    LastReaction = $"parried with {Parry?.CurrentName}";
                }

                return;
            }

            // The pose shows it (the fight's recoil): the pinned body is pulled back to its pose each step,
            // so a kick on the physical part alone hardly shows.
            // (Its side: which way the blow throws the body, against where it faces. Off the front (thrown sideways or
            // forward: a second attacker's from beside or behind), the bend and the lean are stronger, so it reads as
            // contact: the plan "Fighting off several attackers", step 6.)
            var thrown = BlowSideOf(impulse);
            float offFront = thrown == BlowSide.Back ? 1f : CombatFeel.OffFrontBend;
            float react = (reaction == Combat.Reaction.Stagger ? 1.6f : reaction == Combat.Reaction.Flinch ? 1.25f : 1f) * ReactScale * offFront;
            Recoil(impulse, point, shield ? PartId.HandL : part, shield ? 0.6f * react : react);
            if (!shield)
            {
                AddImpact(point, impulse, offFront);
            }

            var armRegion = shield ? BodyRegion.ArmL : AuthorityController.RegionOf(part);
            bool knockedOpen = false;
            if (shield)
            {
                // (The plan "Weight and momentum", step 4: the shield arm gives by the blow's weight; heavy enough, the
                // guard is knocked open, the arm thrown wide and slow to come back, the shield meeting nothing till then.)
                float weight = impulse.magnitude / Mathf.Max(1f, Mass);
                // (Concentrated, it takes more before its guard is knocked open, and gives less: step 7 of the plan.)
                knockedOpen = weight >= CombatFeel.GuardOpenFrom * GuardHolds;
                // (Heavy weapons: the shield gives further to the blow, by the scene's Combat Feel; whether the guard is
                // knocked open goes by the blow's own weight, as before.)
                float give = Mathf.Min(CombatFeel.ShieldGivePerWeight * weight * heavier / GuardHolds, CombatFeel.MostShieldGive) * Size;
                KnockArmBy(1, impulse, point, knockedOpen ? Mathf.Max(give, CombatFeel.MostShieldGive * Size) : give, knockedOpen ? GuardOpenFrequency : ArmKnockFrequency);
                if (knockedOpen)
                {
                    guardOpenUntil = clock + CombatFeel.GuardOpenFor;
                    GuardsOpened++;
                }
            }
            else if (armRegion == BodyRegion.ArmL || armRegion == BodyRegion.ArmR)
            {
                KnockArm(armRegion == BodyRegion.ArmR ? 0 : 1, impulse * partShare, point);
            }

            if (shield)
            {
                Blocks++;
            }
            else
            {
                Hits++;
            }

            Poise = Mathf.Max(0f, Poise - poiseDamage);
            var region = shield ? BodyRegion.ArmL : AuthorityController.RegionOf(part);
            bool hard = reaction == Combat.Reaction.Stagger || reaction == Combat.Reaction.Flinch;
            Soften(region, shield ? 0.4f : hard ? 0.2f : 0.3f, shield ? 0.4f : hard ? 0.9f : 0.6f);
            if (region != BodyRegion.Torso && !shield)
            {
                Soften(BodyRegion.Torso, 0.6f, 0.5f);
            }

            var struck = Rig.Part(shield ? PartId.HandL : part).body;
            if (Authority != null && struck != null && !struck.isKinematic)
            {
                Vector3 kick = impulse * partShare / Mathf.Max(0.5f, struck.mass);
                struck.AddForce(Vector3.ClampMagnitude(kick, (shield ? MostShieldSpeed : MostPartSpeed) * Size), ForceMode.VelocityChange);
            }

            // (A physical leg: knocked the way of the blow, and let go a moment.)
            if (!shield && struck != null && System.Array.IndexOf(UpperBodyArticulation.LegParts, part) >= 0)
            {
                KnockLeg(part, impulse * partShare / Mathf.Max(0.5f, struck.mass));
            }

            // Driven back: what the struck part doesn't take moves the whole body (the fight's rule), with steps
            // when it is hard enough. The body first takes the blow where it stands (its reaction clip), then
            // the steps carry it back (a push at once would cut the reaction off before it showed).
            // (A blow on the shield drives its bearer back as much harder as the scene's Combat Feel makes the weapons
            // heavy; one on the body comes here with that in it already.)
            Vector3 knock = impulse * (1f - partShare) * KnockScale / Mass * (shield ? heavier : drive);
            knock.y = 0f;
            bool driven = !Anchored && knock.magnitude > DrivenFrom;
            string clipName = shield ? null : ReactionClip(part, impulse.magnitude / Mass, reaction, thrown);
            var clip = clipName != null ? LocoLibrary.Get(clipName) : null;
            string reacted = null;
            // (Heavy enough (the plan "Weight and momentum", step 3: the Large's club on a Medium), it is thrown back
            // stumbling at once, its walk taking the catch steps, no reaction clip to wait out first.)
            bool stumbles = driven && !shield && !Striking && impulse.magnitude * drive / Mathf.Max(1f, Mass) >= CombatFeel.StumbleFrom;
            if (stumbles)
            {
                clip = null;
            }
            if (Striking)
            {
                // Struck in its own strike: as the fight has it, a flinch or a stagger cuts the strike short (it backs
                // out a little and fades into its guard), a lighter blow lets it carry on; either way the blow shows in
                // the body's sway, not in a reaction clip, whose feet would drag those the strike has planted.
                clip = null;
                if ((reaction == Combat.Reaction.Stagger || reaction == Combat.Reaction.Flinch) && Loco.ShotAirborne())
                {
                    // (Struck in the air, its jump can't back out or fade into its guard there: it comes down where it
                    // is, its own fall and landing played on, the weapon arm held off the follow-through, as a jump
                    // stopped by what it struck does (the playtest of 6 November: it drifted down to the floor).)
                    Combo?.Stop();
                    Poser.HoldWeaponArm();
                    Loco.StopShotTravel();
                    reacted = "struck in the air, it comes down";
                }
                else if (reaction == Combat.Reaction.Stagger || reaction == Combat.Reaction.Flinch)
                {
                    Combo?.Stop();
                    Loco.CutShort(StrikeHits.RewindMost, StrikeHits.ReboundSpeed);
                    reacted = "its strike cut short";
                }
                else
                {
                    reacted = "its strike carried on";
                }
            }

            if (clip != null && Loco.Current == Locomotor.Mode.OneShot && Loco.Shot != null && IsReaction(Loco.Shot.Name))
            {
                // (Already reeling from a blow: it goes on, the new blow in its sway. A reaction started over another
                // dragged the foot the first had planted.)
                clip = null;
                reacted = "still reeling";
            }

            if (clip != null)
            {
                Loco.Play(clip);
            }

            reacted ??= clip != null ? clipName.Replace("sns_impact_", "") + " reaction" : shield ? (knockedOpen ? "the guard knocked open" : "the shield knocked back") : "no reaction clip";
            if (stumbles)
            {
                Stumbles++;
                thrownAt = clock;
                Vector3 thrownBy = knock * CombatFeel.StumbleBoost + (pushAt > clock ? pendingKnock : Vector3.zero);
                // (What its legs can catch, the plan "Grounded legs", step 4. How fast the blow would throw it, on top of
                // how fast it is going already: up to what its stance steps after it is caught there; up to what a body
                // can run backward it is thrown back stumbling, its walk taking the catch steps (no faster than the
                // scene's "Most Stumble"); faster than it can run, no step catches it and it goes down.)
                Vector3 going = new Vector3(Loco.Velocity.x, 0f, Loco.Velocity.z);
                float thrownAtSpeed = (thrownBy + going).magnitude, caught = CombatFeel.LegsCatch * Mathf.Sqrt(Size);
                if (CombatFeel.LegsDecide && thrownAtSpeed > caught && !Anchored)
                {
                    ThrownDown++;
                    pendingKnock = Vector3.zero;
                    pushAt = -1f;
                    KnockOver(thrownBy, 0.8f, $"{(swept.Length > 0 ? swept + "; " : "")}thrown at {thrownAtSpeed:0.0} m/s, faster than its legs can catch ({caught:0.0}): down", thrownAtSpeed);
                    return;
                }

                pendingKnock = Vector3.ClampMagnitude(thrownBy, CombatFeel.MostStumble);
                pushAt = clock;
                LastReaction = $"{(swept.Length > 0 ? swept + "; " : "")}thrown back stumbling at {pendingKnock.magnitude:0.0} m/s (the blow's {thrownAtSpeed:0.0})";
                return;
            }

            if (driven)
            {
                // (Already being carried back: the new blow adds to it now.)
                pendingKnock = Vector3.ClampMagnitude(knock + (pushAt > clock ? pendingKnock : Vector3.zero), MostKnock);
                pushAt = clock + (clip != null ? DriveAfter : 0f);
                LastReaction = $"{(swept.Length > 0 ? swept + "; " : "")}{reacted}, then driven back at {pendingKnock.magnitude:0.0} m/s";
                return;
            }

            LastReaction = (swept.Length > 0 ? swept + "; " : "") + reacted;
        }

        private static bool IsReaction(string clip) => clip.StartsWith("sns_impact_") || clip.StartsWith("hit_");

        /// <summary>The side hit clips' blow landing: played at its own speed (seconds), the recovery after it hurried.</summary>
        public const float SideHitLands = 0.4f;

        /// <summary>
        /// A side hit clip's recovery hurried so the whole reaction takes no longer than <see cref="CombatFeel.SideHitTime"/>
        /// (against several attackers a 1.6 s reaction left it open to the next one's blow: the playtest runs of 6 November).
        /// </summary>
        private void HurrySideHit()
        {
            if (Loco.Current != Locomotor.Mode.OneShot || Loco.Shot == null || !Loco.Shot.Name.StartsWith("hit_") || Loco.ShotTime < SideHitLands)
            {
                return;
            }

            float rest = Loco.Shot.Duration - SideHitLands, left = CombatFeel.SideHitTime - SideHitLands;
            Loco.ShotSpeed = left > 0.05f && rest > left ? rest / left : 1f;
        }

        /// <summary>How long a driven body takes the blow where it stands before its steps carry it back (seconds: the reaction clips' peak).</summary>
        public const float DriveAfter = 0.22f;

        private Vector3 pendingKnock;
        private float pushAt = -1f;

        /// <summary>What a blow this hard (impulse per kilogram of the body, m/s) feels like below which it only flinches, and from which it is thrown.</summary>
        public const float LightBelow = 0.2f, HeavyFrom = 0.7f;

        /// <summary>
        /// The reaction clip for a blow: a light flinch for a light one; for a real one, doubled over when it is to the
        /// body, the upper body thrown back when it is to the head or the top of the chest; thrown for a heavy one
        /// or a broken poise, wherever it lands.
        /// </summary>
        /// <summary>Which way a blow throws the body, against where it faces: back (a blow from in front), to its left or right, or forward (from behind).</summary>
        /// <summary>Side hits play the side's clips (headless: <c>NO_SIDE_HITS</c> plays the front ones whatever the side, as before).</summary>
        public static bool SideHits = System.Environment.GetEnvironmentVariable("NO_SIDE_HITS") == null;

        public enum BlowSide
        {
            Back,
            Left,
            Right,
            Forward,
        }

        /// <summary>Within this of straight back (degrees) a blow counts as from in front; past this from it, from behind.</summary>
        public const float FrontWithin = 50f, BehindFrom = 130f;

        public BlowSide BlowSideOf(Vector3 impulse)
        {
            impulse.y = 0f;
            if (impulse.sqrMagnitude < 1e-8f)
            {
                return BlowSide.Back;
            }

            var turn = Quaternion.Euler(0f, Loco.Facing, 0f);
            Vector3 back = turn * Vector3.back;
            float off = Vector3.Angle(back, impulse);
            if (off <= FrontWithin)
            {
                return BlowSide.Back;
            }

            if (off >= BehindFrom)
            {
                return BlowSide.Forward;
            }

            return Vector3.Dot(impulse, turn * Vector3.right) > 0f ? BlowSide.Right : BlowSide.Left;
        }

        private int sideTake;

        /// <summary>
        /// Its reaction clip: thrown back (a blow from in front), the pack's own, held in its guard; thrown sideways or
        /// forward, Renier's hit clips by the side (6 November): to its right is a blow from its left (hit_left_), and so on.
        /// Thrown forward, the blow from behind (hit_back_head), but for a light one, which only bends it.
        /// </summary>
        private string ReactionClip(PartId part, float strength, Combat.Reaction reaction, BlowSide thrown = BlowSide.Back)
        {
            bool high = part == PartId.Head || PartAt(part).y > PartAt(PartId.Chest).y - 0.05f * Size;
            bool heavy = reaction == Combat.Reaction.Stagger || reaction == Combat.Reaction.Killed || strength >= HeavyFrom;
            bool light = strength < LightBelow && reaction == Combat.Reaction.Recoil;
            var region = AuthorityController.RegionOf(part);
            bool arm = region == BodyRegion.ArmL || region == BodyRegion.ArmR;
            if (thrown != BlowSide.Back && SideHits && !arm)
            {
                string side = thrown == BlowSide.Right ? "hit_left_" : thrown == BlowSide.Left ? "hit_right_" : null;
                string name = side == null
                    ? (light ? null : "hit_back_head")
                    : part == PartId.Head ? side + (heavy ? ((sideTake++ & 1) == 0 ? "head_big" : "head_big_b") : "head") : side + "rib";
                if (name == null || LocoLibrary.Get(name) != null)
                {
                    return name;
                }
            }

            if (thrown == BlowSide.Forward && light)
            {
                return null;
            }

            if (heavy)
            {
                return "sns_impact_heavy";
            }

            // (An arm struck jerks the body less than a blow to it.)
            if (light || arm)
            {
                return "sns_impact_light";
            }

            return high ? "sns_impact_heavy" : "sns_impact_body";
        }

        // ---- The pose's reaction to a blow (the fight's recoil and arm knock, UnitCombat) -------------------

        private Vector3 lean, leanV, headSnap, headV;

        /// <summary>How far the blows throw the upper body now (radians), for the logs.</summary>
        public float Lean => lean.magnitude;
        private readonly Vector3[] knock = new Vector3[2], knockV = new Vector3[2], knockTurn = new Vector3[2], knockTurnV = new Vector3[2];

        /// <summary>
        /// How much harder the dummy reacts than the fight's rules have a unit react (the lab's first playtest:
        /// at the fight's own, a Medium's blow on a Medium leaned it 7 to 17 degrees and pushed it 0.2 m/s, and
        /// read as nothing). The fight's own are tuned when the walk joins it (step 8).
        /// </summary>
        public float ReactScale = 1.8f;

        /// <summary>How hard what the struck part doesn't take drives the body back (per m/s of it; the scene's Combat Feel sets it).</summary>
        public float KnockScale => CombatFeel.DriveBack;

        /// <summary>The recoil's swing back (Hz), its damping and the head's; the most it throws the upper body and the head (radians).</summary>
        public const float ReactFrequency = 1.6f, ReactDamping = 0.55f, HeadFrequency = 2.4f, MostLean = 0.6f, MostHeadSnap = 0.7f, Kick = 1.8f;

        /// <summary>A struck arm's knock aside: its return (Hz, critically damped), the most it moves the hand (m at a Medium's size) and turns it (radians).</summary>
        public const float ArmKnockFrequency = 2.5f, MostArmKnock = 0.12f, MostArmTurn = 0.44f;

        private static float Tanh(float x)
        {
            float e = Mathf.Exp(-2f * Mathf.Abs(x));
            return Mathf.Sign(x) * (1f - e) / (1f + e);
        }

        /// <summary>The upper body turned about the hips by the blow's lever; the head snapped by one to it.</summary>
        private void Recoil(Vector3 impulse, Vector3 point, PartId part, float scale)
        {
            float j = impulse.magnitude * scale;
            if (j < 1e-4f)
            {
                return;
            }

            float leg = Loco.Leg;
            Vector3 dir = impulse / impulse.magnitude;
            float w = 2f * Mathf.PI * ReactFrequency;
            Vector3 lever = point - Anim.Part(PartId.Pelvis).position;
            Vector3 axis = Vector3.Cross(lever, dir);
            if (axis.sqrMagnitude > 1e-8f)
            {
                float leverage = Mathf.Clamp(lever.magnitude / leg, 0.2f, 1.4f);
                float amount = MostLean * Tanh(1.5f * j * leverage / (Mass * leg)) * CombatFeel.LeanScale;
                leanV += axis.normalized * (amount * Kick * w);
            }

            if (part == PartId.Head)
            {
                Vector3 headLever = point - Anim.Part(PartId.Head).position + Vector3.up * (0.1f * leg);
                Vector3 headAxis = Vector3.Cross(headLever, dir);
                if (headAxis.sqrMagnitude > 1e-8f)
                {
                    float snap = MostHeadSnap * Tanh(6f * j / (Mass * leg)) * CombatFeel.HeadSnapScale;
                    headV += headAxis.normalized * (snap * Kick * 2f * Mathf.PI * HeadFrequency);
                }
            }
        }

        /// <summary>
        /// The sword met by a blow: parrying, the knock goes into the parry (its hand driven back along the blow, coming
        /// back at the hand's pace), so the sword stays the parry's; put on the pose after it, the knock moved the blade
        /// off the line the next blow comes on. Otherwise the arm is knocked as any arm is.
        /// </summary>
        private void KnockSword(Vector3 impulse, Vector3 point)
        {
            if (stance != Stances.Sword || Parry == null)
            {
                KnockArm(0, impulse, point);
                return;
            }

            float j = impulse.magnitude;
            if (j < 1e-4f)
            {
                return;
            }

            float armMass = 0f;
            foreach (var id in new[] { PartId.UpperArmR, PartId.ForearmR, PartId.HandR })
            {
                var body = Rig.Part(id).body;
                armMass += body != null ? body.mass : 0f;
            }

            // (Heavier blows drive it further, by their weight (the plan "Weight and momentum", step 4). Not less than it
            // was: knocked less after a glance, the sword was off the line of the combo's next blow, 7 of 14 lost.)
            float hard = Tanh(j / (Mathf.Max(1f, armMass + 3f) * 3f * Mathf.Sqrt(Size)));
            Parry.Knock(impulse / j * Mathf.Max(MostArmKnock * Size * hard, ParryGive(j / Mathf.Max(1f, Mass))));
        }

        /// <summary>How far a parry gives under a blow of this weight (m/s): metres, at its size (the scene's Combat Feel).</summary>
        public float ParryGive(float weight) => Mathf.Min(CombatFeel.ParryGivePerWeight * weight, CombatFeel.MostParryGive) * Size;

        /// <summary>The heaviest blow (m/s of weight) its square parry holds, by its skill; heavier beats it (the scene's Combat Feel).</summary>
        public float ParryHolds => Mathf.Lerp(CombatFeel.ParryHolds, CombatFeel.ExpertParryHolds, ParrySkillNow) * GuardHolds;

        /// <summary>
        /// A blow of its own stopped on a shield or a blade jars it (the plan "Weight and momentum", step 4): its upper
        /// body rocked back by <paramref name="impulse"/> (world, N·s, back along its blow) at the weapon hand.
        /// </summary>
        public void Jar(Vector3 impulse, Vector3 point)
        {
            if (Fallen || CombatFeel.JarScale <= 0f)
            {
                return;
            }

            Recoil(impulse, point, PartId.HandR, CombatFeel.JarScale * ReactScale);
        }

        /// <summary>Its shield guard knocked open by a heavy blow and not back yet: the shield moves to meet nothing.</summary>
        public bool GuardOpen => clock < guardOpenUntil;

        private float guardOpenUntil = -1f;

        /// <summary>Guards knocked open (for the logs).</summary>
        public int GuardsOpened;

        /// <summary>How quickly a shield arm knocked open comes back (Hz, critically damped; a knocked arm's is <see cref="ArmKnockFrequency"/>).</summary>
        public const float GuardOpenFrequency = 1.1f;

        private readonly float[] armKnockHz = { ArmKnockFrequency, ArmKnockFrequency };

        /// <summary>An arm (the shield arm) knocked back along <paramref name="impulse"/> by <paramref name="give"/> (metres) at its peak and turned with it, coming back at <paramref name="hz"/> (critically damped).</summary>
        private void KnockArmBy(int a, Vector3 impulse, Vector3 point, float give, float hz)
        {
            float j = impulse.magnitude;
            if (j < 1e-4f || give <= 0f)
            {
                return;
            }

            // (A critically damped spring kicked at v peaks at v / (e w).)
            armKnockHz[a] = hz;
            float kick = (float)System.Math.E * 2f * Mathf.PI * hz;
            Vector3 dir = impulse / j;
            knockV[a] += dir * (give * kick);
            Vector3 lever = point - Anim.Part(a == 0 ? PartId.HandR : PartId.HandL).position;
            Vector3 axis = Vector3.Cross(lever, dir);
            if (axis.sqrMagnitude > 1e-8f)
            {
                knockTurnV[a] += axis.normalized * (MostArmTurn * Mathf.Min(1.5f, give / (MostArmKnock * Size)) * kick);
            }
        }

        /// <summary>A struck arm (or the shield it holds) knocked aside and turned, then back.</summary>
        private void KnockArm(int a, Vector3 impulse, Vector3 point)
        {
            armKnockHz[a] = ArmKnockFrequency;
            float j = impulse.magnitude;
            if (j < 1e-4f)
            {
                return;
            }

            float armMass = 0f;
            foreach (var id in a == 0 ? new[] { PartId.UpperArmR, PartId.ForearmR, PartId.HandR } : new[] { PartId.UpperArmL, PartId.ForearmL, PartId.HandL })
            {
                var body = Rig.Part(id).body;
                armMass += body != null ? body.mass : 0f;
            }

            float hard = Tanh(j / (Mathf.Max(1f, armMass + 3f) * 3f * Mathf.Sqrt(Size)));
            float kick = (float)System.Math.E * 2f * Mathf.PI * ArmKnockFrequency;
            Vector3 dir = impulse / j;
            knockV[a] += dir * (MostArmKnock * Size * hard * kick);
            Vector3 lever = point - Anim.Part(a == 0 ? PartId.HandR : PartId.HandL).position;
            Vector3 axis = Vector3.Cross(lever, dir);
            if (axis.sqrMagnitude > 1e-8f)
            {
                knockTurnV[a] += axis.normalized * (MostArmTurn * hard * kick);
            }
        }

        private static void Spring(ref Vector3 x, ref Vector3 v, float hz, float damping, float dt)
        {
            float w = 2f * Mathf.PI * hz;
            v += (-w * w * x - 2f * damping * w * v) * dt;
            x += v * dt;
        }

        /// <summary>A critically damped spring toward <paramref name="to"/> (its rate <paramref name="w"/>, 1/s).</summary>
        private static void CriticalSpring(ref Vector3 x, ref Vector3 v, Vector3 to, float w, float dt)
        {
            Vector3 off = x - to;
            CriticalSpring(ref off, ref v, w, dt);
            x = to + off;
        }

        private static void CriticalSpring(ref Vector3 x, ref Vector3 v, float w, float dt)
        {
            float decay = Mathf.Exp(-w * dt);
            Vector3 c = v + w * x;
            Vector3 nx = (x + c * dt) * decay;
            v = (v - c * (w * dt)) * decay;
            x = nx;
        }

        private readonly Vector3[] legTurn = new Vector3[2], legTurnV = new Vector3[2];

        /// <summary>The most a blow turns a struck leg at the hip (radians), and how quickly it comes back (Hz, critically damped).</summary>
        public const float MostLegTurn = 0.5f, LegKnockFrequency = 2.2f;

        /// <summary>A struck leg knocked along the blow: turned at the hip about the blow's lever, then back.</summary>
        private void KnockLeg(PartId part, Vector3 impulse, Vector3 point)
        {
            float j = impulse.magnitude;
            if (j < 1e-4f)
            {
                return;
            }

            int leg = part == PartId.ThighL || part == PartId.ShinL || part == PartId.FootL ? 0 : 1;
            var hip = Anim.Part(leg == 0 ? PartId.ThighL : PartId.ThighR).position;
            Vector3 axis = Vector3.Cross(point - hip, impulse / j);
            if (axis.sqrMagnitude < 1e-8f)
            {
                return;
            }

            float hard = Tanh(j / (12f * Size * Size));
            float kick = (float)System.Math.E * 2f * Mathf.PI * LegKnockFrequency;
            legTurnV[leg] += axis.normalized * (MostLegTurn * hard * kick);
        }

        /// <summary>The reaction springs stepped and put on the pose: the upper body leans away, the head snaps, a struck arm is knocked aside.</summary>
        private void React(float dt)
        {
            Spring(ref lean, ref leanV, ReactFrequency, ReactDamping, dt);
            Spring(ref headSnap, ref headV, HeadFrequency, ReactDamping, dt);
            lean = Vector3.ClampMagnitude(lean, MostLean);
            headSnap = Vector3.ClampMagnitude(headSnap, MostHeadSnap);
            for (int a = 0; a < 2; a++)
            {
                float wk = 2f * Mathf.PI * armKnockHz[a];
                CriticalSpring(ref knock[a], ref knockV[a], wk, dt);
                CriticalSpring(ref knockTurn[a], ref knockTurnV[a], wk, dt);
            }

            float wl = 2f * Mathf.PI * LegKnockFrequency;
            for (int l = 0; l < 2; l++)
            {
                CriticalSpring(ref legTurn[l], ref legTurnV[l], wl, dt);
                legTurn[l] = Vector3.ClampMagnitude(legTurn[l], MostLegTurn);
                if (legTurn[l].sqrMagnitude > 1e-8f)
                {
                    var thigh = Anim.Part(l == 0 ? PartId.ThighL : PartId.ThighR);
                    thigh.rotation = Quaternion.AngleAxis(legTurn[l].magnitude * Mathf.Rad2Deg, legTurn[l].normalized) * thigh.rotation;
                }
            }

            float angle = lean.magnitude * Mathf.Rad2Deg;
            if (angle > 0.01f)
            {
                // (The spine takes some of the turn, the chest the rest; the hips and legs stay as they stand.)
                Vector3 axis = lean.normalized;
                var spine = Anim.Part(PartId.Spine);
                var chest = Anim.Part(PartId.Chest);
                spine.rotation = Quaternion.AngleAxis(0.45f * angle, axis) * spine.rotation;
                chest.rotation = Quaternion.AngleAxis(0.55f * angle, axis) * chest.rotation;
            }

            if (headSnap.sqrMagnitude > 1e-8f)
            {
                var head = Anim.Part(PartId.Head);
                head.rotation = Quaternion.AngleAxis(headSnap.magnitude * Mathf.Rad2Deg, headSnap.normalized) * head.rotation;
            }

            Bend(dt);

            for (int a = 0; a < 2; a++)
            {
                if (knock[a].sqrMagnitude < 1e-8f && knockTurn[a].sqrMagnitude < 1e-8f)
                {
                    continue;
                }

                var upper = Anim.Part(a == 0 ? PartId.UpperArmR : PartId.UpperArmL);
                var fore = Anim.Part(a == 0 ? PartId.ForearmR : PartId.ForearmL);
                var hand = Anim.Part(a == 0 ? PartId.HandR : PartId.HandL);
                var turnHand = knockTurn[a].sqrMagnitude > 1e-8f ? Quaternion.AngleAxis(knockTurn[a].magnitude * Mathf.Rad2Deg, knockTurn[a].normalized) : Quaternion.identity;
                var keep = turnHand * hand.rotation;
                Vector3 pole = fore.position - 0.5f * (upper.position + hand.position);
                TwoBoneIK.Solve(upper, fore, hand, hand.position + knock[a], pole);
                hand.rotation = keep;
            }
        }

        // ---- The impact's bend (the playtest: the blow's force felt through the body) ---------------------------

        /// <summary>
        /// The bend a blow puts in the body: every part pushed along the blow, the part struck the most and the rest less
        /// the further they are from where it landed, so the body bends round the blow; for a moment, then it
        /// straightens. A pose only, no physics: how far by the blow's weight (m per m/s of it, at most, metres at a
        /// Medium's size), out to how far from the blow it reaches, how soon it peaks (seconds); the scene's Combat Feel
        /// sets them (<see cref="CombatFeel"/>).
        /// </summary>
        private static float BendPerWeight => CombatFeel.BendPerWeight;

        private static float MostBend => CombatFeel.MostBend;

        private static float BendReach => CombatFeel.BendReach;

        private static float BendPeak => CombatFeel.BendPeak;

        private struct Impact
        {
            public Vector3 At, Way;
            public float X, V;
        }

        private readonly List<Impact> impacts = new List<Impact>(4);

        /// <summary>How far the last blow's bend went at its peak (metres), for the logs.</summary>
        public float LastBend { get; private set; }

        private void AddImpact(Vector3 point, Vector3 impulse, float scale = 1f)
        {
            float weight = impulse.magnitude / Mathf.Max(1f, Mass);
            float peak = Mathf.Min(BendPerWeight * weight * scale, MostBend * Mathf.Max(1f, scale)) * Size;
            if (peak < 0.005f)
            {
                return;
            }

            Vector3 way = impulse;
            way.y *= 0.3f;
            way = way.sqrMagnitude > 1e-8f ? way.normalized : -(Quaternion.Euler(0f, Loco.Facing, 0f) * Vector3.forward);
            if (impacts.Count >= 4)
            {
                impacts.RemoveAt(0);
            }

            // (Out to its peak at BendPeak and back: a critically damped spring set off at the speed that peaks there.)
            float w = 1f / BendPeak;
            impacts.Add(new Impact { At = point, Way = way, X = 0f, V = peak * w * Mathf.Exp(1f) });
            LastBend = peak;
        }

        /// <summary>Where the bend puts a point of the body (world): every blow's push there, by how near it is to where the blow landed.</summary>
        private Vector3 BendAt(Vector3 point)
        {
            Vector3 d = Vector3.zero;
            float reach = BendReach * Size;
            foreach (var i in impacts)
            {
                d += i.Way * (i.X * Mathf.Max(0f, 1f - Vector3.Distance(point, i.At) / reach));
            }

            return d;
        }

        /// <summary>
        /// The bend on the posed body: the hips moved by theirs with the feet kept where they stand (the knees taking it),
        /// then the spine, the chest, the head and the arms each turned so its end goes where its share puts it.
        /// </summary>
        private void Bend(float dt)
        {
            if (impacts.Count == 0)
            {
                return;
            }

            float w = 1f / BendPeak;
            bool any = false;
            for (int k = impacts.Count - 1; k >= 0; k--)
            {
                var i = impacts[k];
                float decay = Mathf.Exp(-w * dt);
                float c = i.V + w * i.X;
                float nx = (i.X + c * dt) * decay;
                i.V = (i.V - c * (w * dt)) * decay;
                i.X = nx;
                if (i.X < 1e-4f && i.V <= 0f)
                {
                    impacts.RemoveAt(k);
                    continue;
                }

                impacts[k] = i;
                any = true;
            }

            if (!any)
            {
                return;
            }

            // (Where each joint was, posed, and where the bend wants it: the bend is of the pose as it stood.)
            var pelvis = Anim.Part(PartId.Pelvis);
            Vector3[] feet = { Anim.Part(PartId.FootL).position, Anim.Part(PartId.FootR).position };
            Quaternion[] feetTurn = { Anim.Part(PartId.FootL).rotation, Anim.Part(PartId.FootR).rotation };
            Vector3[] knees = { Anim.Part(PartId.ShinL).position, Anim.Part(PartId.ShinR).position };
            Vector3 headTipWas = Anim.Part(PartId.Head).position + Anim.Part(PartId.Head).up * (0.2f * Size);
            var chain = new[] { PartId.Spine, PartId.Chest, PartId.Head, PartId.UpperArmL, PartId.ForearmL, PartId.HandL, PartId.UpperArmR, PartId.ForearmR, PartId.HandR };
            var was = new Vector3[chain.Length];
            for (int k = 0; k < chain.Length; k++)
            {
                was[k] = Anim.Part(chain[k]).position;
            }

            Vector3 Want(int k) => was[k] + BendAt(was[k]);

            // The hips, as far as both legs still reach their feet from there (a leg near straight dragged its planted
            // foot after them), and the legs under them to where the feet stand.
            Vector3 hipShift = BendAt(pelvis.position);
            for (int l = 0; l < 2; l++)
            {
                var thigh = Anim.Part(l == 0 ? PartId.ThighL : PartId.ThighR);
                var shin = Anim.Part(l == 0 ? PartId.ShinL : PartId.ShinR);
                float leg = 0.98f * (Vector3.Distance(thigh.position, shin.position) + Vector3.Distance(shin.position, feet[l]));
                float now = Vector3.Distance(thigh.position, feet[l]);
                for (int k = 0; k < 6 && Vector3.Distance(thigh.position + hipShift, feet[l]) > Mathf.Max(leg, now); k++)
                {
                    hipShift *= 0.5f;
                }
            }

            pelvis.position += hipShift;
            for (int l = 0; l < 2; l++)
            {
                var thigh = Anim.Part(l == 0 ? PartId.ThighL : PartId.ThighR);
                var shin = Anim.Part(l == 0 ? PartId.ShinL : PartId.ShinR);
                var foot = Anim.Part(l == 0 ? PartId.FootL : PartId.FootR);
                Vector3 knee = knees[l] + BendAt(knees[l]);
                Vector3 pole = knee - 0.5f * (thigh.position + feet[l]);
                TwoBoneIK.Solve(thigh, shin, foot, feet[l], pole.sqrMagnitude > 1e-8f ? pole : Quaternion.Euler(0f, Loco.Facing, 0f) * Vector3.forward);
                foot.rotation = feetTurn[l];
            }

            // Each bone turned about its own joint so the joint it carries goes where the bend wants it.
            void Aim(PartId bone, Vector3 end, Vector3 want)
            {
                var t = Anim.Part(bone);
                Vector3 from = end - t.position, to = want - t.position;
                if (from.sqrMagnitude > 1e-8f && to.sqrMagnitude > 1e-8f)
                {
                    t.rotation = Quaternion.FromToRotation(from, to) * t.rotation;
                }
            }

            Aim(PartId.Spine, Anim.Part(PartId.Chest).position, Want(1));
            Aim(PartId.Chest, Anim.Part(PartId.Head).position, Want(2));
            var head = Anim.Part(PartId.Head);
            Aim(PartId.Head, head.position + head.up * (0.2f * Size), headTipWas + BendAt(headTipWas));
            Aim(PartId.UpperArmL, Anim.Part(PartId.ForearmL).position, Want(4));
            Aim(PartId.ForearmL, Anim.Part(PartId.HandL).position, Want(5));
            Aim(PartId.UpperArmR, Anim.Part(PartId.ForearmR).position, Want(7));
            Aim(PartId.ForearmR, Anim.Part(PartId.HandR).position, Want(8));
        }

        /// <summary>The body parts where the animation rig has them (kinematic).</summary>
        private void Carry()
        {
            foreach (var id in PartInfo.All)
            {
                var part = Rig.Part(id);
                var target = Anim.Part(id);
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

        /// <summary>Its get-up's root, which lives beside it (a fight unit that owns its own object goes; this goes with it).</summary>
        public void DestroyGetUpRoot()
        {
            if (getUpRoot != null)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(getUpRoot.gameObject);
                }
                else
                {
                    Object.DestroyImmediate(getUpRoot.gameObject);
                }
            }
        }

        /// <summary>
        /// Its unit is going, or has gone: it is no longer told of the engine's steps, and its physical body and its
        /// get-up's root go. Whoever destroys its unit calls this: left told, it reaches for a body that is not there
        /// (<see cref="PhysicsSteps.Run"/>).
        /// </summary>
        public void Release()
        {
            PhysicsSteps.BeforeSubstep -= BeforeSubstep;
            PhysicsSteps.AfterStep -= AfterStep;
            if (Application.isPlaying)
            {
                Upper?.Destroy();
            }

            Upper = null;
            DestroyGetUpRoot();
        }

        public void Destroy()
        {
            PhysicsSteps.BeforeSubstep -= BeforeSubstep;
            PhysicsSteps.AfterStep -= AfterStep;
            Upper?.Destroy();
            Upper = null;
            if (Unit != null)
            {
                Object.Destroy(Unit);
            }

            // (And its get-up's root, which lives beside it.)
            if (getUpRoot != null)
            {
                Object.Destroy(getUpRoot.gameObject);
            }
        }
    }
}
