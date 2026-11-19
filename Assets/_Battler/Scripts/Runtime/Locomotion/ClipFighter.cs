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
        /// <summary>
        /// Two fighters are kept apart while one of them is off his feet too (Combat Feel "Fighters Keep Apart Off Their
        /// Feet Too"): one on his feet is walked off a body that is down or getting up near him, and one lying does not
        /// start to get up while another stands or rises where he would come up. Off: as before, nothing keeps them apart
        /// until both stand; two who fell together got up inside each other and their bodies locked (his log of the
        /// 19th: 0.32 to 0.85 m apart, root to root, for seven seconds).
        /// </summary>
        public static bool KeepsApartOffHisFeet = true;

        /// <summary>Set each step by whoever keeps bodies apart: another stands or rises within the least gap of where he lies.</summary>
        public bool NoRoomToRise;

        /// <summary>
        /// The room a body needs to get up in (metres, root to root, at a Medium's size; the gap two keep on their feet
        /// if that is more); nearer than this share of it one on his feet is moved off as well as stepping off, no
        /// faster than this (m/s at a Medium's size); and how long at most a lying man waits for room (seconds).
        /// Measured from where each body is (<see cref="BodyAt"/>).
        /// </summary>
        public const float RoomToRise = 1.1f, RoomDeepIn = 0.6f, RoomSpeed = 1.5f, WaitsForRoom = 3f;

        /// <summary>
        /// Where his body is on the ground, for the room it needs: on his feet his root; down or getting up his hips (a
        /// get-up's root is where he will stand, up to half a metre from where he lies: by their roots two who lay
        /// 0.96 m apart were 0.54 m apart a moment after the first began to rise).
        /// </summary>
        public Vector3 BodyAt
        {
            get
            {
                if (Footing == Footings.Standing || Rig == null)
                {
                    return Loco.Position;
                }

                Vector3 hips = Rig.Part(PartId.Pelvis).body.position;
                return new Vector3(hips.x, Loco.Position.y, hips.z);
            }
        }

        /// <summary>For the reports: how far bodies on their feet were moved off bodies that were down (metres, all fighters), and how often a lying man waited for room.</summary>
        public static float RoomMoved;
        public static int RoomWaits;
        private bool waitedForRoom;

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
        public bool ShieldArmHeld => interceptWeight > 0.05f || blockWeight > 0.05f || Bashing;

        /// <summary>The shield is doing something of its own (a press of it, a bash): the cover lets the arm go for it.</summary>
        public bool ShieldLeaves;

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

        /// <summary>A body that settles where it lies (<see cref="FallenBodiesSettle"/>) is down this long before it gets up, give or take this much from one fall to the next (seconds): as long as bodies were down on average before they settled (1.4 s in Unity over 56 falls).</summary>
        public const float SettledDown = 1.4f, SettledDownVaries = 0.3f;

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
        /// <summary>
        /// What a body holds is solid while it lies as a ragdoll (the plan "Controlled falls", step 11): a body with
        /// a limb cut off, alive or dead, falls as the rig's own parts do, and its sword and its shield are stopped
        /// by the ground and by other bodies, and lie there with it (by nothing of its own: a blade is not stopped
        /// by its bearer). Off: what a ragdoll held was stopped by nothing, as it is in a standing hand, where it only
        /// says what it touches: a sword hung from a dead hand straight down through the ground.
        /// </summary>
        public static bool RagdollsHoldSolidThings = true;

        private bool heldSolid;

        /// <summary>What its hands hold made solid (it lies as a ragdoll), or only saying what it touches (as it stands, and in its one body, which has solids of its own for them).</summary>
        private void HeldSolid(bool solid)
        {
            if (solid == heldSolid)
            {
                return;
            }

            heldSolid = solid;
            foreach (var (hand, collider, mass, blade) in Solids)
            {
                if (collider != null)
                {
                    collider.isTrigger = !solid;
                }
            }
        }

        private readonly List<(Collider arm, Collider body, bool left)> ragdollArmsToPart = new List<(Collider, Collider, bool)>();
        private readonly List<(Collider arm, Collider body)> ragdollArmsStopped = new List<(Collider, Collider)>();
        private static readonly PartId[] LowerArms = { PartId.ForearmL, PartId.HandL, PartId.ForearmR, PartId.HandR };

        /// <summary>
        /// A ragdoll's arms and its shield are stopped by its own trunk, head and thighs, as a body's are that falls
        /// in its one body (<see cref="UpperBodyArticulation.ArmsStoppedByBody"/>): none of the rig's parts stop each
        /// other, and a ragdoll's arm lay in its own body for as long as it lay. Each arm from when the whole of it
        /// is out of the body: one that is in it is not thrown out.
        /// </summary>
        private void RagdollArmsToStop()
        {
            RagdollArmsPass();
            if (!UpperBodyArticulation.ArmsStoppedByBody)
            {
                return;
            }

            void Against(Collider arm, bool left)
            {
                foreach (var id in TrunkSolids)
                {
                    var body = Rig.Part(id);
                    if (arm != null && body != null && body.partCollider != null && !Rig.IsSevered(id))
                    {
                        ragdollArmsToPart.Add((arm, body.partCollider, left));
                    }
                }
            }

            foreach (var id in LowerArms)
            {
                var part = Rig.Part(id);
                if (part != null && part.partCollider != null && !Rig.IsSevered(id))
                {
                    Against(part.partCollider, id == PartId.ForearmL || id == PartId.HandL);
                }
            }

            // (Its shield, when it is solid: what a hand holds that is no blade.)
            if (heldSolid)
            {
                foreach (var (hand, collider, mass, blade) in Solids)
                {
                    if (!blade && collider != null && !Rig.IsSevered(hand))
                    {
                        Against(collider, hand == PartId.HandL);
                    }
                }
            }
        }

        /// <summary>A ragdoll's arm that is wholly out of its body is stopped by it from now.</summary>
        private void StepRagdollArms()
        {
            if (ragdollArmsToPart.Count == 0)
            {
                return;
            }

            for (int side = 0; side < 2; side++)
            {
                bool any = false, clear = true;
                foreach (var (arm, body, left) in ragdollArmsToPart)
                {
                    if (left != (side == 0) || arm == null || body == null)
                    {
                        continue;
                    }

                    any = true;
                    if (Physics.ComputePenetration(arm, arm.transform.position, arm.transform.rotation, body, body.transform.position, body.transform.rotation, out _, out float depth) && depth > 0f)
                    {
                        clear = false;
                        break;
                    }
                }

                if (!any || !clear)
                {
                    continue;
                }

                for (int i = ragdollArmsToPart.Count - 1; i >= 0; i--)
                {
                    var (arm, body, left) = ragdollArmsToPart[i];
                    if (left != (side == 0))
                    {
                        continue;
                    }

                    if (arm != null && body != null)
                    {
                        Physics.IgnoreCollision(arm, body, false);
                        ragdollArmsStopped.Add((arm, body));
                    }

                    ragdollArmsToPart.RemoveAt(i);
                }
            }
        }

        /// <summary>Getting up, or built again: none of its parts stop each other, as before it fell.</summary>
        private void RagdollArmsPass()
        {
            foreach (var (arm, body) in ragdollArmsStopped)
            {
                if (arm != null && body != null)
                {
                    Physics.IgnoreCollision(arm, body, true);
                }
            }

            ragdollArmsStopped.Clear();
            ragdollArmsToPart.Clear();
        }

        private void FallInBody(int leg, Vector3 way, float knock, float thrown)
        {
            RagdollArmsPass();
            HeldSolid(false);
            fallBody = true;
            OneBodyFallen++;
            landedAt = slowAt = touchedAt = -1f;
            handFreeUntil[0] = handFreeUntil[1] = 0f;
            // (Thrown off its feet, not knocked over them: it curls up as it flies.)
            tucks = ThrownBodiesTuck && !Dead && !catching && thrown >= TuckFrom * Mathf.Sqrt(Size);
            tucked = 0f;
            Tucked += tucks ? 1 : 0;
            Upper.Tone = FallMuscle;
            // (The blow that felled it: most of its force is taken back. The fall has its own speed.)
            Upper.EaseStrike(FelledHitForce);
            Upper.LetGo(Size);
            if (tucks)
            {
                // (Its chin held down as hard as a neck can: let whip back, the head met the ground at 3 to 6 m/s a
                // moment after the back did.)
                Upper.NeckHas = TuckedNeck;
            }

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
                // (Knocked over, its legs are left behind and it goes over them. Thrown off its feet, the whole of it
                // goes, and a little up: sent by its trunk alone it turned over backward in the air, in a third of a
                // second, and came down on its head.)
                float speed = swept ? SweptLegSpeed * (id == PartId.ThighL || id == PartId.ThighR ? 0.5f : 1f)
                    : AuthorityController.RegionOf(id) == BodyRegion.Legs && !tucks ? 0.2f * ThrownBodySpeed : ThrownBodySpeed;
                Upper.Send(id, (tucks ? way + Vector3.up * ThrownUp : way) * (speed * k * harder * Mathf.Clamp(knock / 0.5f, 0.6f, 1.6f)));
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
            // (Its feet are held where they stood until it has gone over far enough to take them with it. One thrown
            // off its feet has them with it from the start.)
            if (Upper.FeetHeld && (tucks || footingTime > FeetHeldFor || Vector3.Angle(hips.up, Vector3.up) > FeetHeldTo || landedAt >= 0f))
            {
                Upper.LetFeetGo();
            }

            // (Down and no longer going anywhere: from then it settles, and a tucked body comes out of its tuck.)
            if (landedAt >= 0f && slowAt < 0f && Upper.SpeedOf(PartId.Pelvis).magnitude < SlowBelow * Mathf.Sqrt(Size))
            {
                slowAt = footingTime;
            }

            // (Tucked until it has been on the ground a moment: it tumbles once in its tuck, then opens out, and its
            // hands and feet stop it. Held longer, it slid on in a ball for five metres.)
            if (touchedAt < 0f && Mathf.Min(hips.position.y, Upper.Link(PartId.Chest).transform.position.y) - groundLevel < 0.32f * Size)
            {
                touchedAt = footingTime;
            }

            if (gathering)
            {
                // (Gathering: the pose his muscles draw him to is the get-up's start, with the strength coming in over
                // a moment. He is still the falling body: only his joints are drawn, each from the part it sits in, so
                // his trunk lies as it lies; and the ground, his own body and his shield are solid to his limbs.)
                float gathered = Mathf.Clamp01(gatherTime / GatherEasesIn);
                Upper.Tone = Mathf.Lerp(Upper.Tone, GatherMuscle * FallMuscle, gathered * gathered * (3f - 2f * gathered));
                getUp.PoseAt(0f);
                if (stance != Stances.Open && GetUp.SwordHandStays)
                {
                    // (His sword hand stays where it lies through a get-up: his sword arm is not gathered. Drawn to the
                    // start with the rest, the hand was then pinned where it had been asked to and not where it was,
                    // and thrown there at up to 43 m/s.)
                    fallPosed.Capture(Anim);
                    fallAsIs.Capture(Rig);
                    fallPosed.MixToward(fallAsIs, id => id == PartId.UpperArmR || id == PartId.ForearmR || id == PartId.HandR ? 1f : 0f);
                    fallPosed.Apply(Anim, false);
                }

                // (And his trunk is turned toward how the start has it, softly: what a man does by pushing on the
                // ground. Without it a gather from his side ran out of time with a hand or foot still 30 cm off.)
                Upper.TurnHips(GatherTurns > 0f ? Anim.Part(PartId.Pelvis).rotation : (Quaternion?)null, GatherTurnQuick, gathered * GatherTurns * Mass * 9.81f);
                Upper.BeginStep(dt);
                return;
            }

            bool holdsTuck = tucks && !Dead && slowAt < 0f && footingTime < TuckAtMost && (touchedAt < 0f || footingTime - touchedAt < TuckAfterLanding);
            tucked = Mathf.MoveTowards(tucked, holdsTuck ? 1f : 0f, dt / (holdsTuck ? TuckIn : TuckOut));
            StepLandedHands();
            float into = Mathf.Clamp01((footingTime - 0.08f) / 0.3f);
            FallPose.Pose(Anim, hips, Upper.SpeedOf(PartId.Pelvis), groundLevel, Size, into * into * (3f - 2f * into), stance != Stances.Open,
                Upper.HandPinned(0) ? Upper.HandPinAt(0) : (Vector3?)null, Upper.HandPinned(1) ? Upper.HandPinAt(1) : (Vector3?)null,
                tucked * tucked * (3f - 2f * tucked), TrunkHalf(), stance == Stances.Shield);
            if (FallenBodiesSettle && slowAt >= 0f && !Dead && !catching)
            {
                // (Down, it stops reaching: the pose it is drawn to becomes the pose it lies in, so its muscles only
                // hold what is and damp it. The head is still held up.)
                float settle = Mathf.Clamp01((footingTime - slowAt - SettlesAfter) / SettlesOver);
                if (settle > 0f)
                {
                    settle = settle * settle * (3f - 2f * settle);
                    fallPosed.Capture(Anim);
                    fallAsIs.Capture(Rig);
                    fallPosed.MixToward(fallAsIs, id => id == PartId.Pelvis || id == PartId.Head ? 0f : settle);
                    fallPosed.Apply(Anim, false);
                }
            }

            Upper.BeginStep(dt);
        }

        /// <summary>
        /// A body that is down settles where it lies: a quarter of a second after its hips are down and it is no
        /// longer going anywhere (<see cref="SlowBelow"/>), over the next 0.4 s, the pose its muscles draw it to
        /// becomes the pose it is in, so they hold and damp it and no longer
        /// reach. Off (as it was): it went on being drawn to a pose made from how it lay, which turned over as the
        /// body did: on its side it rocked back and forth for as long as it was down, its hands sweeping the ground.
        /// </summary>
        public static bool FallenBodiesSettle = true;

        /// <summary>How long after its hips are down a body begins to settle, and how long it takes over it (seconds).</summary>
        public const float SettlesAfter = 0.25f, SettlesOver = 0.4f;

        private readonly PoseBuffer fallPosed = new PoseBuffer(), fallAsIs = new PoseBuffer();
        private readonly PoseBuffer[] bodyPoses = { new PoseBuffer(), new PoseBuffer(), new PoseBuffer(), new PoseBuffer() };
        private readonly float[] bodySteps = new float[4];
        private int bodyAt, bodySeen;
        private bool takenUpMoving, swordHandHeld;

        /// <summary>Held still, his sword hand is held bodily too: the physical hand pinned to where the pose has it and held to its turn. Off: the pose alone holds it still, and the physical hand follows with its own muscles (for measuring one against the other; <c>FIGHT_HANDPIN=0</c>).</summary>
        public static bool SwordHandHeldBodily = true;

        /// <summary>How near the physical sword hand must be to where the pose holds it before it is taken hold of (metres, on a Medium).</summary>
        public const float SwordHandTakenWithin = 0.06f;

        /// <summary>Over how long the hold brings the physical sword hand from where it was taken hold of to where the pose has it (seconds).</summary>
        public const float SwordHandTakenOver = 0.2f;

        private Vector3 swordHandHeldOff;
        private float swordHandHeldFor;

        /// <summary>His sword hand's physical hold let go.</summary>
        private void LetSwordHandGo()
        {
            if (swordHandHeld && Upper != null)
            {
                Upper.PinHand(1, false, Vector3.zero, 1f, 1f);
            }

            swordHandHeld = false;
        }

        /// <summary>
        /// The step a get-up takes him up in is its first step too, when it takes him up as he moves
        /// (<see cref="GetUp.TakesOverAsHeMoves"/>): the pose goes on at once from the body he has, at the speed he
        /// has. Left to the next step, his body was held to the pose he lay in for one step first, which stopped
        /// him dead, and then set off again.
        /// </summary>
        private void FirstStepOfGetUp(float dt)
        {
            if (!takenUpMoving || Footing != Footings.GettingUp || dt <= 0f)
            {
                return;
            }

            takenUpMoving = false;
            footingTime = dt;
            StepGettingUp(dt, true);
        }

        /// <summary>
        /// A body thrown off its feet tucks (the plan "Controlled falls", step 9): thrown faster than
        /// <see cref="TuckFrom"/>, it curls up as it flies (the back rounded, the chin down, the knees drawn up, the
        /// arms in before the chest; an arm that holds a weapon stays out with it), its feet with it from the start,
        /// and holds the tuck while it tumbles: the physics leads. Down and slow, it comes out of it and settles as
        /// any body does, and stays down longer. Off: thrown, it fell as one knocked over does, reaching for the
        /// ground with arms that were not going to stop it.
        /// </summary>
        public static bool ThrownBodiesTuck = true;

        /// <summary>
        /// Thrown this fast and more it tucks (m/s at a Medium's size: twice what its legs catch); the tuck comes in
        /// over this long and out over this long (seconds), and is held this long at most, and this long after its
        /// trunk or hips first come to the ground; a body whose hips are
        /// down and going slower than this is no longer going anywhere (m/s at a Medium's size); and one that was
        /// tucked stays down this much longer (seconds), and may be down this much longer than any other before it
        /// is made to get up still moving (seconds). <see cref="ThrownUp"/>: how much of the speed it is thrown
        /// with is upward as well.
        /// </summary>
        public const float ThrownUp = 0.3f;

        /// <summary>Tucked: what its neck has of a person's strength, to hold the chin down.</summary>
        public static float TuckedNeck = 3f;

        public const float TuckFrom = 9f, TuckIn = 0.15f, TuckOut = 0.35f, TuckAtMost = 1.6f, TuckAfterLanding = 0.45f, SlowBelow = 1.5f, ThrownDownLonger = 0.5f, ThrownDownMost = 1.5f;

        /// <summary>Falls it tucked in, for the reports.</summary>
        public int Tucked;

        private bool tucks;
        private float tucked, slowAt = -1f, touchedAt = -1f;

        /// <summary>
        /// A hand that lands in a fall stays where it landed (the plan "Controlled falls", steps 5 to 7): held there
        /// with a limited force, as a planted foot is, while the body comes down over it and the arm bends under
        /// it. A body still sliding drags it, against that force: its hands brake it. Off: a hand that reached the ground skated over it (two metres and more in a fall, both together) and
        /// came off it again.
        /// </summary>
        public static bool HandsStayWhereTheyLand = true;

        /// <summary>
        /// A wrist this near the ground, or a hand whose underside is this near it, has landed (metres at a Medium's size); a hand going over the ground faster
        /// than this does not hold yet (m/s); one lifted this far over where it is held has come away, and one
        /// pulled this far along the ground from it is dragged (metres at a Medium's size); a hand that came away is
        /// not held again for this long (seconds); and all are let go this long after the hips are down and it is no
        /// longer going anywhere (seconds).
        /// </summary>
        public const float HandLandsWithin = 0.075f, HandUnderWithin = 0.035f, HandHoldsBelow = 4.5f, HandComesAwayAt = 0.12f, HandDragsPast = 0.03f, HandHeldAgainAfter = 0.25f, HandsHeldAfterLanding = 0.6f;

        /// <summary>Hands held where they landed, and hands that came away again, for the reports.</summary>
        public int HandsHeld, HandsCameAway;

        /// <summary>How far held hands were dragged over the ground (metres), for the reports.</summary>
        public float HandsDragged;

        private readonly float[] handFreeUntil = new float[2];
        private readonly float[] handUnderSince = { -1f, -1f };

        /// <summary>A held hand that the body has come down over is let go after this long under it (seconds): it is drawn out from under it, and held again beside it.</summary>
        public const float HandUnderFor = 0.1f;

        /// <summary>Held hands let go because the body had come down over them, for the reports.</summary>
        public int HandsFromUnder;

        private Vector2 trunkHalf;

        /// <summary>Half its chest's width and its depth (metres), as its solid has them.</summary>
        private Vector2 TrunkHalf()
        {
            if (trunkHalf.x <= 0f)
            {
                var chest = Rig.Part(PartId.Chest);
                if (chest != null && chest.partCollider is BoxCollider box)
                {
                    Vector3 scale = box.transform.lossyScale;
                    trunkHalf = new Vector2(0.5f * box.size.x * Mathf.Abs(scale.x), 0.5f * box.size.z * Mathf.Abs(scale.z));
                }
                else
                {
                    trunkHalf = new Vector2(0.155f, 0.09f) * Size;
                }
            }

            return trunkHalf;
        }

        /// <summary>A point on the ground is under its own trunk, as the body lies (a hand there is in it or under it).</summary>
        private bool UnderItsTrunk(Vector3 at, int hand)
        {
            if (!FallPose.HandsClearOfTrunk)
            {
                return false;
            }

            var pelvis = Rig.Part(PartId.Pelvis);
            var chest = Rig.Part(PartId.Chest);
            var head = Rig.Part(PartId.Head);
            if (pelvis == null || chest == null || head == null)
            {
                return false;
            }

            FallPose.ClearOfTrunk(pelvis.transform, chest.transform, head.transform, at, hand, Size, TrunkHalf(), groundLevel, -FallPose.HandInBy * Size, out bool under);
            return under;
        }

        private void StepLandedHands()
        {
            // (Tucked, its hands are in: none lands to be held.)
            // (Held until it is down and no longer going anywhere, and a moment more: a body that slides on has its
            // hands on the ground to stop it.)
            bool over = Dead || !HandsStayWhereTheyLand || tucked > 0.3f || (slowAt >= 0f && footingTime - slowAt > HandsHeldAfterLanding);
            for (int h = 0; h < 2; h++)
            {
                var id = h == 0 ? PartId.HandL : PartId.HandR;
                if (Rig.IsSevered(id))
                {
                    continue;
                }

                if (Upper.HandPinned(h))
                {
                    // (Lifted off the ground it has come away; pulled along the ground harder than it grips, it is
                    // dragged, and its hold goes with it: the pull on the body stays what the grip is.)
                    Vector3 off = Upper.Link(id).transform.position - Upper.HandPinAt(h);
                    bool away = off.y > HandComesAwayAt * Size;
                    // (The body has come down over it: let go, to be drawn out from under it. A hand held under a
                    // chest lay in it.)
                    bool bodyOver = UnderItsTrunk(Upper.HandPinAt(h), h);
                    handUnderSince[h] = !bodyOver ? -1f : handUnderSince[h] < 0f ? footingTime : handUnderSince[h];
                    bool lainOn = bodyOver && footingTime - handUnderSince[h] >= HandUnderFor;
                    if (over || away || lainOn)
                    {
                        Upper.PinHand(h, false, Vector3.zero, 1f, 1f);
                        handFreeUntil[h] = footingTime + HandHeldAgainAfter;
                        handUnderSince[h] = -1f;
                        HandsCameAway += away ? 1 : 0;
                        HandsFromUnder += lainOn && !over && !away ? 1 : 0;
                        continue;
                    }

                    off.y = 0f;
                    float past = off.magnitude - HandDragsPast * Size;
                    if (past > 0f)
                    {
                        Upper.MoveHandPin(h, Upper.HandPinAt(h) + off.normalized * past);
                        HandsDragged += past;
                    }

                    continue;
                }

                // (A hand with a shield on its arm is kept before the chest: it does not land.)
                if (over || footingTime < handFreeUntil[h] || (h == 0 && stance == Stances.Shield && FallPose.ShieldKeptBeforeChest))
                {
                    continue;
                }

                Vector3 at = Upper.Link(id).transform.position;
                Vector3 goes = Upper.SpeedOf(id);
                goes.y = 0f;
                // (Landed: its underside on the ground, as the fall's measure has it; or its wrist as low as a hand lies.)
                var shape = Rig.Part(id) != null ? Rig.Part(id).partCollider : null;
                float under = shape != null ? shape.ClosestPoint(new Vector3(at.x, groundLevel - 1f, at.z)).y - groundLevel : float.MaxValue;
                if ((under <= HandUnderWithin * Size || at.y - groundLevel <= HandLandsWithin * Size) && goes.magnitude <= HandHoldsBelow * Mathf.Sqrt(Size)
                    && !UnderItsTrunk(at, h))
                {
                    Upper.PinHand(h, true, at, Mathf.Max(0.25f, Mass / MediumMass), Size);
                    HandsHeld++;
                }
            }
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
        private readonly PoseBuffer lying = new PoseBuffer(), standingPose = new PoseBuffer(), startAsWas = new PoseBuffer();

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
            if (Poser.Borne != null)
            {
                // (Down in the middle of a stumble: its feet are let go.)
                Poser.Unbear();
                Stagger.End();
            }

            Built?.End();
            Falls++;
            // (A knock that was waiting to drive him back goes with the fall: the fall has its own speed. Left waiting,
            // it lay with him and threw him the moment he was on his feet again, by nothing: thrown at up to 6.2 m/s
            // and down a second time. Seen by Renier in the tube test, 19 November, where one tube both strikes him
            // and fells him.)
            if (pushAt >= 0f)
            {
                StaleKnocks++;
            }

            pushAt = -1f;
            pendingKnock = pendingImpulse = Vector3.zero;
            gathering = false;
            bodySeen = 0;
            swordHandHeld = false;
            gathered = false;
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
            fallTipSeen = false;
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

            HeldSolid(RagdollsHoldSolidThings);
            RagdollArmsToStop();
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

                if (GetUp.TakesOverAsHeMoves && fallBody && Upper != null)
                {
                    // (The pose his body has, kept for the last few steps: a get-up that takes him up as he moves reads
                    // his speeds off them. Over three steps, not one: what lies on the ground trembles from step to
                    // step, and off two steps a hand that lay still read as turning at 15 radians a second.)
                    bodyAt = (bodyAt + 1) % bodyPoses.Length;
                    bodyPoses[bodyAt].Capture(Rig);
                    bodySteps[bodyAt] = dt;
                    bodySeen++;
                }
                else
                {
                    bodySeen = 0;
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
                        FirstStepOfGetUp(dt);
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
                // (A body that settles is still at once, and would be up a second after it went down every time: half
                // a second sooner than bodies were on average, and all in step. It stays down as long as they did on
                // average, each fall a little longer or shorter than the last.)
                float least = fallBody && FallenBodiesSettle ? (tucks ? ThrownDownLonger : 0f) + SettledDown + SettledDownVaries * (((OneBodyFallen * 3 + Mathf.Abs(Mathf.RoundToInt(7f * pelvis.position.x + 3f * pelvis.position.z))) % 5) - 2) / 2f : LeastDown;
                // (One that was thrown may still be tumbling when the others would be made to get up.)
                // (19 November: not while another stands or rises where he would come up. He waits, at most a few seconds
                // more; the other, on his feet, is walked off him.)
                bool crowded = KeepsApartOffHisFeet && NoRoomToRise && footingTime < MostDown + WaitsForRoom;
                NoRoomToRise = false;
                if (crowded && !waitedForRoom && footingTime >= least && stillFor >= StillFor)
                {
                    waitedForRoom = true;
                    RoomWaits++;
                }

                bool ready = (footingTime >= least && stillFor >= StillFor) || footingTime >= MostDown + (fallBody && tucks ? ThrownDownMost : 0f);
                if (gathering)
                {
                    // (He is drawing himself to the get-up's start. It takes over once his hands and feet are there,
                    // or after the most a gather may take; not while there is no room to rise.)
                    gatherTime += dt;
                    foreach (var id in Upper.Own)
                    {
                        gatherFastest = Mathf.Max(gatherFastest, Upper.SpeedOf(id).magnitude);
                    }

                    float left = GatherFurthest();
                    float turnLeft = GatherTurns > 0f ? Quaternion.Angle(Rig.Part(PartId.Pelvis).transform.rotation, Anim.Part(PartId.Pelvis).rotation) : 0f;
                    GatherTurnLeft = turnLeft;
                    if (!crowded && gatherTime >= GatherLeast && ((left < GatherNear * Size && turnLeft < GatherTurnedWithin) || gatherTime >= GatherMost))
                    {
                        gathering = false;
                        Gathers++;
                        GathersCapped += left < GatherNear * Size && turnLeft < GatherTurnedWithin ? 0 : 1;
                        GatherTurnLeftOver += turnLeft;
                        Upper.TurnHips(null);
                        GatherSeconds += gatherTime;
                        GatherLeftOver += left;
                        GatherFastestSum += gatherFastest;
                        GatherFastestMost = Mathf.Max(GatherFastestMost, gatherFastest);
                        LastGatherTold = $"he gathered himself for {gatherTime:0.00} s (the fastest part {gatherFastest:0.0} m/s; the furthest hand or foot still {100f * left:0} cm from the start, his hips {turnLeft:0} degrees)";
                        waitedForRoom = false;
                        EndFallMeasure();
                        // (The get-up takes over from the body as it is: what it reads off the pose as it begins (where
                        // his sword hand lies) has to be the seen body's, not the start he was being drawn to.)
                        fallAsIs.Capture(Rig);
                        fallAsIs.Apply(Anim, false);
                        BeginGetUp();
                        FirstStepOfGetUp(dt);
                    }
                }
                else if (!StaysDown && !crowded && GathersBeforeGetUp && fallBody && Upper != null && NaturalGetUps && !Dead && !catching
                    && (ready || (footingTime >= least - GatherLead && stillFor >= StillFor)))
                {
                    // (The plan "From how he lies to how he gets up", step 3: the get-up is set out, and he goes on as
                    // the body he fell in, his muscles drawing him to its start.)
                    BeginGetUp(true);
                    gathering = true;
                    gatherTime = gatherFastest = 0f;
                    Upper.PinHand(0, false, Vector3.zero, 1f, 1f);
                    Upper.PinHand(1, false, Vector3.zero, 1f, 1f);
                }
                else if (!StaysDown && !crowded && ready)
                {
                    waitedForRoom = false;
                    EndFallMeasure();
                    BeginGetUp();
                    FirstStepOfGetUp(dt);
                }

                return;
            }

            StepGettingUp(dt, false);
        }

        /// <summary>
        /// A step of getting up: the get-up's pose, the body held to it, and his stand taking over at the end.
        /// <paramref name="takenUpNow"/>: this is the step he was taken up in (a get-up that takes him up as he moves
        /// begins in that same step, so that his body is not held still for one step first): nothing has been seen of
        /// it yet, so nothing is measured.
        /// </summary>
        private void StepGettingUp(float dt, bool takenUpNow)
        {
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
            if (!takenUpNow)
            {
                MeasureGetUpSeen(dt);
                MeasureGetUpStart(dt);
                if (footingTime >= getUp.Duration - GetUpEndFrom)
                {
                    MeasureGetUpEnd(dt, footingTime - getUp.Duration);
                }
            }

            if (fallBody && Upper != null)
            {
                // (His sword hand held still while he pushes himself up on it: where the pose has it pinned, the
                // physical hand is held to that place and that turn. Let go as the pose lets it go.)
                bool held = SwordHandHeldBodily && GetUp.SwordHandHeldStill && GetUp.SwordHandStays && stance != Stances.Open && getUp.SwordHandHeld && !Rig.IsSevered(PartId.HandR);
                // (Taken hold of only once the physical hand is where the pose has it, or it would be snatched there: a
                // shield bearer bashed down, taken up as he moved at 2.9 m/s, had his forearm jump 5.6 m/s in one step.)
                held = held && (swordHandHeld || (Rig.Part(PartId.HandR).body.position - Anim.Part(PartId.HandR).position).sqrMagnitude <= SwordHandTakenWithin * SwordHandTakenWithin * Size * Size);
                if (held)
                {
                    var posed = Anim.Part(PartId.HandR);
                    float heavy = Mathf.Max(0.25f, Mass / MediumMass);
                    if (!swordHandHeld)
                    {
                        // (Taken hold of where the physical hand is, and brought to where the pose has it over a
                        // fifth of a second: taken hold of at the pose's place, the few centimetres between them
                        // were crossed in one step, his forearm jumping 6.8 m/s.)
                        swordHandHeldOff = Rig.Part(PartId.HandR).body.position - posed.position;
                        swordHandHeldFor = 0f;
                    }

                    swordHandHeldFor += dt;
                    Vector3 holdAt = posed.position + swordHandHeldOff * (1f - Mathf.SmoothStep(0f, 1f, swordHandHeldFor / SwordHandTakenOver));
                    Upper.PinHand(1, true, holdAt, heavy, Size);
                    Upper.MoveHandPin(1, holdAt);
                    Upper.HoldHandTurn(1, posed.rotation, heavy);
                    swordHandHeld = true;
                }
                else if (swordHandHeld)
                {
                    LetSwordHandGo();
                }

                // (The one body it fell in is drawn to the get-up's pose, held as a standing body is.)
                upperDt = dt;
                Upper.BeginStep(dt);
            }
            float u = Mathf.Clamp01(footingTime / Mathf.Max(0.1f, getUp.Duration));
            for (int r = 0; r < 5; r++)
            {
                Authority.Set((BodyRegion)r, AuthorityLevel.Pinned, Mathf.Lerp(0.4f, 1f, u) * (r == (int)BodyRegion.Legs ? profile.torsoPin : BasePin((BodyRegion)r)));
            }

            if (!takenUpNow)
            {
                // (In the step he is taken up, the step of lying down has already had its turn.)
                Authority.Apply(dt);
            }

            if (footingTime >= getUp.Duration)
            {
                LetSwordHandGo();
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
                // (What his hands hold is settled afresh. The settling kept, from before he fell, where his shield hand
                // had been and how much of that to keep: as he stood it was let go over three steps, from a place a
                // fall old, and the shield hand jumped 12 cm out and back: 8 to 9 m/s at the end of every shield
                // bearer's get-up.)
                if (WholeBodyEasesOutOfGetUp)
                {
                    for (int a = 0; a < heldShare.Length; a++)
                    {
                        heldShare[a] = 0f;
                        heldKnown[a] = false;
                    }
                }

                // (From the body he has, not from the pose it was drawn to: the body lags its pose by some
                // centimetres, and put on the stand's pose it crossed them in one step, the shield hand at 8 m/s.)
                if (WholeBodyEasesOutOfGetUp && RoseFromTheBody)
                {
                    rose.Capture(Rig);
                }
                else
                {
                    rose.Capture(Anim);
                }

                roseAt = clock;
                TakeUpSword();
                getUpDuration = getUp.Duration;
                getUpLogAt = clock + RoseEase + 0.2f;
                LastReaction = "got up";
            }
        }

        private void StepFallMeasure(float dt)
        {
            // (Each step it is down: a ragdoll's arms, stopped by its body once they are out of it.)
            if (!fallBody)
            {
                StepRagdollArms();
            }

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

            falling.Ragdoll |= !fallBody;
            MeasureHeldInFall(dt);
            if ((Dead || StaysDown) && falling.Lasted >= FallMeasuredFor)
            {
                EndFallMeasure();
            }
        }

        private Vector3 fallTipWas;
        private bool fallTipSeen;

        /// <summary>
        /// What it holds through a fall, as seen (the plan "Controlled falls", step 11): its weapon and its shield
        /// against its own body and the ground, and its arms against its body. Nothing here changes a fall.
        /// </summary>
        private void MeasureHeldInFall(float dt)
        {
            // (Measured while a playtest is recorded, and headless: as what is seen of its arms on its feet is.)
            if (falling == null || dt <= 0f || !(MeasuresArms || Diagnostics.PlaytestTrace.Enabled))
            {
                return;
            }

            falling.HeldMeasured = true;
            bool arm = false;
            for (int a = 0; a < 2; a++)
            {
                var hand = a == 0 ? PartId.HandL : PartId.HandR;
                if (Rig.IsSevered(hand))
                {
                    continue;
                }

                Vector3 elbow = Rig.Part(a == 0 ? PartId.ForearmL : PartId.ForearmR).transform.position;
                Vector3 wrist = Rig.Part(hand).transform.position;
                arm |= SeenInside(Vector3.Lerp(elbow, wrist, 0.3f), wrist + (wrist - elbow).normalized * (0.08f * Size), TrunkSolids);
            }

            falling.ArmInFor += arm ? dt : 0f;
            var slot = Rig.SlotWeaponR;
            bool armed = false;
            foreach (var (hand, solid, mass, blade) in Solids)
            {
                if (solid == null || Rig.IsSevered(hand))
                {
                    continue;
                }

                if (blade && hand == PartId.HandR)
                {
                    armed = true;
                    continue;
                }

                if (blade)
                {
                    continue;
                }

                // (Its shield: how deep its board is in the trunk, the head or a thigh.)
                falling.Shielded = true;
                float deepest = 0f;
                foreach (var id in TrunkSolids)
                {
                    var part = Rig.Part(id);
                    if (part == null || part.partCollider == null || Rig.IsSevered(id))
                    {
                        continue;
                    }

                    if (Physics.ComputePenetration(solid, solid.transform.position, solid.transform.rotation, part.partCollider, part.partCollider.transform.position, part.partCollider.transform.rotation, out _, out float depth))
                    {
                        deepest = Mathf.Max(deepest, depth);
                    }
                }

                Vector3 middle = solid.transform.TransformPoint(solid is BoxCollider box ? box.center : Vector3.zero);
                falling.ShieldLowest = Mathf.Min(falling.ShieldLowest, solid.ClosestPoint(new Vector3(middle.x, groundLevel - 50f, middle.z)).y - groundLevel);
                falling.ShieldDeepest = Mathf.Max(falling.ShieldDeepest, deepest);
                falling.ShieldInFor += deepest > FallMeasure.ShieldInBy * Size ? dt : 0f;
            }

            if (armed && slot != null)
            {
                falling.Armed = true;
                Vector3 along = slot.rotation * Vector3.forward;
                Vector3 tip = slot.position + along * Poser.WeaponLength;
                falling.BladeInFor += SeenInside(slot.position + along * (0.15f * Size), tip, BladeSolids) ? dt : 0f;
                falling.BladeLowest = Mathf.Min(falling.BladeLowest, tip.y - groundLevel);
                if (fallTipSeen)
                {
                    falling.BladeFastest = Mathf.Max(falling.BladeFastest, (tip - fallTipWas).magnitude / dt);
                }

                fallTipWas = tip;
                fallTipSeen = true;
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

        /// <summary>A get-up starts from the start he gathered himself for, unless he has plainly rolled off it (Combat Feel; <c>FIGHT_KEEPSTART=0</c> chooses afresh as it takes him up, as before).</summary>
        public static bool KeepsTheStartHeGatheredFor = true;

        /// <summary>How far past the line between two starts his hips may have rolled and the gathered start still holds (of his front's up-and-down part: 0.3 is about 17 degrees).</summary>
        public const float GatheredStartHoldsFor = 0.3f;

        /// <summary>Get-ups that kept the start gathered for where his hips would have chosen another (report).</summary>
        public static int GetUpStartsKept;

        private bool gathered;
        private GetUp.Kind gatheredKind;
        private float gatheredSide;

        /// <summary>Natural get-ups: each is this much faster or slower than the last at most (a share of its length), so fighters felled together do not rise in step.</summary>
        public const float GetUpVaries = 0.08f;

        /// <summary>Get-ups, and for the reports the last one's measures: its route, how far it turned to its threat (degrees), the fastest a part moved early (m/s), the furthest a planted hand or foot was from its key (metres), how far planted ones slid (metres).</summary>
        public int GetUps;
        public readonly List<string> GetUpLog = new List<string>();
        public string GetUpRoute = "";
        public float GetUpTurned, GetUpFastest, GetUpMiss, GetUpSlid;

        /// <summary>
        /// A swordsman that has been down, or has for any reason not held its guard a while, takes its sword up from
        /// where its arm is now. Off (as it was): from where in the world it last held it, which after a fall is where
        /// it stood before: a metre and more behind the body that got up, and the arm reached for it there, through
        /// its own back, the whole of the turn that followed.
        /// </summary>
        public static bool SwordTakenUpWhereItIs = true;

        /// <summary>The guard not held for this long (seconds) is taken up afresh (<see cref="SwordTakenUpWhereItIs"/>).</summary>
        public const float SwordLeftFor = 0.2f;

        /// <summary>How many times a sword was taken up afresh after its guard had not been held a while, for the reports.</summary>
        public int SwordsTakenUp;

        private float parryHeldAt = -1f;

        /// <summary>The sword taken up from where the pose now has it (the end of a get-up; a guard not held a while).</summary>
        private void TakeUpSword()
        {
            if (!SwordTakenUpWhereItIs || stance != Stances.Sword || Parry == null)
            {
                return;
            }

            Parry.Resync();
            parryWaited = false;
            parryHeldAt = clock;
            SwordsTakenUp++;
        }

        /// <summary>How long a fighter's arms take to go from where the get-up left them to what a standing fighter's arms do (seconds).</summary>
        public const float RoseEase = 0.4f;

        /// <summary>
        /// The whole of him is eased out of a get-up into his stand, not only his arms (Combat Feel "A Get-Up Ends Into
        /// His Stand Over A Moment"; <c>FIGHT_GETUPEND=0</c> for the old way). The get-up's last pose and the stand's
        /// first are not the same pose: in Renier's log of 19 November, 20:10, every get-up in the bash lab ended with
        /// his hips jumping about 6 cm and his forearms 13 to 16 cm in one step.
        /// </summary>
        public static bool WholeBodyEasesOutOfGetUp = true;

        /// <summary>How long his trunk, head and legs take to go from where the get-up left them to where his stand has them (seconds).</summary>
        public const float RoseBodyEase = 0.2f;

        /// <summary>The easing out of a get-up starts from the body as it is seen, not from the get-up's last pose (<c>FIGHT_ROSEBODY=0</c>: from the pose).</summary>
        public static bool RoseFromTheBody = true;

        private readonly PoseBuffer rose = new PoseBuffer(), roseNow = new PoseBuffer(), roseMix = new PoseBuffer();
        private float roseAt = -1f, getUpLogAt = -1f, getUpDuration;

        private static bool IsArm(PartId id) =>
            id == PartId.UpperArmL || id == PartId.ForearmL || id == PartId.HandL || id == PartId.UpperArmR || id == PartId.ForearmR || id == PartId.HandR;

        /// <summary>From how long before he stands the end of a get-up is measured (seconds); it is measured until his arms have eased out of it (<see cref="RoseEase"/>).</summary>
        public const float GetUpEndFrom = 0.15f;

        /// <summary>
        /// For the reports, of the last get-up's end, as seen: the biggest jump in any part's speed from one step to
        /// the next as the stand takes over from the get-up (m/s), which part, and when against the step he stood
        /// (seconds). The other end of the take-over's measure: in Renier's log of 19 November, 20:10, every get-up in
        /// the bash lab ended with his forearms jumping 8 to 10 m/s and his hips 3 in one step, and nothing measured it.
        /// </summary>
        public float GetUpEndJump, GetUpEndJumpAt;
        public string GetUpEndJumpPart = "";
        private readonly Vector3[] endWas = new Vector3[PartInfo.All.Length], endSpeed = new Vector3[PartInfo.All.Length];
        private int endSeen;
        private Vector3 endHipsWas;
        private float endSampledAt = -1f;

        private void MeasureGetUpEnd(float dt, float at)
        {
            if (dt <= 0f || clock == endSampledAt)
            {
                return;
            }

            endSampledAt = clock;
            // (Set down somewhere else, as a lab does with its pair: not a jump of his. Measured afresh from there.)
            Vector3 hipsNow = Rig.Part(PartId.Pelvis).transform.position;
            if (endSeen >= 1 && (hipsNow - endHipsWas).magnitude > 0.2f)
            {
                endSeen = 0;
            }

            endHipsWas = hipsNow;
            for (int i = 0; i < PartInfo.All.Length; i++)
            {
                var id = PartInfo.All[i];
                if (Rig.IsSevered(id))
                {
                    continue;
                }

                Vector3 now = Rig.Part(id).transform.position;
                if (endSeen >= 1)
                {
                    Vector3 speed = (now - endWas[i]) / dt;
                    if (endSeen >= 2)
                    {
                        float jump = (speed - endSpeed[i]).magnitude;
                        if (jump > GetUpEndJump)
                        {
                            GetUpEndJump = jump;
                            GetUpEndJumpAt = at;
                            GetUpEndJumpPart = id.ToString();
                        }
                    }

                    endSpeed[i] = speed;
                }

                endWas[i] = now;
            }

            endSeen++;
        }

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
                    // (The rest of him over a shorter moment. Before, only his arms were eased: his trunk, head and legs
                    // were where the stand has them in the very next step.)
                    float body = 1f;
                    if (WholeBodyEasesOutOfGetUp)
                    {
                        body = Mathf.Clamp01((clock - roseAt) / RoseBodyEase);
                        body = body * body * (3f - 2f * body);
                    }

                    roseMix.MixToward(roseNow, id => IsArm(id) ? w : body);
                    roseMix.Apply(Anim, false);
                }
            }

            if (getUpLogAt >= 0f && clock <= getUpLogAt - 0.2f)
            {
                MeasureGetUpEnd(dt, clock - (getUpLogAt - RoseEase - 0.2f));
            }

            MeasureArmsSeen(dt);
            if (getUpLogAt >= 0f && clock >= getUpLogAt)
            {
                LogGetUp();
            }
        }

        /// <summary>The last get-up said, with what was seen of it up to a moment after it stood.</summary>
        // ---- The first moment of a get-up, measured (the plan "From how he lies to how he gets up", step 1; 19 November) ----

        /// <summary>How much of a get-up counts as its first moment (seconds): the blend from how he lay, where nine tenths of a get-up's crossings were in Renier's log of 19 November.</summary>
        public const float GetUpStartFor = 0.75f;

        /// <summary>
        /// For the reports, of the last get-up's first moment (<see cref="GetUpStartFor"/>): how long his own shield,
        /// sword or sword arm crossed him as seen (seconds; counted only while crossings are measured), how far each
        /// hand went as seen (metres: the shield hand, the sword hand), and the fastest a part of the seen body moved
        /// (m/s) and which.
        /// </summary>
        public float GetUpStartCrossed, GetUpStartHandL, GetUpStartHandR, GetUpStartFastest;
        public string GetUpStartFastestPart = "";

        /// <summary>How much of a get-up counts as the take-over (seconds): the moment the get-up takes the fallen body up, and the steps just after.</summary>
        public const float TakeOverFor = 0.2f;

        /// <summary>
        /// For the reports, of the last get-up's take-over (<see cref="TakeOverFor"/>; the plan "From how he lies to
        /// how he gets up", step 4): the most the speed of a part of the seen body changed from one step to the next
        /// (m/s: a part stopped dead, or thrown, shows as a jump), which part and when (seconds into the get-up); and
        /// how fast his fastest part was moving as the get-up took him up (m/s).
        /// </summary>
        public float TakeOverJump, TakeOverJumpAt, TakeOverSpeed;
        public string TakeOverJumpPart = "";

        /// <summary>Of the last get-up's take-over: how long what was left between his body and the get-up's pose took to ease out (seconds; the old blend's time with the new take-over off), and how far the furthest hand or foot was from where the get-up had it (metres; 0 with it off).</summary>
        public float TakeOverTook, TakeOverFrom;

        /// <summary>Of the last get-up, as seen: the lowest the middle of a knee was over the ground (metres; negative: under it), and how long one was in the ground (seconds).</summary>
        public float GetUpKneeSeenLowest = 9f, GetUpKneeSeenInGround;

        /// <summary>The last get-up's first moment in a line, for the panel and the log ("" before the first).</summary>
        public string GetUpStartTold = "";

        /// <summary>The same of the last get-up of any fighter, with his name ("" before the first).</summary>
        public static string LastGetUpStartTold = "";

        /// <summary>A get-up began (after its keys are made and his body taken up; before its first step). For the lab's stop frame.</summary>
        public static event System.Action<ClipFighter> GetUpBegan;

        /// <summary>For the reports: every get-up's first moment, all fighters, since the run began.</summary>
        public struct GetUpStart
        {
            public float Crossed, Fastest, HandL, HandR, Jump, Speed, Took, From, KneePosed, KneeSeen, KneeInGround, HandStill, EndJump;
        }

        public static readonly List<GetUpStart> GetUpStarts = new List<GetUpStart>();

        /// <summary>How the last get-up's start was chosen, in a few words ("" when it was by his hips only).</summary>
        public string GetUpStartChose = "";

        /// <summary>
        /// He gathers himself before he gets up (the plan "From how he lies to how he gets up", step 3; Combat Feel "He
        /// Gathers Himself Before He Gets Up"). A little before he would begin to rise, the get-up is set out, and for a
        /// moment he goes on as the physical body he fell in while his muscles draw his arms and legs to the get-up's
        /// start. His trunk lies as it lies; the ground, his own body and his shield stay solid to his limbs, so they
        /// go round what is in the way. The get-up takes over once his hands and feet are within
        /// <see cref="GatherNear"/> of the start (measured from his hips), or after <see cref="GatherMost"/>, and blends
        /// from the pose he then has. Off: as before, the get-up takes over where he lies and blends from there.
        /// </summary>
        public static bool GathersBeforeGetUp = true;

        /// <summary>The most a gather may take (seconds), and the share of a person's strength his muscles gather with.</summary>
        public static float GatherMost = DefaultGatherMost, GatherMuscle = DefaultGatherMuscle;
        public const float DefaultGatherMost = 0.6f, DefaultGatherMuscle = 1f;

        /// <summary>
        /// Gathering, his trunk is turned toward how the get-up's start has it (Combat Feel "Gather Turns His Trunk"):
        /// the most torque that turns it, as a share of his weight on a metre's lever (0: his trunk lies as it lies,
        /// as the first build had it), and how quick the turn is (Hz). Off (0) for now (19 November): at 0.25 it did
        /// not finish the gathers from his side (his hips still 21 to 28 degrees off as the clock ended them) and
        /// made the take-over worse, because he was still rolling as the get-up took him up: the fastest part in a
        /// get-up's first moment 11.5 m/s on average felled to his right, against 4.1 without it. It needs the
        /// take-over from a body that is moving (the plan's step 4) first.
        /// </summary>
        public static float GatherTurns = DefaultGatherTurns;
        public const float DefaultGatherTurns = 0f, GatherTurnQuick = 1.5f, GatherTurnedWithin = 20f;

        /// <summary>A gather begins this long before he would rise (seconds: most of it is time he lay still anyway); takes at least this long; its strength comes in over this long; and is done when each hand and foot is within this of the start (metres at a Medium's size).</summary>
        public const float GatherLead = 0.4f, GatherLeast = 0.15f, GatherEasesIn = 0.15f, GatherNear = 0.12f;

        /// <summary>For the reports (all fighters): gathers, those ended by the clock and not by being there, the seconds they took, and how far the furthest hand or foot still was as the get-up took over (metres, summed).</summary>
        public static int Gathers, GathersCapped;
        public static float GatherSeconds, GatherLeftOver, GatherFastestSum, GatherFastestMost, GatherTurnLeftOver;

        /// <summary>How far his hips are still turned from the start's as he gathers (degrees; 0 with the turning off).</summary>
        public float GatherTurnLeft;

        /// <summary>The last gather of any fighter in a few words ("" before the first, or with gathering off).</summary>
        public static string LastGatherTold = "";
        private float gatherFastest;

        /// <summary>He is gathering himself: down still, the get-up set out, his body drawn to its start.</summary>
        public bool Gathering => gathering;
        private bool gathering;
        private float gatherTime;

        private static readonly PartId[] GatherEnds = { PartId.HandL, PartId.HandR, PartId.FootL, PartId.FootR };

        /// <summary>How far the furthest of his hands and feet is from where the pose has it, each measured from his own hips (metres). His sword hand is left out when it stays where it lies.</summary>
        private float GatherFurthest()
        {
            var seenHips = Rig.Part(PartId.Pelvis).transform;
            var posedHips = Anim.Part(PartId.Pelvis);
            Quaternion seenBack = Quaternion.Inverse(seenHips.rotation), posedBack = Quaternion.Inverse(posedHips.rotation);
            float most = 0f;
            foreach (var id in GatherEnds)
            {
                if (Rig.IsSevered(id) || (id == PartId.HandR && stance != Stances.Open && GetUp.SwordHandStays))
                {
                    continue;
                }

                Vector3 seen = seenBack * (Rig.Part(id).transform.position - seenHips.position);
                Vector3 posed = posedBack * (Anim.Part(id).position - posedHips.position);
                most = Mathf.Max(most, (seen - posed).magnitude);
            }

            return most;
        }

        /// <summary>
        /// A get-up starts nearest to how he lies (the plan "From how he lies to how he gets up", step 2; Combat Feel
        /// "A Get-Up Starts Nearest To How He Lies"). Every start he could use is set out and weighed: how far his
        /// hands, elbows, knees and feet are from where that get-up has them as its blend ends, with more for a hand
        /// whose straight way there passes through his own trunk. The ways weighed: the one his hips say; from a side
        /// as well, if his hips are turned that way at all; each posting on either hand (a sword alone always posts
        /// on its free hand). Off: as before, by which way his hips face only, posting on the hand the stance says.
        /// Off for now (19 November): by itself it did not help. Over twelve bash-downs each way his own shield or
        /// sword crossed him 0.19 s in a get-up's first moment with it, 0.14 s without: the nearest start for his
        /// joints is often the one that posts on his shield hand, which lays the board under him. It is for the
        /// gather (step 3) to use, with the board counted in what a start costs.
        /// </summary>
        public static bool GetUpStartNearest;

        /// <summary>His hips' front within this of level (its height, of 1) a get-up from his side is weighed too; another start is taken only if it is nearer by this much (metres at a Medium's size); a hand's way through his trunk counts as this much further (metres at a Medium's size).</summary>
        public const float SideConsideredWithin = 0.75f, StartMargin = 0.15f, ThroughTrunkCosts = 0.6f;

        /// <summary>For the reports: get-ups whose start was weighed, and those where another start than the one his hips say was taken.</summary>
        public static int StartsWeighed, StartsChosen;

        private static readonly (PartId id, float counts)[] StartParts =
        {
            (PartId.HandL, 1f), (PartId.HandR, 1f), (PartId.ForearmL, 0.5f), (PartId.ForearmR, 0.5f),
            (PartId.ShinL, 0.5f), (PartId.ShinR, 0.5f), (PartId.FootL, 0.75f), (PartId.FootR, 0.75f),
        };

        /// <summary>
        /// How far he is from the start the pose now has (metres, weighted): each hand, elbow, knee and ankle from where
        /// it lies (the seen body) to where the pose has it, and <see cref="ThroughTrunkCosts"/> more for a hand whose
        /// straight way there passes through his trunk as it lies.
        /// </summary>
        private float StartCost()
        {
            float cost = 0f;
            foreach (var (id, counts) in StartParts)
            {
                if (!Rig.IsSevered(id))
                {
                    cost += counts * Vector3.Distance(Rig.Part(id).transform.position, Anim.Part(id).position);
                }
            }

            Vector3 hips = Rig.Part(PartId.Pelvis).transform.position, neck = Rig.Part(PartId.Head).transform.position;
            foreach (var hand in new[] { PartId.HandL, PartId.HandR })
            {
                if (!Rig.IsSevered(hand) && SegmentsApart(Rig.Part(hand).transform.position, Anim.Part(hand).position, hips, neck) < 0.11f * Size)
                {
                    cost += ThroughTrunkCosts * Size;
                }
            }

            return cost;
        }

        /// <summary>The least distance between two lines of a given length each (from a to b, from c to d).</summary>
        private static float SegmentsApart(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            Vector3 u = b - a, v = d - c, w = a - c;
            float uu = Vector3.Dot(u, u), uv = Vector3.Dot(u, v), vv = Vector3.Dot(v, v), uw = Vector3.Dot(u, w), vw = Vector3.Dot(v, w);
            float den = uu * vv - uv * uv;
            float s = den > 1e-8f ? Mathf.Clamp01((uv * vw - vv * uw) / den) : 0f;
            float t = vv > 1e-8f ? Mathf.Clamp01((uv * s + vw) / vv) : 0f;
            s = uu > 1e-8f ? Mathf.Clamp01((uv * t - uw) / uu) : 0f;
            return Vector3.Distance(a + u * s, c + v * t);
        }

        private readonly Vector3[] startWas = new Vector3[PartInfo.All.Length], startSpeed = new Vector3[PartInfo.All.Length], jumpWas = new Vector3[PartInfo.All.Length];
        private readonly bool[] startSpeedKnown = new bool[PartInfo.All.Length];
        private bool startKnown;

        private void MeasureGetUpStart(float dt)
        {
            if (footingTime > GetUpStartFor || dt <= 0f)
            {
                return;
            }

            for (int i = 0; i < PartInfo.All.Length; i++)
            {
                var id = PartInfo.All[i];
                if (Rig.IsSevered(id))
                {
                    continue;
                }

                Vector3 at = Rig.Part(id).transform.position;
                if (startKnown)
                {
                    float went = (at - startWas[i]).magnitude;
                    if (went / dt > GetUpStartFastest)
                    {
                        GetUpStartFastest = went / dt;
                        GetUpStartFastestPart = id.ToString();
                    }

                    GetUpStartHandL += id == PartId.HandL ? went : 0f;
                    GetUpStartHandR += id == PartId.HandR ? went : 0f;
                }

                if (startSpeedKnown[i] && footingTime <= TakeOverFor + dt)
                {
                    // (The take-over: how much this part's speed changed since the step before. The first step is
                    // measured against the speed the fallen body had as it was taken up.)
                    Vector3 speed = (at - jumpWas[i]) / dt;
                    float jump = (speed - startSpeed[i]).magnitude;
                    if (jump > TakeOverJump)
                    {
                        TakeOverJump = jump;
                        TakeOverJumpAt = footingTime;
                        TakeOverJumpPart = id.ToString();
                    }

                    startSpeed[i] = speed;
                }

                jumpWas[i] = at;
                startWas[i] = at;
            }

            startKnown = true;
        }

        private void LogGetUp()
        {
            if (getUpLogAt < 0f)
            {
                return;
            }

            getUpLogAt = -1f;
            GetUpStartTold = $"{GetUpRoute}; its first {GetUpStartFor:0.00} s: his own shield or sword crossing him {GetUpStartCrossed:0.00} s, the fastest part {GetUpStartFastest:0.0} m/s ({GetUpStartFastestPart}), the shield hand went {GetUpStartHandL:0.00} m and the sword hand {GetUpStartHandR:0.00} m; the take-over: taken up moving at {TakeOverSpeed:0.0} m/s, the biggest jump in a part's speed in one step {TakeOverJump:0.0} m/s ({TakeOverJumpPart} at {TakeOverJumpAt:0.00} s), {(GetUp.TakesOverAsHeMoves ? $"what was left eased out in {TakeOverTook:0.00} s (a hand or foot {100f * TakeOverFrom:0} cm off at the most)" : "blended from how he lay (old)")}{(GetUpStartChose.Length > 0 ? "; " + GetUpStartChose : "")}";
            LastGetUpStartTold = (FightUnit != null ? FightUnit.name : Rig.name) + " got up " + GetUpStartTold;
            GetUpStarts.Add(new GetUpStart { Crossed = GetUpStartCrossed, Fastest = GetUpStartFastest, HandL = GetUpStartHandL, HandR = GetUpStartHandR, Jump = TakeOverJump, Speed = TakeOverSpeed, Took = TakeOverTook, From = TakeOverFrom, KneePosed = getUp.KneeLowest, KneeSeen = GetUpKneeSeenLowest, KneeInGround = GetUpKneeSeenInGround, HandStill = getUp.SwordHandStillFor, EndJump = GetUpEndJump });
            GetUpLog.Add($"got up {GetUpRoute} in {getUpDuration:0.00} s, turned {GetUpTurned:0} degrees toward what it fights; the fastest a part moved early {GetUpFastest:0.0} m/s ({getUp.Fastest}); a planted hand or foot at most {100f * GetUpMiss:0} cm from where its pose wanted it ({getUp.Missed}), and they slid {100f * GetUpSlid:0} cm in all; an arm in its own body at most {100f * getUp.ArmDeepest:0} cm{(getUp.ArmDeepestAt.Length > 0 ? " (" + getUp.ArmDeepestAt + ")" : "")}, more than 2 cm in for {getUp.ArmInFor:0.00} s; what it holds at most {100f * getUp.BladeDeepest:0} cm, for {getUp.BladeInFor:0.00} s; a thigh turned on the hips at most {getUp.ThighFastest:0.0} rad/s ({getUp.ThighFastestAt}) and was at most {getUp.ThighPast:0} degrees past the hip's range ({getUp.ThighPastAt}); an ankle at the nearest {getUp.AnkleNearest:0.00} of a leg's length from its hip ({getUp.AnkleNearestAt}); as seen (the physical body): an arm in its body for {GetUpArmSeenIn:0.00} s, what it holds for {GetUpBladeSeenIn:0.00} s"
                + $"; as he stood (from {GetUpEndFrom:0.00} s before to {RoseEase:0.00} s after): the biggest jump in a part's speed in one step {GetUpEndJump:0.0} m/s ({GetUpEndJumpPart} at {GetUpEndJumpAt:+0.00;-0.00} s)"
                + (GetUpShieldSeenLowest < 8f ? $"; its shield over the ground at the lowest {100f * GetUpShieldPosedLowest:0} cm as posed, {100f * GetUpShieldSeenLowest:0} cm as seen (at {GetUpShieldLowestAt:0.00} s), more than 2 cm in the ground for {GetUpShieldInFloorFor:0.00} s" : ""));
            GetUpLog[GetUpLog.Count - 1] += $"; his knees: at the lowest {100f * getUp.KneeLowest:0} cm over the ground as posed, {100f * GetUpKneeSeenLowest:0} cm as seen, in the ground for {GetUpKneeSeenInGround:0.00} s (a foot moved {100f * getUp.KneeSlidFoot:0} cm from where it was asked, to keep its knee out); his sword hand held still for {getUp.SwordHandStillFor:0.00} s";
            GetUpLog[GetUpLog.Count - 1] += $"; the start: {GetUpStartTold}";
            if (Diagnostics.PlaytestTrace.Enabled)
            {
                Diagnostics.PlaytestTrace.Note(FightUnit, "getup", GetUpLog[GetUpLog.Count - 1]);
            }
        }

        private static readonly PartId[] TrunkSolids = { PartId.Pelvis, PartId.Spine, PartId.Chest, PartId.Head, PartId.ThighL, PartId.ThighR };
        private static readonly PartId[] BladeSolids = { PartId.Pelvis, PartId.Spine, PartId.Chest, PartId.Head, PartId.ThighL, PartId.ThighR, PartId.ShinL, PartId.ShinR };

        /// <summary>For the reports: how long through the last get-up the body as it is seen (the physical one) had a forearm or hand inside its own trunk, head or thighs, and what its right hand holds inside its trunk, head or legs (seconds).</summary>
        public float GetUpArmSeenIn, GetUpBladeSeenIn;

        /// <summary>
        /// For the reports: through the last get-up, the lowest its shield's rim came over the ground (metres; below 0:
        /// in it) as its pose had it and as seen (the physical body), when through the get-up that was (seconds), and
        /// how long it was seen more than 2 cm in the ground (seconds).
        /// </summary>
        public float GetUpShieldPosedLowest = 9f, GetUpShieldSeenLowest = 9f, GetUpShieldLowestAt, GetUpShieldInFloorFor;

        /// <summary>What it holds, as its unit has it (the weapons' own measures: a shield's rim, a blade's length).</summary>
        public Combat.Loadout Kit;

        /// <summary>
        /// A get-up knows what the hands hold (18 November; Combat Feel "Get Ups Keep What The Hands Hold Off The
        /// Ground"). The get-up has had, since the first shields, its own care of a shield: the board laid flat under
        /// the forearm while the hands push, held up toward what it fights as it comes off them, and what both hands
        /// hold kept out of the ground. All of it reads the weapons' measures off the loadout, and a clip fighter's
        /// get-up was never given one (the old pose pipeline gave it; that went on 11 November), so none of it ran. Off:
        /// as it was, the get-up not knowing what is held.
        /// </summary>
        public static bool GetUpsKnowWhatIsHeld = true;

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

            // (The pose itself: a swordsman on its feet, not striking, whose sword hand is wanted behind its own chest.
            // No guard has it there: it is reaching for somewhere the body is not.)
            bool behind = false;
            if (stance == Stances.Sword && Parry != null && Footing == Footings.Standing && !Striking && !Rig.IsSevered(PartId.HandR))
            {
                var chest = Anim.Part(PartId.Chest);
                behind = Vector3.Dot(Anim.Part(PartId.HandR).position - chest.position, chest.forward) < -SwordBehindBy * Size;
            }

            SwordBehindFor += behind ? dt : 0f;
            if (behind)
            {
                swordBehind += dt;
            }
            else
            {
                if (swordBehind >= 0.15f && Diagnostics.PlaytestTrace.Enabled)
                {
                    Diagnostics.PlaytestTrace.Note(FightUnit, "arms", $"its pose had its sword hand behind its own chest for {swordBehind:0.00} s ({Loco.Current} {(Loco.Shot != null ? Loco.Shot.Name : "")}; {LastReaction})");
                }

                swordBehind = 0f;
            }
        }

        /// <summary>How long its pose had its sword hand behind its own chest, on its feet and not striking (seconds), for the reports.</summary>
        public float SwordBehindFor;

        /// <summary>A sword hand this far behind the chest's middle (metres at a Medium's size) is behind it.</summary>
        public const float SwordBehindBy = 0.15f;

        private float swordBehind;

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

        private static readonly (PartId id, string name)[] BoardSolids =
        {
            (PartId.Chest, "chest"), (PartId.Spine, "belly"), (PartId.Pelvis, "hips"), (PartId.Head, "head"),
            (PartId.ThighL, "shield-side thigh"), (PartId.ThighR, "sword-side thigh"), (PartId.ShinL, "shield-side shin"), (PartId.ShinR, "sword-side shin"),
            (PartId.UpperArmR, "sword upper arm"),
        };

        private readonly float[] crossFor = new float[3], crossAt = new float[3];
        private float crossBoardOff, crossTipOff, crossTurnOff;
        private readonly string[] crossDoing = { "", "", "" }, crossWhat = { "", "", "" };

        /// <summary>What he is doing, for <see cref="SelfCrossings"/>; and the attack he is striking with, if he is.</summary>
        private string DoingNow(out string attack)
        {
            attack = null;
            if (Footing == Footings.GettingUp)
            {
                return "getting up";
            }

            if (Footing == Footings.Down)
            {
                return "falling or down";
            }

            if (Bashing)
            {
                return "bashing";
            }

            if (Stumbling)
            {
                return "stumbling";
            }

            if (Striking)
            {
                attack = Built != null && Built.Playing ? Built.Name : Combo != null && Combo.Playing ? "a combo" : Loco.Shot != null ? Loco.Shot.Name : "a blow";
                return "striking";
            }

            if (SinceStruck < 0.5f)
            {
                return "just struck";
            }

            if (Loco.Current == Locomotor.Mode.OneShot)
            {
                return "a gesture";
            }

            return Loco.Velocity.magnitude > 0.3f ? "moving" : "standing";
        }

        private readonly Vector3[,] shakeWas = new Vector3[HeldShake.Signals, 5];
        private int shakeCount;

        /// <summary>How much what he holds trembles, as seen, as posed, and the seen against the posed (<see cref="HeldShake"/>). It changes nothing.</summary>
        private void MeasureHeldShake(float dt)
        {
            // (Not its first second: it is set down and takes up its guard.)
            if (!(SelfCrossings.Measures || Diagnostics.PlaytestTrace.Enabled) || Authority == null || dt <= 0f || stance == Stances.Open || Footing == Footings.Down || clock < 1f)
            {
                shakeCount = 0;
                return;
            }

            var now = new Vector3[HeldShake.Signals];
            bool board = stance == Stances.Shield && Rig.SlotWeaponL != null && Anim.SlotWeaponL != null && BoardOnSlot(out var onSlot) && !Rig.IsSevered(PartId.HandL);
            if (board)
            {
                BoardOnSlot(out var local);
                now[0] = Rig.SlotWeaponL.TransformPoint(local);
                now[1] = Anim.SlotWeaponL.TransformPoint(local);
                now[2] = now[0] - now[1];
            }

            bool blade = Rig.SlotWeaponR != null && Anim.SlotWeaponR != null && !Rig.IsSevered(PartId.HandR);
            if (blade)
            {
                now[3] = Rig.SlotWeaponR.position + Rig.SlotWeaponR.rotation * Vector3.forward * Poser.WeaponLength;
                now[4] = Anim.SlotWeaponR.position + Anim.SlotWeaponR.rotation * Vector3.forward * Poser.WeaponLength;
                now[5] = now[3] - now[4];
            }

            now[6] = Rig.Part(PartId.Chest).transform.position - Anim.Part(PartId.Chest).position;
            now[7] = Rig.Part(PartId.HandL).transform.position - Anim.Part(PartId.HandL).position;
            now[8] = Rig.Part(PartId.HandR).transform.position - Anim.Part(PartId.HandR).position;
            now[9] = Rig.Part(PartId.Pelvis).transform.position - Anim.Part(PartId.Pelvis).position;
            for (int s = 0; s < HeldShake.Signals; s++)
            {
                for (int i = 0; i < 4; i++)
                {
                    shakeWas[s, i] = shakeWas[s, i + 1];
                }

                shakeWas[s, 4] = now[s];
            }

            shakeCount++;
            if (shakeCount < 5)
            {
                return;
            }

            string doing = DoingNow(out _);
            HeldShake.Watched(doing, dt);
            for (int s = 0; s < HeldShake.Signals; s++)
            {
                if (s < 3 ? !board : s < 6 && !blade)
                {
                    continue;
                }

                Vector3 mean = (shakeWas[s, 0] + shakeWas[s, 1] + shakeWas[s, 2] + shakeWas[s, 3] + shakeWas[s, 4]) / 5f;
                float tremble = (shakeWas[s, 2] - mean).magnitude;
                HeldShake.Add(doing, s, tremble, dt);
                if (HeldShake.Trace && doing == "getting up" && (s == 1 || s == 4))
                {
                    Debug.Log(string.Format(System.Globalization.CultureInfo.InvariantCulture, "GETUPSHAKE {0} {1:0.000} {2}: {3:0.0} mm; gap {4:0.0} cm",
                        FightUnit != null ? FightUnit.name : "?", footingTime, HeldShake.Names[s], 1000f * tremble, 100f * getUp.WeaponGroundGap));
                }

                if (HeldShake.Trace && s == 4 && tremble > 0.02f && (doing == "standing" || doing == "moving"))
                {
                    // (19 November, for finding why the blade's tip trembles on his feet: what he was at as the posed tip jumped.)
                    Debug.Log(string.Format(System.Globalization.CultureInfo.InvariantCulture, "TIPLOG {0:0.00} {1} tip posed: {2:0} mm, {3}; {4}; parry {5}; a blow coming {6}; posturing {7}; struck {8:0.0} s ago; {9} {10}",
                        clock, FightUnit != null ? FightUnit.name : "?", 1000f * tremble, doing, stance, Parry != null ? Parry.CurrentName : "none", Blow.HasValue ? "yes" : "no", Posturing ? "yes" : "no",
                        clock - lastHit, Loco.Current, Loco.Shot != null ? Loco.Shot.Name : ""));
                }

                if (HeldShake.Trace && (s == 1 || s == 2 || s == 7) && tremble > 0.015f && (doing == "standing" || doing == "moving"))
                {
                    Debug.Log(string.Format(System.Globalization.CultureInfo.InvariantCulture, "SHAKELOG {0:0.00} {1} {2}: {3:0} mm, {4}; cover {5:0.00}; last: {6}; {7} {8}",
                        clock, FightUnit != null ? FightUnit.name : "?", HeldShake.Names[s], 1000f * tremble, doing, interceptWeight, LastReaction, Loco.Current, Loco.Shot != null ? Loco.Shot.Name : ""));
                }
            }
        }

        /// <summary>
        /// Counts what his own shield and sword cross of himself, as seen (<see cref="SelfCrossings"/>): the blade and the
        /// sword arm against the board's disc, the board's rim and face against his own solids. It changes nothing.
        /// </summary>
        private void MeasureOwnCrossings(float dt)
        {
            if (!(SelfCrossings.Measures || Diagnostics.PlaytestTrace.Enabled) || Authority == null || dt <= 0f
                || stance != Stances.Shield || !ShieldBoard(out Vector3 centre, out Vector3 normal, out float radius) || Rig.IsSevered(PartId.HandL))
            {
                return;
            }

            string doing = DoingNow(out string attack);
            SelfCrossings.Watched(doing, dt, attack);
            bool blade = false, arm = false, board = false;
            string what = "";
            var slot = Rig.SlotWeaponR;
            if (slot != null && !Rig.IsSevered(PartId.HandR))
            {
                Vector3 along = slot.rotation * Vector3.forward;
                blade = ThroughBoard(slot.position + along * (0.15f * Size), slot.position + along * Poser.WeaponLength, centre, normal, radius);
                Vector3 elbow = Rig.Part(PartId.ForearmR).transform.position, wrist = Rig.Part(PartId.HandR).transform.position;
                arm = ThroughBoard(elbow, wrist + (wrist - elbow).normalized * (0.08f * Size), centre, normal, radius);
            }

            // (The board: its middle, a ring half way out and its rim, against his own solids.)
            Vector3 u = Vector3.Cross(normal, Vector3.up);
            u = u.sqrMagnitude > 1e-6f ? u.normalized : Vector3.Cross(normal, Vector3.forward).normalized;
            Vector3 v = Vector3.Cross(normal, u);
            foreach (var (id, name) in BoardSolids)
            {
                var part = Rig.Part(id);
                if (part == null || part.partCollider == null || Rig.IsSevered(id))
                {
                    continue;
                }

                bool inside = Inside(part.partCollider, centre);
                for (int i = 0; i < 20 && !inside; i++)
                {
                    float a = i * (Mathf.PI / 6f), r = i < 12 ? 0.95f * radius : 0.5f * radius;
                    inside = Inside(part.partCollider, centre + (u * Mathf.Cos(a) + v * Mathf.Sin(a)) * r);
                }

                if (inside)
                {
                    board = true;
                    what = what.Length > 0 ? what + ", " + name : name;
                    SelfCrossings.InPart(name, dt);
                }
            }

            // (The pose itself: the same three, on the posed skeleton.)
            var posedSlot = Anim.SlotWeaponL;
            if ((blade || arm || board) && posedSlot != null && BoardOnSlot(out var onSlot))
            {
                Vector3 pc = posedSlot.TransformPoint(onSlot), pn = posedSlot.forward;
                var sword = Anim.SlotWeaponR;
                if (sword != null)
                {
                    Vector3 along = sword.rotation * Vector3.forward;
                    Vector3 elbow = Anim.Part(PartId.ForearmR).position, wrist = Anim.Part(PartId.HandR).position;
                    if (blade && ThroughBoard(sword.position + along * (0.15f * Size), sword.position + along * Poser.WeaponLength, pc, pn, radius))
                    {
                        SelfCrossings.Posed(doing, SelfCrossings.Blade, dt);
                    }

                    if (arm && ThroughBoard(elbow, wrist + (wrist - elbow).normalized * (0.08f * Size), pc, pn, radius))
                    {
                        SelfCrossings.Posed(doing, SelfCrossings.SwordArm, dt);
                    }
                }

                bool posedIn = false;
                foreach (var (id, _) in BoardSolids)
                {
                    if (!board || posedIn || !PoseSolid(id, out Vector3 a, out Vector3 b, out float thick))
                    {
                        continue;
                    }

                    for (int i = 0; i <= 4 && !posedIn; i++)
                    {
                        posedIn = DiscGap(Vector3.Lerp(a, b, i / 4f), pc, pn, radius, out _) - thick < 0f;
                    }
                }

                if (posedIn)
                {
                    SelfCrossings.Posed(doing, SelfCrossings.Board, dt);
                }
            }

            // (How far the seen board and the seen blade's tip are from where the pose has them, while one goes through the other.)
            if ((blade || arm) && posedSlot != null && BoardOnSlot(out var on) && Anim.SlotWeaponR != null && slot != null)
            {
                crossBoardOff = Mathf.Max(crossBoardOff, Vector3.Distance(centre, posedSlot.TransformPoint(on)));
                crossTipOff = Mathf.Max(crossTipOff, Vector3.Distance(slot.position + slot.rotation * Vector3.forward * Poser.WeaponLength, Anim.SlotWeaponR.position + Anim.SlotWeaponR.rotation * Vector3.forward * Poser.WeaponLength));
                crossTurnOff = Mathf.Max(crossTurnOff, Vector3.Angle(normal, posedSlot.forward));
            }

            Cross(SelfCrossings.Blade, blade, dt, doing, attack, attack ?? "");
            Cross(SelfCrossings.SwordArm, arm, dt, doing, attack, attack ?? "");
            Cross(SelfCrossings.Board, board, dt, doing, attack, what);
            if (Footing == Footings.GettingUp && footingTime <= GetUpStartFor && (blade || arm || board))
            {
                GetUpStartCrossed += dt;
            }

            if (!blade && !arm && crossBoardOff + crossTipOff > 0f)
            {
                if (SelfCrossings.Log.Count > 0 && SelfCrossings.Log.Count < 60)
                {
                    SelfCrossings.Log[SelfCrossings.Log.Count - 1] += $" [the seen board up to {100f * crossBoardOff:0} cm and {crossTurnOff:0} degrees from its pose, the seen tip {100f * crossTipOff:0} cm]";
                }

                crossBoardOff = crossTipOff = crossTurnOff = 0f;
            }
        }

        private static bool Inside(Collider solid, Vector3 p) => (solid.ClosestPoint(p) - p).sqrMagnitude < 1e-8f;

        /// <summary>Does the line from <paramref name="from"/> to <paramref name="to"/> pass through the board's disc.</summary>
        private static bool ThroughBoard(Vector3 from, Vector3 to, Vector3 centre, Vector3 normal, float radius)
        {
            float a = Vector3.Dot(from - centre, normal), b = Vector3.Dot(to - centre, normal);
            if (a * b > 0f || Mathf.Abs(a - b) < 1e-6f)
            {
                return false;
            }

            Vector3 at = Vector3.Lerp(from, to, a / (a - b));
            return (at - centre).sqrMagnitude < radius * radius;
        }

        private void Cross(int kind, bool crossing, float dt, string doing, string attack, string what)
        {
            if (crossing)
            {
                SelfCrossings.Crossed(doing, kind, dt, attack);
                if (crossFor[kind] <= 0f)
                {
                    crossDoing[kind] = doing;
                    crossWhat[kind] = what;
                    crossAt[kind] = Fallen ? footingTime : -1f;
                }

                crossFor[kind] += dt;
                return;
            }

            if (crossFor[kind] >= 0.05f)
            {
                SelfCrossings.Stretches++;
                SelfCrossings.Stretch(crossDoing[kind], kind, crossFor[kind], FightUnit != null ? FightUnit.name : "?");
                string said = $"{SelfCrossings.Names[kind]} for {crossFor[kind]:0.00} s, as seen; it began {crossDoing[kind]}{(crossWhat[kind].Length > 0 ? " (" + crossWhat[kind] + ")" : "")}{(crossAt[kind] >= 0f ? $", {crossAt[kind]:0.00} s into it" : "")}";
                if (SelfCrossings.Log.Count < 60)
                {
                    SelfCrossings.Log.Add((FightUnit != null ? FightUnit.name : "?") + ": " + said);
                }

                if (Diagnostics.PlaytestTrace.Enabled)
                {
                    Diagnostics.PlaytestTrace.Note(FightUnit, "cross", said);
                }
            }

            crossFor[kind] = 0f;
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
            for (int f = 0; f < 2; f++)
            {
                // (His knees over the ground, as seen: the middle of the knee, where the shin begins.)
                var shin = f == 0 ? PartId.ShinL : PartId.ShinR;
                if (!Rig.IsSevered(shin))
                {
                    float up = Rig.Part(shin).transform.position.y - groundLevel;
                    GetUpKneeSeenLowest = Mathf.Min(GetUpKneeSeenLowest, up);
                    GetUpKneeSeenInGround += up < GetUp.KneeInGround * Size ? dt : 0f;
                }
            }

            // (Its shield over the ground, in the get-up itself: as posed, and as the physical body has it.)
            var shield = Kit != null ? Kit.Off : null;
            if (Footing == Footings.GettingUp && shield != null && shield.IsShield)
            {
                if (Anim.SlotWeaponL != null)
                {
                    float gap = PhysicsBattler.Motion.GroundClamp.Gap(Anim.SlotWeaponL, shield, Kit.Scale, Poser.GroundY);
                    GetUpShieldPosedLowest = Mathf.Min(GetUpShieldPosedLowest, gap);
                }

                if (Rig.SlotWeaponL != null && !Rig.IsSevered(PartId.HandL))
                {
                    float gap = PhysicsBattler.Motion.GroundClamp.Gap(Rig.SlotWeaponL, shield, Kit.Scale, Poser.GroundY);
                    if (gap < GetUpShieldSeenLowest)
                    {
                        GetUpShieldSeenLowest = gap;
                        GetUpShieldLowestAt = footingTime;
                    }

                    GetUpShieldInFloorFor += gap < -0.02f * Size ? dt : 0f;
                }
            }

            var slot = Rig.SlotWeaponR;
            if (stance != Stances.Open && slot != null)
            {
                Vector3 along = slot.rotation * Vector3.forward;
                GetUpBladeSeenIn += SeenInside(slot.position + along * (0.15f * Size), slot.position + along * Poser.WeaponLength, BladeSolids) ? dt : 0f;
            }
        }

        private void BeginGetUp(bool onlySetOut = false)
        {
            RagdollArmsPass();
            HeldSolid(false);
            GetUpArmSeenIn = GetUpBladeSeenIn = 0f;
            GetUpKneeSeenLowest = 9f;
            GetUpKneeSeenInGround = 0f;
            GetUpEndJump = GetUpEndJumpAt = 0f;
            GetUpEndJumpPart = "";
            endSeen = 0;
            GetUpShieldPosedLowest = GetUpShieldSeenLowest = 9f;
            GetUpShieldLowestAt = GetUpShieldInFloorFor = 0f;
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

                // (The start he gathered himself for is the start he gets up from, unless he has plainly rolled off it
                // since: his arms and legs were drawn to that start's first pose. Chosen afresh, a fighter whose hips
                // rolled a little past the line while gathering was taken up by another start, a hand or foot over a
                // metre from where it had just been drawn to: a forearm at 25 m/s in a fight of six.)
                if (onlySetOut)
                {
                    gathered = true;
                    gatheredKind = kind;
                    gatheredSide = getUp.Side;
                }
                else if (gathered)
                {
                    gathered = false;
                    if (KeepsTheStartHeGatheredFor && !catching && (kind != gatheredKind || getUp.Side != gatheredSide))
                    {
                        bool still = gatheredSide != 0f ? Mathf.Abs(front.y) < OnSideWithin + GatheredStartHoldsFor
                            : gatheredKind == GetUp.Kind.FaceUp ? front.y > OnSideWithin - GatheredStartHoldsFor
                            : front.y < GatheredStartHoldsFor - OnSideWithin;
                        if (still)
                        {
                            kind = gatheredKind;
                            getUp.Side = gatheredSide;
                            GetUpStartsKept++;
                        }
                    }
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

            // (How he was moving: the pose his body had a step ago. Only for the get-up that takes him up, not for one
            // set out for the gather.)
            takenUpMoving = false;
            getUp.LayBefore = null;
            if (!onlySetOut && GetUp.TakesOverAsHeMoves && fallBody && Upper != null && bodySeen >= bodyPoses.Length)
            {
                // (The oldest of the poses kept, and the time from it to the newest: the steps taken since.)
                int oldest = (bodyAt + 1) % bodyPoses.Length;
                getUp.LayBefore = bodyPoses[oldest];
                getUp.LayBeforeDt = 0f;
                for (int k = 1; k < bodyPoses.Length; k++)
                {
                    getUp.LayBeforeDt += bodySteps[(oldest + k) % bodyPoses.Length];
                }
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
            string RouteOf() => catching && NaturalGetUps ? (caughtSitting ? "from sitting, its fall caught" : "from a knee and a hand, its fall caught") : !NaturalGetUps ? (kind == GetUp.Kind.FaceDown ? "from its front, as before" : "from its back, as before")
                : (getUp.Side != 0f ? (getUp.Side > 0f ? "from its right side" : "from its left side") : kind == GetUp.Kind.FaceDown ? "from its front" : "from its back") + (GetUp.FromRecordings ? ", as recorded" : "");
            GetUpRoute = RouteOf();
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

            lying.Capture(Rig);
            if (!onlySetOut && GetUp.TakesOverAsHeMoves && NaturalGetUps && fallBody && Upper != null)
            {
                // (Taken up as he moves: the get-up reads the body he has, not the pose his fall was drawing him to.
                // It reads where his sword hand lies off the pose as it begins; a fall caught on a knee is taken up
                // with that pose a long way from his body, and his sword hand was thrown from the one to the other in
                // the first step, at 15 m/s in a fight of six.)
                lying.Apply(Anim, false);
            }

            // (A weapon is in its right hand with a shield or without: laid clear of its body as it gets up. With no
            // shield its free hand is the one that goes to the ground, and the sword stays up before it.)
            getUp.HoldsMain = stance != Stances.Open;
            getUp.BladeLength = stance != Stances.Open ? Poser.WeaponLength : 0f;
            getUp.HoldsOff = stance == Stances.Shield;
            getUp.Loadout = GetUpsKnowWhatIsHeld ? Kit : null;
            Vector3 position = default;
            float yaw = 0f;

            // (One way of getting up set out: which hand it posts on is known before it is placed, as that says which
            // foot it plants, and so where it stands up; then its keys are made from how it lies.)
            void SetOut(GetUp.Kind way, float side, int post)
            {
                getUp.Side = side;
                getUp.PostWith = post;
                GetUp.Place(way, pelvis.position, headward, body.SizeFactor, NaturalGetUps, threat, out position, out yaw, out GetUpTurned, getUp.SitsBack, getUp.StandsAhead(way));
                position.y = groundLevel;
                getUpRoot.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
                getUpStance.Plant(position, yaw);
                getUp.Begin(way, lying);
            }

            int byRule = stance == Stances.Sword ? PhysicsBattler.Motion.Stance.Left : -1;
            float hipsSide = getUp.Side;
            GetUpStartChose = "";
            if (GetUpStartNearest && NaturalGetUps && !catching)
            {
                // (The plan "From how he lies to how he gets up", step 2: every start he could use is set out, and the
                // one his hands, elbows, knees and feet are nearest to is taken. The ways: the one his hips say; from
                // a side too if his hips are turned at all that way; and each posting on either hand, but for a sword
                // alone, whose free hand is the one that goes to the ground.)
                var ways = new List<(GetUp.Kind way, float side, int post)>();
                float rolled = (pelvis.rotation * Vector3.right).y;
                var kinds = new List<(GetUp.Kind way, float side)> { (kind, hipsSide) };
                if (hipsSide == 0f && Mathf.Abs(front.y) < SideConsideredWithin && Mathf.Abs(rolled) > 0.2f)
                {
                    kinds.Add((GetUp.Kind.FaceDown, rolled < 0f ? 1f : -1f));
                }

                foreach (var (way, side) in kinds)
                {
                    if (byRule >= 0)
                    {
                        ways.Add((way, side, byRule));
                    }
                    else
                    {
                        // (The hand the stance says, and the other one.)
                        ways.Add((way, side, -1));
                        ways.Add((way, side, getUpStance.Lead));
                    }
                }

                // (Each is set out from the pose as it was: a get-up takes things from it as it begins (where his sword
                // hand lies, for one). Left on the last start weighed, that hand was pinned where that start had it
                // and thrown there at 59 m/s.)
                startAsWas.Capture(Anim);
                int best = 0;
                float bestCost = float.MaxValue, ruleCost = 0f;
                for (int w = 0; w < ways.Count; w++)
                {
                    startAsWas.Apply(Anim, false);
                    SetOut(ways[w].way, ways[w].side, ways[w].post);
                    getUp.PoseAt(getUp.BlendIn);
                    float cost = StartCost();
                    ruleCost = w == 0 ? cost : ruleCost;
                    // (Another way has to be nearer by a margin: of two as near, the one his hips say.)
                    if (cost < bestCost - (w == 0 ? 0f : StartMargin * Size))
                    {
                        bestCost = cost;
                        best = w;
                    }
                }

                kind = ways[best].way;
                startAsWas.Apply(Anim, false);
                SetOut(kind, ways[best].side, ways[best].post);
                GetUpRoute = RouteOf();
                GetUpStartChose = best == 0 ? $"the start his hips say was the nearest of {ways.Count} ({bestCost:0.00} m to go)"
                    : $"chosen of {ways.Count} starts{(ways[best].post >= 0 && byRule < 0 ? ", posting on his " + (ways[best].post == PhysicsBattler.Motion.Stance.Left ? "shield-side" : "sword-side") + " hand" : "")}: {bestCost:0.00} m to go, against {ruleCost:0.00} m for the start his hips say";
                StartsChosen += best == 0 ? 0 : 1;
                StartsWeighed++;
            }
            else
            {
                SetOut(kind, hipsSide, byRule);
            }

            if (onlySetOut)
            {
                // (For the gather: the get-up is set out so that its start can be posed. He is not taken up yet.)
                GetUps--;
                return;
            }

            // (Where it will stand: its walk placed there, the feet let go to land on their spots.)
            Loco.Place(position, yaw);
            Poser.ReleaseFeet();
            if (fallBody && Upper != null)
            {
                // (Taken up where it lies: the pose put on how it lies, and the body held to it again. After the
                // get-up's keys are made: making them leaves the pose standing, and a body taken up before that was
                // carried to its feet for one step and back. Seen in Unity on 14 November, at every get-up.)
                lying.Apply(Anim, false);
                Upper.TakeUp();
                takenUpMoving = GetUp.TakesOverAsHeMoves && NaturalGetUps;
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
            GetUpStartCrossed = GetUpStartHandL = GetUpStartHandR = GetUpStartFastest = 0f;
            GetUpStartFastestPart = "";
            startKnown = false;
            TakeOverJump = TakeOverJumpAt = TakeOverSpeed = 0f;
            TakeOverJumpPart = "";
            for (int i = 0; i < PartInfo.All.Length; i++)
            {
                // (The speed each part of the fallen body had as it was taken up: what the first step is measured against.)
                var id = PartInfo.All[i];
                startSpeedKnown[i] = fallBody && Upper != null && Upper.Holds(id);
                startSpeed[i] = startSpeedKnown[i] ? Upper.SpeedOf(id) : Vector3.zero;
                jumpWas[i] = Rig.Part(id).transform.position;
                TakeOverSpeed = Mathf.Max(TakeOverSpeed, startSpeed[i].magnitude);
            }

            TakeOverTook = getUp.TookOverIn;
            TakeOverFrom = getUp.TookOverFrom;
            GetUpBegan?.Invoke(this);
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
            StepUncover(dt);
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
            // (What he shows of an impact he is in, for the meter: read off the pose as it was last shown.)
            ImpactMeter.Sample(this, Time.time);
            if (Upper != null)
            {
                Upper.HeldMayStop = Footing == Footings.Standing || (UpperBodyArticulation.ShieldStoppedOffFeet && Footing == Footings.Down);
            }

            MeasureOwnCrossings(dt);
            MeasureHeldShake(dt);
            StepUncover(dt);
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
                if (StumbleSteps.On && pendingImpulse.sqrMagnitude > 0f)
                {
                    // (Thrown by a blow: what comes of it is his legs' to decide.)
                    Vector3 thrownBy = pendingKnock;
                    pushAt = -1f;
                    ThrowBody(thrownBy, pendingPoint, pendingImpulse, "a blow");
                }
                else
                {
                    Loco.Push(pendingKnock, !CombatFeel.LegsDecide && clock - thrownAt < 0.5f);
                }

                pendingImpulse = Vector3.zero;
                pushAt = -1f;
                pendingKnock = Vector3.zero;
            }

            HurrySideHit();
            Vector3 asked = want;
            // (Stand and step: near its enemy the steering's small velocity is not walked in slow motion; it stands until
            // a step's worth is asked, and steps it.)
            Loco.KeptUp = WalksOn;
            if (Locomotor.WalksOn && WalksOn && want.sqrMagnitude > 0.01f)
            {
                // (Told to walk on: it is walked as asked at any pace, not stood and stepped. See Locomotor.WalksOn.)
                Pacer.Forget();
                Loco.Paced = false;
            }
            else if (Loco.Legs != Locomotor.Mode.OneShot && Loco.Guarding)
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

            // (An enemy near: a turn to face him is a hurried one, see Locomotor.QuickTurns.)
            Loco.Hurried = apart > 1e-3f && apart < Locomotor.QuickTurnWithin * Size;
            if (Stagger.Active)
            {
                StepStagger(dt);
            }
            else
            {
                Loco.Step(dt, want, yaw);
            }

            CarryBy(Loco.SnappedThisStep);
            PullOnTheMove();
            if (Combo != null)
            {
                Combo.Step(dt);
            }

            // (A built blow's own clock: stopped with the body in a hit stop, as a clip's is.)
            Built?.Step(dt);

            // (Its own strike taking the sword arm from the parry: the parry's arms and trunk, as they were last step,
            // faded into the strike's, not dropped in a frame.)
            if (Defends && stance == Stances.Sword && Parry != null && Striking && !parryWaited)
            {
                CaptureHandOff();
            }

            // (A strike alone follows its target too, as far as its pull goes.)
            if (Follow != null && (Combo == null || !Combo.Playing) && Loco.Current == Locomotor.Mode.OneShot && Loco.Shot != null && Loco.Shot.ContactFrame >= 0 && Loco.Shot != sweepClip)
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
                PoseBuilt(dt);
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
                    PoseBuilt(dt);
                }
                else if (GingaHasArms(dt))
                {
                    // (Standing in the ginga with no blow coming: the arms are the ginga's own, not the sword's guard.
                    // The parry takes the sword up again from where the ginga has it.)
                    parryWaited = true;
                    PoseBuilt(dt);
                }
                else
                {
                    if (parryWaited)
                    {
                        Parry.Resync();
                        parryWaited = false;
                    }
                    else if (parryHeldAt >= 0f && clock - parryHeldAt > SwordLeftFor)
                    {
                        // (Its guard was not held a while (it was down, or stunned): taken up from where the arm is.)
                        TakeUpSword();
                    }

                    parryHeldAt = clock;
                    Parry.Skill = ParrySkillNow;
                    if (gingaArms > 0f)
                    {
                        // (Going between the ginga's arms and the sword's guard: the guard is solved, and the arms shown
                        // part of the way back to where the ginga had them.)
                        for (int i = 0; i < ArmParts.Length; i++)
                        {
                            gingaArmWas[i] = Anim.Part(ArmParts[i]).localRotation;
                        }
                    }

                    // (A fake he dodges is not parried.)
                    Parry.Step(dt, FlinchAtFake() ? (BlowRead?)null : Blow, opponent);
                    if (gingaArms > 0f)
                    {
                        float w = gingaArms * gingaArms * (3f - 2f * gingaArms);
                        // (The made ginga: the left arm alone goes back to where the ginga has it; the sword arm is its guard's.)
                        int arms = Loco.StandsInPattern ? 3 : ArmParts.Length;
                        for (int i = 0; i < arms; i++)
                        {
                            var part = Anim.Part(ArmParts[i]);
                            part.localRotation = Quaternion.Slerp(part.localRotation, gingaArmWas[i], w);
                        }
                    }

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
                PoseBuilt(dt);
            }

            Recover(dt);
            MarkArms("the recovery");
            Poise = Mathf.Min(MaxPoise, Poise + PoiseBack * MaxPoise * dt);
        }

        private static readonly PartId[] ArmParts = { PartId.UpperArmL, PartId.ForearmL, PartId.HandL, PartId.UpperArmR, PartId.ForearmR, PartId.HandR };
        private readonly Quaternion[] gingaArmWas = new Quaternion[6];
        private float gingaArms;

        /// <summary>How long the arms take to go between the ginga's own and the sword's guard (seconds).</summary>
        public const float GingaArmsBlend = 0.3f;

        /// <summary>
        /// Standing in a stand that steps (the ginga), with no blow coming at it, the arms are that stand's own: they
        /// swing with it, where a sword's guard would hold them still on its line. True when they are wholly the
        /// ginga's this step; going over, the guard is still solved and the arms shown between the two.
        /// </summary>
        private bool GingaHasArms(float dt)
        {
            bool wanted = Loco.StandSteps && Loco.Current == Locomotor.Mode.Idle && !Blow.HasValue;
            gingaArms = Mathf.MoveTowards(gingaArms, wanted ? 1f : 0f, dt / GingaArmsBlend);
            // (In the ginga made of its pattern the sword stays in its guard, ready: only the free arm is the ginga's.)
            return gingaArms >= 1f && !Loco.StandsInPattern;
        }

        /// <summary>The built blow it is striking with, or struck with last (the plan "Aimed attacks"); null before any.</summary>
        public BuiltBlow Built { get; private set; }

        /// <summary>
        /// A built blow begun (the plan "Aimed attacks"): its sword arm is the blow's from now to the end of its way
        /// back, and its blade is judged as a strike clip's is. False, and nothing begun, when it is busy, has no
        /// strikes, or the blow can't begin.
        /// </summary>
        public bool StartBuilt(BuiltBlow blow)
        {
            if (blow == null || Busy || Strikes == null || !blow.Begin())
            {
                return false;
            }

            Built = blow;
            Strikes.Built = blow;
            return true;
        }

        /// <summary>The sword arm as its built blow has it, after the body's own pose and its sway.</summary>
        private void PoseBuilt(float dt)
        {
            if (Built != null && Built.Playing)
            {
                Built.Pose(dt);
                MarkArms("the built blow");
            }
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
        /// <summary>For the reports: told each stage of the pose as it is done (the fighter, the stage's name), to look at where the arms are after it.</summary>
        public static System.Action<ClipFighter, string> ArmStageProbe;

        private void MarkArms(string stage)
        {
            ArmStageProbe?.Invoke(this, stage);
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
                GiveWayToSword(dt, su, sf, sh);
                KeepShieldOff(su, sf, sh, TwoBoneIK.BendWay(su, sf, sh));
            }
            else
            {
                laneKnown = false;
                laneGive = Vector3.zero;
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
        /// What he holds moves smoothly (18 November; Combat Feel "What He Holds Moves Smoothly"). The shield hand, and
        /// the sword hand of a sword and shield while it is not striking, are not put where the pose has them each
        /// step: they follow it, speeding up and settling in (a critically damped spring, against where he stands), so
        /// nothing the pose does to them shows as a jump. Measured before it: the pose itself moved a standing or
        /// walking fighter's board by 2 to 12 cm in single steps as it was raised to a blow, knocked, taken up by the
        /// walk from standing still, or handed from one posture to the next, and the physical arm then chased it;
        /// with the physical bodies touching nothing at all the tremble was the same. Quicker while the shield has
        /// somewhere to be (raised to a blow, driven in a bash): <see cref="HeldSettleQuick"/> against
        /// <see cref="HeldSettleRate"/> (1/s; three over it is about the time it takes to arrive). Off: as before.
        /// </summary>
        public static bool HeldSettles = true;
        public static float HeldSettleRate = 26f, HeldSettleQuick = 60f;

        private readonly Vector3[] heldAt = new Vector3[2], heldAtV = new Vector3[2], heldAsked = new Vector3[2], heldTurnV = new Vector3[2];

        /// <summary>A turn as a vector: its axis, as long as its angle (radians, the short way round).</summary>
        private static Vector3 TurnOf(Quaternion q)
        {
            q.ToAngleAxis(out float angle, out Vector3 axis);
            if (float.IsNaN(axis.x) || float.IsInfinity(axis.x))
            {
                return Vector3.zero;
            }

            if (angle > 180f)
            {
                angle -= 360f;
            }

            return axis * (angle * Mathf.Deg2Rad);
        }

        private static Quaternion FromTurn(Vector3 turn)
        {
            float angle = turn.magnitude;
            return angle > 1e-6f ? Quaternion.AngleAxis(angle * Mathf.Rad2Deg, turn / angle) : Quaternion.identity;
        }

        private readonly Quaternion[] heldTurned = { Quaternion.identity, Quaternion.identity };
        private readonly bool[] heldKnown = new bool[2];
        private readonly float[] heldShare = new float[2];

        /// <summary>For the reports: the furthest a hand was held behind where the pose had it by <see cref="SettleHeld"/> (metres), shield hand and sword hand.</summary>
        public readonly float[] HeldBehindMost = new float[2];

        /// <summary>The hands that hold something follow their pose smoothly (<see cref="HeldSettles"/>): 0 the shield hand, 1 the sword hand.</summary>
        private void SettleHeld(float dt)
        {
            // (Against where he stands and the way he faces, not against his chest: what his trunk does at once, rocked by
            // a blow or turned by a posture, the hands then follow too, where against the chest they were carried with it.)
            Vector3 rootAt = Loco.Position + Vector3.up * Poser.GroundY;
            Quaternion rootTurn = Quaternion.Euler(0f, Loco.Facing, 0f), rootBack = Quaternion.Inverse(rootTurn);
            for (int a = 0; a < 2; a++)
            {
                bool left = a == 0;
                var handId = left ? PartId.HandL : PartId.HandR;
                Transform upper = Anim.Part(left ? PartId.UpperArmL : PartId.UpperArmR), fore = Anim.Part(left ? PartId.ForearmL : PartId.ForearmR), hand = Anim.Part(handId);
                // (Which hands: the shield's always; the sword's of a sword and shield while it is neither in a blow nor held
                // to one. A lone sword's arm is its parry's, which must be where it is asked when it is asked.)
                bool mine = HeldSettles && dt > 0f && stance == Stances.Shield && !Poser.TwoHanded && !Rig.IsSevered(handId) && roseAt < 0f
                    && (left || !(Striking || clock < weaponArmHeldUntil));
                Vector3 asked = rootBack * (hand.position - rootAt);
                Quaternion turned = rootBack * hand.rotation;
                // (Taken up over a moment, and let go over a shorter one: a sword hand goes to its blow from where it had
                // settled to, not from where the pose had it.)
                heldShare[a] = Mathf.Clamp01(heldShare[a] + (mine ? dt / 0.12f : -dt / 0.05f));
                if (heldShare[a] <= 0f || !heldKnown[a])
                {
                    // (Not held back: where the pose has it, at the speed the pose moves it, so it is taken up from there.)
                    heldAtV[a] = heldKnown[a] && dt > 0f ? Vector3.ClampMagnitude((asked - heldAsked[a]) / dt, 8f) : Vector3.zero;
                    heldAt[a] = heldAsked[a] = asked;
                    heldTurned[a] = turned;
                    heldTurnV[a] = Vector3.zero;
                    heldKnown[a] = true;
                    continue;
                }

                heldAsked[a] = asked;
                float urgent = left ? Mathf.Max(Mathf.Max(Mathf.Clamp01(interceptWeight), Bashing ? 1f : 0f), GivingWay) : 0f;
                float w = Mathf.Lerp(HeldSettleRate, HeldSettleQuick, urgent);
                CriticalSpring(ref heldAt[a], ref heldAtV[a], asked, w, dt);
                // (Its turn the same way: how far it is turned from where it is asked, as a turn, settled like the place.)
                Vector3 off = TurnOf(heldTurned[a] * Quaternion.Inverse(turned));
                CriticalSpring(ref off, ref heldTurnV[a], w, dt);
                heldTurned[a] = FromTurn(off) * turned;
                float share = heldShare[a] * heldShare[a] * (3f - 2f * heldShare[a]);
                HeldBehindMost[a] = Mathf.Max(HeldBehindMost[a], share * Vector3.Distance(heldAt[a], asked));
                Vector3 pole = TwoBoneIK.BendWay(upper, fore, hand);
                TwoBoneIK.Solve(upper, fore, hand, rootAt + rootTurn * Vector3.Lerp(asked, heldAt[a], share), pole);
                hand.rotation = rootTurn * Quaternion.Slerp(turned, heldTurned[a], share);
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
            SettleHeld(dt);
            // (Last of what poses his arms: put before the cover and the guard, they put his hands back where they
            // had them, and his arms went out 25 to 64 degrees at the shoulder with his hands hardly moving.)
            StepArmsOut(dt);

            if (Authority != null)
            {
                // (Through a strike and a moment after it the weapon arm is held exactly to its pose, kinematic, as
                // the fight holds it in a blow: pinned, the physical arm, and the weapon seen on it and swept for
                // hits, trailed a 16 m/s swing by tens of centimetres and passed by what the pose struck.)
                if (Strikes != null && ((Loco.Current == Locomotor.Mode.OneShot && Loco.Shot != null && Loco.Shot.ContactFrame >= 0) || (Built != null && Built.Playing)))
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
            RagdollArmsPass();
            HeldSolid(false);
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
                    HeldSolid(RagdollsHoldSolidThings);
                    RagdollArmsToStop();
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
            windUpsBehind = -1;
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
        public bool Striking => (Loco.Current == Locomotor.Mode.OneShot && Loco.Shot != null && Loco.Shot.ContactFrame >= 0) || (Combo != null && Combo.Playing) || (Built != null && Built.Playing);

        /// <summary>Busy with something it can't break off to strike: a strike, a reaction, down.</summary>
        public bool Busy => Fallen || Striking || Loco.Current == Locomotor.Mode.OneShot || Stumbling;

        /// <summary>Thrown back stumbling and not yet on its feet again (<see cref="CombatFeel.StumbleRecovery"/>): it can't strike.</summary>
        public bool Stumbling => Stagger.Active || (thrownAt >= 0f && clock - thrownAt < CombatFeel.StumbleRecovery);

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

        /// <summary>The attack it made last (the plan "Aimed attacks": <see cref="AttackMaker"/>); null before any.</summary>
        public MadeAttack Made { get; private set; }

        /// <summary>A point of the world in its own frame as it stands now, in a clip's units (leg lengths; x to its right, y over the ground, z forward).</summary>
        public Vector3 InOwnFrame(Vector3 point)
        {
            Vector3 off = Quaternion.Euler(0f, -Loco.Facing, 0f) * (point - Loco.Position);
            return new Vector3(off.x, point.y - groundLevel, off.z) / Mathf.Max(0.1f, Loco.Leg);
        }

        /// <summary>
        /// An attack made for a point (the plan "Aimed attacks"): of its strikes' scores (or the one named), the one
        /// whose blade comes nearest the point as made for it, made in <paramref name="style"/> to land
        /// <paramref name="landIn"/> seconds from now (0: in its own time), and begun. Null, and nothing begun, with no
        /// strikes.
        /// </summary>
        /// <param name="counts">Which parts of the target count as the one the blade is for (null: not looked at): an attack whose blade would meet another part first is passed over for one whose way is open.</param>
        public MadeAttack StrikeMade(ClipFighter target, Vector3 point, in AttackStyle style, float landIn = 0f, string named = null, System.Func<PartId, bool> counts = null, string legs = null, AttackScore given = null, float mostTurn = 0f)
        {
            if (Strikes != null)
            {
                // (As heavy as any: a blow of a class sets its own after it is made, AttackClasses.)
                Strikes.Power = 1f;
            }

            // (Asked for as a posture, PostureMaker: the attack is made as any is, and only its beginning thrown.)
            postureNow = PostureMaker.Take(out postureFeint, out postureLunge);
            if (postureNow)
            {
                // (His own levers: how deep his feints go and how far his fake lunges carry him, against everyone's.)
                postureFeint = Mathf.Min(postureFeint * FighterLevers.FeintOf(Loco.Own), 1f / Mathf.Max(0.05f, PostureMaker.FeintDepth));
                postureLunge *= FighterLevers.LungeOf(Loco.Own);
            }
            // (A fake cut short of an attack is cut only from one whose wind-up takes the sword behind him, if he has one.)
            fakeFromBehind = postureNow && postureFeint > 0f && postureLunge <= 0f && PostureMaker.CutShort && PostureMaker.FakesWindBehind && named == null && given == null && HasWindUpBehind();
            if (AttackOnTheMove.On && landIn <= 0f && legs == null && given == null && !(postureNow && postureLunge > 0f && !PostureMaker.LungesOnTheMove))
            {
                // (Walking, the blow is thrown on the move: the walk goes on under it.)
                var moved = StrikeOnTheMove(target, point, style, named);
                if (moved != null)
                {
                    KeepOff(target);
                    return moved;
                }
            }

            var aim = new AttackAim { Aimed = true, Target = InOwnFrame(point), LandIn = landIn, MostTurn = mostTurn, Sweeps = given != null && mostTurn > 0f && TurnSlashes.SweepsFree };
            // (A style's manner laid over him: the blow is fitted for the body as the manner will have it.)
            if (Loco.MannerForBlow(out var mannerFrom, out var mannerTo, out var mannerDials))
            {
                aim.Mannered = true;
                aim.MannerFrom = mannerFrom;
                aim.MannerTo = mannerTo;
                aim.MannerDials = mannerDials;
            }

            // (From where he is, the plan "Fight Rhythm from Recordings", step 3: the way he is going is carried into
            // the attack, and a foot caught in the air finishes its step.)
            bool going = AttackMaker.FromAnyPose && Loco.Velocity.magnitude > 0.1f * Size;
            aim.Eased = AttackMaker.FromAnyPose;
            if (going)
            {
                Vector3 own = Quaternion.Euler(0f, -Loco.Facing, 0f) * Loco.Velocity;
                aim.Carry = new Vector3(own.x, 0f, own.z) / Mathf.Max(0.1f, Loco.Leg);
                if (Loco.Current == Locomotor.Mode.Cycle)
                {
                    aim.AirL = Loco.Down[0] < 0.5f ? Mathf.Clamp(Loco.SwingLeft(0), 0.06f, 0.4f) : 0f;
                    aim.AirR = Loco.Down[1] < 0.5f ? Mathf.Clamp(Loco.SwingLeft(1), 0.06f, 0.4f) : 0f;
                }
            }

            var hold = Poser.Hold();
            madePlans.Clear();
            if (given != null)
            {
                // (A score put together for the occasion, a cut on a turn's legs: that one.)
                var plan = AttackMaker.Make(given, style, aim, hold, build: false);
                if (plan != null)
                {
                    madePlans.Add(plan);
                }
            }

            foreach (var clip in given != null ? noStrikes : StrikeClips)
            {
                // (With none named: of the plain blows, made with the feet on the ground. The leaps and the spins are asked for by name.)
                var score = AttackScore.Of(clip);
                if (score == null || (named != null ? clip.Name != named : !score.Plain) || (fakeFromBehind && !score.WindsBehind))
                {
                    continue;
                }

                foreach (var under in StrikeClips)
                {
                    // (On its own legs, or on another plain blow's: a thrust recorded standing on a cut's lunge, a cut
                    // recorded lunging on the thrust's standing legs. With no legs named: whichever fits the distance.)
                    // (A leap or a spin keeps its own legs, whatever legs were asked for.)
                    var legScore = AttackScore.Of(under);
                    bool own = under == clip;
                    if (legScore == null || (!own && (!legScore.Plain || !score.Plain)) || (legs == null ? !own && !MadeBorrowsLegs : legs == "own" || !score.Plain ? !own : under.Name != legs))
                    {
                        continue;
                    }

                    var plan = AttackMaker.Make(own ? score : score.OnLegsOf(legScore), style, aim, hold, build: false);
                    if (plan != null)
                    {
                        // (Its own legs before another's that fit no better.)
                        plan.Cost += own ? 0f : BorrowedLegsCost;
                        madePlans.Add(plan);
                    }
                }
            }

            // The plan that costs least (what it takes to bring that score's blade onto the point: the maker's own
            // count); with the target as it stands, the least of those whose way is open (what its blade would meet
            // first is a part it was made for), looked for among the four best.
            madePlans.Sort((a, b) => a.Cost.CompareTo(b.Cost));
            MadeAttack best = null;
            for (int i = 0; i < madePlans.Count && i < 4; i++)
            {
                AttackMaker.Build(madePlans[i]);
                if (counts == null || target == null)
                {
                    best = madePlans[i];
                    break;
                }

                FirstMet(madePlans[i], target, counts);
                if (madePlans[i].WayOpen)
                {
                    best = madePlans[i];
                    break;
                }
            }

            if (best == null && madePlans.Count > 0)
            {
                best = madePlans[0];
            }

            madePlans.Clear();
            if (best == null)
            {
                return null;
            }

            KeepOff(target);
            Made = best;
            // (The turn is in the made clip's own root: the walk plays it as it is.)
            Loco.KeepsSpeedAtNextSwitch = AttackMaker.FromAnyPose;
            if (postureNow)
            {
                var cutFrom = best.Clip;
                var posture = PostureMaker.Make(best, postureFeint, postureLunge, out float turnsAt);
                if (posture != null)
                {
                    FakeBegun(posture, cutFrom, turnsAt);
                    best.Clip = posture;
                    best.Feint = postureFeint;
                    best.FakeLunge = postureLunge;
                    best.ContactAt = turnsAt;
                    best.DriveAt = turnsAt;
                    best.Held = 0f;
                    Postures++;
                    if (postureFeint > 0f)
                    {
                        // (A feint's arms are the attack's own: it is played as an attack is.)
                        Poser.PlayMade(posture, point, 0f);
                    }
                    else
                    {
                        // (A fake lunge alone: his arms stay in his guard, which goes on over it.)
                        Loco.PlayAimed(posture, 1f, 0f, 0f);
                    }

                    return best;
                }
            }

            Poser.PlayMade(best.Clip, point, 0f);
            return best;
        }

        private static readonly List<LocoClip> noStrikes = new List<LocoClip>();

        // (The fake being made is cut only from attacks whose wind-up takes the sword behind him.)
        private bool fakeFromBehind;
        private int windUpsBehind = -1;

        /// <summary>He has a plain attack whose wind-up takes the sword behind him (<see cref="AttackScore.WindsBehind"/>): what his fakes are cut from. Without one, his fakes are cut from any, as they were.</summary>
        public bool HasWindUpBehind()
        {
            if (windUpsBehind < 0)
            {
                windUpsBehind = 0;
                foreach (var clip in StrikeClips)
                {
                    var score = AttackScore.Of(clip);
                    windUpsBehind += score != null && score.Plain && score.WindsBehind ? 1 : 0;
                }
            }

            return windUpsBehind > 0;
        }

        /// <summary>
        /// A cut made while turning round (<see cref="TurnSlashes"/>): the quick turn's legs, <paramref name="degrees"/>
        /// round (+ to his right), under a recorded cut's chest, head and arms, made for a point on his enemy and
        /// begun. Null, and nothing begun: he has no quick turn that way, or the cut is not there.
        /// </summary>
        public MadeAttack TurnSlashMade(ClipFighter target, IReadOnlyList<Vector3> points, float degrees, out int taken, System.Func<int, System.Func<PartId, bool>> counts = null)
        {
            taken = -1;
            var turn = Loco.Guard == null ? null : degrees > 0f ? Loco.Guard.QuickR : Loco.Guard.QuickL;
            var upper = AttackScore.Of(LocoLibrary.Get(degrees > 0f ? TurnSlashes.RightCut : TurnSlashes.LeftCut));
            if (turn == null || upper == null || Mathf.Abs(turn.Turn) < 1f || points == null)
            {
                return null;
            }

            if (TurnSlashes.EndsShort && Mathf.Abs(upper.Facing[upper.Frames.Length - 1]) > 200f)
            {
                // (A cut recorded spinning right round: its carry-through is the spinner's, his arm wrapping behind
                // him as his body goes on round. On a body that stops as the blade lands it is ended short.)
                upper = upper.EndedShort(TurnSlashes.SweepsOn * TurnSlashes.BladeGoes, TurnSlashes.CarriesOn, TurnSlashes.EasesBack);
            }

            var span = Locomotor.Busy(turn);
            var style = TurnSlashes.Style;
            var hold = Poser.Hold();
            float lands = TurnSlashes.LandsAt;
            // (The turn goes evenly through the cut's wind-up and its drive: as much of it is done by the time the drive
            // begins as of the cut's own time to the landing.)
            float driveAt = Mathf.Clamp(upper.Drive / (float)Mathf.Max(1, upper.Contact), 0.2f, 0.75f);
            AttackScore best = null;
            float least = float.MaxValue;
            for (int i = 0; i < points.Count; i++)
            {
                // (The legs go as far round as his enemy is, in steps of 10 degrees. The aim then turns his whole body
                // a little more or less, early, to bring the blade onto its line: a backhand's blade is still to the left
                // of where he faces as it lands, so he is carried a little past his enemy. That much is given back
                // after the blade has landed, so he ends facing him.)
                var aim = new AttackAim { Aimed = true, Target = InOwnFrame(points[i]), Eased = AttackMaker.FromAnyPose, MostTurn = TurnSlashes.AimTurns, Sweeps = TurnSlashes.SweepsFree };
                // (Never more than half way round in all, the aim's own turn counted: a made cut whose body goes past
                // 180 degrees left his feet where they stood and wound his legs round each other (the playtest of 17
                // November, the other put straight behind him). Until that is found, he turns 158 degrees at most on his
                // legs and the rest is eased round as he comes back to his guard.)
                float mostScale = Mathf.Floor((TurnSlashes.MostRound - TurnSlashes.AimTurns) / Mathf.Abs(turn.Turn) * 18f) / 18f;
                float scale = Mathf.Min(mostScale, Mathf.Round(Mathf.Clamp(degrees / turn.Turn, 0.25f, 1.15f) * 18f) / 18f);
                var score = upper.OnLegsOf(AttackScore.OfTurn(turn, span.x, span.y, lands, scale, 0f, driveAt));
                var plan = AttackMaker.Make(score, style, aim, hold, build: false);
                if (plan == null)
                {
                    continue;
                }

                // (Where he is to end: facing his enemy, or as far round as a cut may take him. What the legs' turn and
                // the aim's leave of that is turned after the blade has landed: back, where the aim carried him past;
                // on, where the legs' turn was cut short of it.)
                float ends = Mathf.Sign(degrees) * Mathf.Min(Mathf.Abs(degrees), TurnSlashes.MostRound);
                float after = ends - scale * turn.Turn - plan.Turn;
                if (Mathf.Abs(after) >= 2.5f)
                {
                    var again = upper.OnLegsOf(AttackScore.OfTurn(turn, span.x, span.y, lands, scale, after, driveAt));
                    var replanned = AttackMaker.Make(again, style, aim, hold, build: false);
                    if (replanned != null)
                    {
                        plan = replanned;
                        score = again;
                    }
                }

                // (Of the parts he may cut at, the one the cut reaches with the least done to it: his hips not sunk
                // far for a cut that goes high, nor raised for one that goes low.)
                float cost = plan.Cost + 2f * Mathf.Abs(plan.Drop);
                if (cost < least)
                {
                    least = cost;
                    best = score;
                    taken = i;
                }
            }

            if (best == null)
            {
                return null;
            }

            var made = StrikeMade(target, points[taken], style, 0f, null, counts != null ? counts(taken) : null, null, best, TurnSlashes.AimTurns);
            if (made != null)
            {
                TurnSlashes.Made++;
                TurnSlashesMade++;
                sweepClip = TurnSlashes.SweepsFree ? made.Clip : null;
            }

            return made;
        }

        /// <summary>Cuts it has made turning round.</summary>
        public int TurnSlashesMade;

        /// <summary>The cut made turning round that is under way, when it sweeps free: its hand is not pulled after its target.</summary>
        private LocoClip sweepClip;

        /// <summary>He is told to walk on (to walk up, back away, circle), by what orders him: his walk is walked at any pace, not stepped (<see cref="Locomotor.WalksOn"/>).</summary>
        public bool WalksOn;

        private bool postureNow;
        private float postureFeint, postureLunge;

        /// <summary>
        /// A drop (<see cref="PostureMaker"/>): a sudden dip of the hips, as if loading to spring, over whatever he is
        /// doing. False: he cannot (down, in a clip of his own, his movement not made of its parts, one under way).
        /// </summary>
        /// <summary>
        /// A dodge (<see cref="PostureMaker.DodgeSinks"/>): over whatever he is doing, standing or walking, he ducks
        /// and leans to one side (above 0: his right), his hips going that way, and comes back up. His feet stay
        /// where they are; his shield goes with his chest, keeping its cover. False: he cannot (down, in a clip of
        /// his own, a drop or a dodge under way).
        /// </summary>
        public bool Dodge(int side)
        {
            if (Fallen || Loco.Current == Locomotor.Mode.OneShot)
            {
                return false;
            }

            float leg = Mathf.Max(0.1f, Loco.Leg), to = side >= 0 ? 1f : -1f;
            if (!Loco.Dip(PostureMaker.DodgeSinks * Size / leg, PostureMaker.DodgeLeans, PostureMaker.DodgeDown, PostureMaker.DodgeHeld, PostureMaker.DodgeUp, to * PostureMaker.DodgeAside * Size / leg, to * PostureMaker.DodgeRolls))
            {
                return false;
            }

            Postures++;
            Dodges++;
            return true;
        }

        /// <summary>Dodges thrown.</summary>
        public int Dodges;

        /// <summary>Fakes cut short that it threw; and, of the other's, how many it met as a blow and how many it dodged (for the logs).</summary>
        public int FakesCut, FakesMet, FakesDodged;

        /// <summary>
        /// A posture begun: if it is a fake cut short of an attack (<see cref="PostureMaker.CutShort"/>), its blade is
        /// told so (<see cref="StrikeHits.FakeClip"/>): it strikes nothing, is not heard, and reads to the other as the
        /// attack it is cut from until it is checked, if he is taken in (<see cref="PostureMaker.FakeBelieved"/>).
        /// </summary>
        private void FakeBegun(LocoClip posture, LocoClip cutFrom, float checkedAt)
        {
            if (Strikes == null)
            {
                return;
            }

            bool cut = posture != null && posture.Name.StartsWith("made_fake_");
            Strikes.FakeClip = cut ? posture : null;
            Strikes.FakeOf = cut ? cutFrom : null;
            Strikes.FakeUntil = checkedAt;
            Strikes.FakeFools = cut && (DodgesEvery || PostureMaker.Luck() < PostureMaker.FakeBelieved);
            FakesCut += cut ? 1 : 0;
        }

        private int fakeSerial = -1, fakeTries;
        private bool fakeDodges, fakeAnswered;

        /// <summary>
        /// A dodge away from a blow he sees coming: to the side the blade does not come from (no blow read: a side
        /// drawn). In a gesture of his own (a fake, a feint: not a blow) he lets it go for the dodge
        /// (<see cref="DodgeDropsGesture"/>); it ends over the next step, so the dodge may only take on a later try.
        /// </summary>
        public bool DodgeAway()
        {
            if (DodgeDropsGesture && Posturing && Loco.Current == Locomotor.Mode.OneShot)
            {
                Loco.EndShotNow();
            }

            int side = shieldLuck.Next(2) == 0 ? -1 : 1;
            if (Blow.HasValue)
            {
                // (Away from the side the blade comes from.)
                Vector3 right = Quaternion.Euler(0f, Loco.Facing, 0f) * Vector3.right;
                float from = Vector3.Dot(Blow.Value.ComesFrom, right);
                side = Mathf.Abs(from) > 0.05f ? (from > 0f ? -1 : 1) : side;
            }

            return Dodge(side);
        }

        /// <summary>A fighter who dodges lets a gesture of his own go to do it (on; off: in a gesture he cannot dodge, as before 18 November).</summary>
        public static bool DodgeDropsGesture = true;

        /// <summary>To see the dodges: every fake is taken for a blow and dodged, and in the dance every real blow made out in time is dodged too. A lab switch, off by default.</summary>
        public static bool DodgesEvery;

        /// <summary>
        /// The other's fake, taken for a blow (<see cref="BlowRead.Feint"/>): he meets it with his shield or his sword
        /// as he would the blow (nothing to do here: the read of it is the read of a blow), or, <see cref="PostureMaker.FakeDodged"/>
        /// of the times, he ducks aside from it, once it has come on far enough to be read. True: this one he dodges,
        /// and his guard does not go to meet it.
        /// </summary>
        private bool FlinchAtFake()
        {
            if (!Blow.HasValue || !Blow.Value.Feint)
            {
                return false;
            }

            var blow = Blow.Value;
            if (blow.Serial != fakeSerial)
            {
                fakeSerial = blow.Serial;
                fakeAnswered = false;
                fakeTries = 0;
                fakeDodges = DodgesEvery || (float)shieldLuck.NextDouble() < PostureMaker.FakeDodged;
            }

            float whole = blow.Coming + blow.ToContact;
            bool readable = whole <= 1e-3f || blow.Coming >= CombatFeel.ShieldReadsFrom * whole;
            if (readable && !fakeAnswered)
            {
                if (fakeDodges)
                {
                    bool mine = Posturing;
                    if (DodgeAway())
                    {
                        fakeAnswered = true;
                        FakesDodged++;
                        SayFake("dodged");
                    }
                    else if (!(DodgeDropsGesture && (mine || fakeTries > 0) && ++fakeTries <= 8 && !Fallen))
                    {
                        // (In a blow of his own, or down: he cannot.)
                        fakeDodges = false;
                    }
                }

                if (!fakeDodges)
                {
                    fakeAnswered = true;
                    FakesMet++;
                    SayFake(stance == Stances.Sword ? "went to parry" : "brought his shield to");
                }
            }

            return fakeDodges;
        }

        private void SayFake(string how)
        {
            FakeAnswered?.Invoke(this, how);
            if (PhysicsBattler.Diagnostics.PlaytestTrace.Enabled)
            {
                PhysicsBattler.Diagnostics.PlaytestTrace.Note(FightUnit, "fake", $"{how} a fake he took for a blow");
            }
        }

        /// <summary>A fighter answered a fake he took for a blow: who, and how ("dodged", "brought his shield to", "went to parry"). For the logs and the lab.</summary>
        public static event System.Action<ClipFighter, string> FakeAnswered;

        public bool Drop()
        {
            if (Fallen || Loco.Current == Locomotor.Mode.OneShot)
            {
                return false;
            }

            if (!Loco.Dip(PostureMaker.DropSinks * FighterLevers.DropOf(Loco.Own) * Size / Mathf.Max(0.1f, Loco.Leg), PostureMaker.DropLeans, PostureMaker.DropDown, PostureMaker.DropHeld, PostureMaker.DropUp))
            {
                return false;
            }

            Postures++;
            return true;
        }

        /// <summary>
        /// One of the upper body's short postures (<see cref="PostureMaker"/>): the blade's point dips, flicks aside or
        /// goes once round; or the shield is thrust out and brought back. Over whatever he is doing. False: he cannot.
        /// </summary>
        public bool Bluff(Locomotor.Pulse kind)
        {
            if (Fallen || Loco.Current == Locomotor.Mode.OneShot)
            {
                return false;
            }

            float size = PostureMaker.BluffSize * FighterLevers.BluffOf(Loco.Own);
            bool thrown = kind == Locomotor.Pulse.BladeDip ? Loco.Throw(kind, PostureMaker.BladeDips * size, 0.1f, 0.04f, 0.22f)
                : kind == Locomotor.Pulse.BladeFlick ? Loco.Throw(kind, PostureMaker.BladeFlicks * size, 0.09f, 0.03f, 0.2f)
                : kind == Locomotor.Pulse.BladeCircle ? Loco.Throw(kind, PostureMaker.BladeCircles * size, 0.2f, 0.15f, 0.2f)
                : Loco.Throw(kind, Mathf.Clamp01(PostureMaker.ShieldPressFar * FighterLevers.BluffOf(Loco.Own)), 0.12f, 0.06f, 0.3f);
            if (thrown)
            {
                Postures++;
            }

            return thrown;
        }

        /// <summary>A change of guard: the sword arm carried high (1), as the recordings have it (0) or low (-1).</summary>
        public void GuardAt(int height)
        {
            Loco.GuardLift = FighterLevers.GuardOf(Loco.Own) * (height > 0 ? PostureMaker.GuardHigh : height < 0 ? -PostureMaker.GuardLow : 0f);
        }

        /// <summary>
        /// A fake commits the shield (Combat Feel "A Fake Commits The Shield"): while a posture of his own plays (a
        /// feint, a fake lunge) his shield stays where the posture has it and does not go to meet a blow. That is the
        /// price of a fake, and what makes the opening it leaves a real one. Off: his shield meets a blow in the
        /// middle of his own fake as readily as from his guard, as it did.
        /// </summary>
        public static bool PostureCommitsShield = false;

        /// <summary>
        /// A fake draws no guard (Combat Feel "A Fake Draws No Guard"). A fighter's shield (or his parry) goes to meet
        /// only a real blow: a fake of the other's, which stops short of its drive, does not take it from its cover.
        /// Off: a fake is read as the blow it begins as, and the guard goes to meet it, as it did (in the playtest of
        /// 17 November two thirds of the times a shield left its cover it was for a fake).
        /// </summary>
        public static bool FakesDrawNoGuard = true;

        /// <summary>A posture of his own is playing (a feint, a fake lunge), standing or over his walk.</summary>
        public bool Posturing => Made != null && Made.Posture && Loco.Shot == Made.Clip && (Loco.Current == Locomotor.Mode.OneShot || Loco.ShotOverWalk);

        // ---- An uncovering (the plan's step 7: the shield test) ----

        /// <summary>How an uncovering went: the part, which way the shield was carried and how far, and when the part was open, as measured on the bodies (seconds after it began).</summary>
        public struct Uncovering
        {
            public AimSpot Spot;

            /// <summary>The way the shield went: "up" or "down"; and the furthest it was carried (metres).</summary>
            public string Way;
            public float Went;

            /// <summary>The part came open (a straight line from the other's sword shoulder reached it), when, and when it was covered again.</summary>
            public bool Opened, Shut;
            public float OpenedAt, ShutAt;

            /// <summary>Never opened: what was still before it with the shield carried away.</summary>
            public string Behind;

            /// <summary>It is over: the shield is back.</summary>
            public bool Over;
        }

        /// <summary>The uncovering under way, or the last one.</summary>
        public Uncovering Uncovered;

        /// <summary>An uncovering is under way (its tell, the shield away, or coming back).</summary>
        public bool IsUncovering => uncoverT >= 0f;

        /// <summary>The uncovering under way is still in its tell: the shield has not started away.</summary>
        public bool UncoverTelling => uncoverT >= 0f && uncoverPhase == 0;

        /// <summary>How long ago the uncovering under way began (seconds; below 0: none).</summary>
        public float UncoverTime => uncoverT;

        private float uncoverT = -1f, uncoverWant, uncoverAt, uncoverSpeed, uncoverOpenAt, uncoverHeldFrom;
        private int uncoverPhase, uncoverWay, uncoverRun;
        private float uncoverRunAt, uncoverRunWant;
        private ClipFighter uncoverBy;

        /// <summary>
        /// He uncovers one part of himself to <paramref name="seenBy"/> for a moment (the shield test). First its
        /// tell, for <see cref="PostureMaker.UncoverTell"/>: the point of his blade dips and is held down (a gesture
        /// of his sword, so it is not taken for the shield's own move). Then his shield is carried straight down or
        /// straight up, whichever takes it off the straight line between the other's sword shoulder and the part
        /// soonest, the board still facing the other and his elbow straightening for it (it is not swung about his
        /// shoulder, which drew it in against his body). It stays away until the part has been open for
        /// <see cref="PostureMaker.UncoverFor"/>, and comes back. How far it goes is not set: it goes until the line
        /// is clear of the board, looked at on the bodies each step, and when the part was open is measured the
        /// same way (<see cref="Uncovered"/>). False: he cannot (down, in a clip of his own, no shield, his shield
        /// still up to a blow, his movement not made of its parts, one under way).
        /// </summary>
        public bool Uncover(AimSpot spot, ClipFighter seenBy)
        {
            if (Fallen || Loco.Current == Locomotor.Mode.OneShot || !MoveScore.On || IsUncovering || ShieldArmHeld || seenBy == null || !ShieldBoard(out Vector3 centre, out Vector3 normal, out float radius))
            {
                return false;
            }

            // Which way: up or down, whichever needs the board carried the least to leave the line. (Not aside: out to his
            // own left it is still between the other's sword shoulder and him, however far it goes.)
            Vector3 from = seenBy.PartAt(PartId.UpperArmR), at = AimSpots.Point(this, spot);
            uncoverWay = 0;
            {
                float least = float.MaxValue;
                for (int way = -1; way <= 1; way += 2)
                {
                    float needs = PostureMaker.UncoverMost + 0.1f;
                    for (float far = 0.05f; far <= PostureMaker.UncoverMost + 0.001f; far += 0.05f)
                    {
                        Vector3 c = centre + Vector3.up * (far * way * Size);
                        float gap = float.MaxValue;
                        for (int i = 0; i <= 12; i++)
                        {
                            gap = Mathf.Min(gap, DiscGap(Vector3.Lerp(at, from, Mathf.Min(1f, i / 12f * Openings.LooksFrom * Size / Mathf.Max(0.1f, Vector3.Distance(at, from)))), c, normal, radius, out _));
                        }

                        if (gap >= PostureMaker.UncoverRoom * Size)
                        {
                            needs = far;
                            break;
                        }
                    }

                    // (Down where it is as near: the arm lets a shield down more easily than it holds it up.)
                    if (needs < least - (way > 0 ? 0.05f : 0f))
                    {
                        least = needs;
                        uncoverWay = way;
                    }
                }

                uncoverWay = uncoverWay == 0 ? -1 : uncoverWay;
            }

            Uncovered = new Uncovering { Spot = spot, Way = uncoverWay > 0 ? "up" : "down", Behind = "" };
            uncoverBy = seenBy;
            uncoverT = 0f;
            uncoverPhase = 0;
            uncoverWant = uncoverAt = uncoverSpeed = 0f;
            uncoverRun = 0;
            // (Its tell: the blade's point dips, is held down for as long as the tell lasts, and comes back as the shield goes.)
            if (PostureMaker.UncoverTell > 0.05f)
            {
                Loco.Throw(Locomotor.Pulse.BladeDip, PostureMaker.BladeDips * PostureMaker.BluffSize, 0.1f, Mathf.Max(0f, PostureMaker.UncoverTell - 0.1f), 0.22f);
            }

            Postures++;
            return true;
        }

        private void StepUncover(float dt)
        {
            if (uncoverT < 0f)
            {
                return;
            }

            if (Fallen || uncoverBy == null)
            {
                // (Knocked down in it: it is over where it is.)
                uncoverT = -1f;
                Uncovered.Over = true;
                Loco.ShieldShift = Vector3.zero;
                return;
            }

            uncoverT += dt;
            Vector3 from = uncoverBy.PartAt(PartId.UpperArmR);
            var seen = Openings.LookAt(this, from, Uncovered.Spot);
            // (Open, or shut again, once it has been seen so three steps running: a line that grazes the board's rim flickers.)
            uncoverRun = seen.Open == (uncoverRun > 0) ? uncoverRun + (seen.Open ? 1 : -1) : seen.Open ? 1 : -1;
            if (uncoverRun == 1)
            {
                uncoverRunAt = uncoverT;
                uncoverRunWant = uncoverWant;
            }
            else if (uncoverRun == -1)
            {
                uncoverRunAt = uncoverT;
            }

            // (Not in its tell: what is open before the shield has started away is not this uncovering's doing.)
            if (uncoverRun >= 3 && !Uncovered.Opened && uncoverPhase > 0)
            {
                Uncovered.Opened = true;
                Uncovered.OpenedAt = Mathf.Max(uncoverRunAt, PostureMaker.UncoverTell);
                uncoverOpenAt = uncoverRunWant;
            }
            else if (uncoverRun <= -3 && Uncovered.Opened && !Uncovered.Shut)
            {
                Uncovered.Shut = true;
                Uncovered.ShutAt = uncoverRunAt;
                // (Shut early, by a blow that knocked the board back or by his own arm: it comes back from here.)
                uncoverPhase = 3;
            }

            switch (uncoverPhase)
            {
                case 0:
                    // (Its tell: his blade's, thrown as it began. The shield waits.)
                    if (uncoverT >= PostureMaker.UncoverTell)
                    {
                        uncoverPhase = 1;
                    }

                    break;
                case 1:
                    // (Away, until the line to the part is clear of the board by the room asked, or the arm can go no further.)
                    uncoverWant = Mathf.Min(PostureMaker.UncoverMost, uncoverWant + PostureMaker.UncoverRate * dt);
                    if (uncoverWant >= PostureMaker.UncoverMost || !Openings.ShieldNear(this, from, Uncovered.Spot, PostureMaker.UncoverRoom))
                    {
                        uncoverPhase = 2;
                        uncoverHeldFrom = uncoverT;
                        if (!seen.Open && !Uncovered.Opened)
                        {
                            Uncovered.Behind = seen.Behind;
                        }
                    }

                    break;
                case 2:
                    // (Held away; it starts back so the board is on the line again as the part has been open for as long as asked.)
                    bool back = Uncovered.Opened
                        ? uncoverT >= Uncovered.OpenedAt + PostureMaker.UncoverFor - PostureMaker.UncoverLag - Mathf.Max(0f, uncoverWant - uncoverOpenAt) / PostureMaker.UncoverBack
                        : uncoverT >= uncoverHeldFrom + PostureMaker.UncoverFor;
                    if (back)
                    {
                        uncoverPhase = 3;
                    }

                    break;
                default:
                    uncoverWant = Mathf.Max(0f, uncoverWant - PostureMaker.UncoverBack * dt);
                    break;
            }

            Uncovered.Went = Mathf.Max(Uncovered.Went, uncoverAt);
            // (The arm follows as an arm does: it speeds up and settles in.)
            float omega = 2f / 0.045f, x = omega * dt, eases = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);
            float off = uncoverAt - uncoverWant, carried = (uncoverSpeed + omega * off) * dt;
            uncoverSpeed = (uncoverSpeed - omega * carried) * eases;
            uncoverAt = uncoverWant + (off + carried) * eases;
            if (uncoverPhase == 3 && uncoverWant <= 0f && Mathf.Abs(uncoverAt) < 0.005f)
            {
                uncoverAt = 0f;
                uncoverT = -1f;
                Uncovered.Over = true;
                if (Uncovered.Opened && !Uncovered.Shut)
                {
                    Uncovered.Shut = true;
                    Uncovered.ShutAt = Uncovered.OpenedAt + PostureMaker.UncoverFor;
                }
            }

            float carriedBy = uncoverAt * Size / Mathf.Max(0.1f, Loco.Leg);
            Loco.ShieldShift = new Vector3(0f, uncoverWay * carriedBy, 0f);
        }

        /// <summary>How many postures it has thrown (feints, fake lunges, drops, bluffs).</summary>
        public int Postures { get; private set; }

        private static readonly List<MadeAttack> madePlans = new List<MadeAttack>();

        /// <summary>How far short of its target the last blow asked for on the move would have fallen (metres; 0: it was thrown on the move, or he was not walking): over <see cref="AttackOnTheMove.ShortOfReach"/> he planted himself for it.</summary>
        public float MovedShort { get; private set; }

        /// <summary>The last blow asked for on the move was thrown planted (it was out of the walk's reach and <see cref="AttackOnTheMove.StaysWhenShort"/> is off).</summary>
        public bool MovedPlanted { get; private set; }

        private readonly Vector3[] walkPose = new Vector3[LocoClip.BoneCount];
        private static readonly List<float> footfalls = new List<float>();
        private Vector3 movedFrom;
        private float movedFacing;
        private bool movedTold;

        /// <summary>
        /// An attack thrown on the move (<see cref="AttackOnTheMove"/>): of its plain strikes' scores (or the one
        /// named), the one that fits the point best when it is thrown over the walk as it goes now, timed to land
        /// as a foot comes down, and begun over the walk. Null, and nothing begun, when he is not walking (standing,
        /// or stepping in his stance) or no strike can be so thrown.
        /// </summary>
        private MadeAttack StrikeOnTheMove(ClipFighter enemy, Vector3 point, in AttackStyle style, string named)
        {
            MovedShort = 0f;
            MovedPlanted = false;
            float pace = new Vector2(Loco.Velocity.x, Loco.Velocity.z).magnitude;
            if (Loco.Current != Locomotor.Mode.Cycle || pace < AttackOnTheMove.WalksFrom * Mathf.Sqrt(Mathf.Max(0.1f, Size)))
            {
                return null;
            }

            var m = BodyMeasures.OfClips;
            float leg = Mathf.Max(0.1f, Loco.Leg);
            var raw = Loco.RawPoints;
            for (int i = 0; i < walkPose.Length; i++)
            {
                walkPose[i] = raw[i] / leg;
            }

            var walk = m.Decompose(walkPose);
            Vector3 own = Quaternion.Euler(0f, -Loco.Facing, 0f) * Loco.Velocity;
            Vector3 target = InOwnFrame(point);
            Vector3 toward = new Vector3(target.x, 0f, target.z);
            var move = AttackOnTheMove.Style;
            var aim = new AttackAim
            {
                Aimed = true,
                Target = target,
                OnTheMove = true,
                Walks = new Vector3(own.x, 0f, own.z) / leg,
                Turns = Loco.FacingSpeed,
                WalkNow = walk,
                Move = move,
                Into = toward.sqrMagnitude > 1e-6f ? toward.normalized * move.HipsInto : Vector3.zero,
                // (He faces his enemy as he goes, and turns to keep facing it.)
                Faces = enemy != null,
                FacesAt = enemy != null ? InOwnFrame(enemy.Loco.Position) : Vector3.zero,
            };
            var hold = Poser.Hold();
            MadeAttack best = null;
            foreach (var clip in StrikeClips)
            {
                var score = AttackScore.Of(clip);
                if (score == null || (named != null ? clip.Name != named : !score.Plain) || (fakeFromBehind && !score.WindsBehind))
                {
                    continue;
                }

                // (To land as a foot comes down: each of the walk's coming footfalls near the blow's own time is tried,
                // and the one the blow fits best taken; a wind-up hurried or drawn out to meet it costs a little.)
                float natural = score.Drive / score.Rate * Mathf.Max(0.05f, style.WindUpTime) + (score.Contact - score.Drive) / score.Rate * Mathf.Clamp(style.DriveTime, 0.4f, 3f);
                AttackOnTheMove.Footfalls(Loco.Phase, Loco.PhaseRate, Mathf.Max(0.3f, 0.7f * natural), 1.6f * natural, footfalls);
                if (footfalls.Count == 0)
                {
                    footfalls.Add(natural);
                }

                foreach (float lands in footfalls)
                {
                    var timed = aim;
                    timed.LandIn = lands;
                    var plan = AttackMaker.Make(score, style, timed, hold, build: false);
                    if (plan != null)
                    {
                        plan.Cost += 0.6f * Mathf.Abs(lands - natural);
                        if (best == null || plan.Cost < best.Cost)
                        {
                            best = plan;
                        }
                    }
                }
            }

            if (best == null)
            {
                return null;
            }

            AttackMaker.Build(best);
            best.Meets = "";
            // (Out of the walk's reach even with its step in: thrown on the move all the same, to miss; or, as it was,
            // he plants himself and lunges for it.)
            MovedShort = best.Short * leg > AttackOnTheMove.ShortOfReach * Size ? best.Short * leg : 0f;
            if (MovedShort > 0f && !AttackOnTheMove.StaysWhenShort)
            {
                MovedPlanted = true;
                return null;
            }

            MovedPlanted = false;
            Vector3 stepIn = Quaternion.Euler(0f, Loco.Facing, 0f) * best.StepIn * leg;
            if (postureNow)
            {
                // (A posture on the move. A feint: the attack's wind-up over the walk and back, the walk as it was:
                // no check, no step in, the hips not going in. A fake lunge: the walk's legs go on, and his hips dart
                // toward the other and sink as the lunge's do, and come back; his feet keep to their way.)
                var cutFrom = best.Clip;
                var posture = PostureMaker.Make(best, postureFeint, postureLunge, out float turnsAt);
                Vector3 darts = Vector3.zero;
                if (postureLunge > 0f)
                {
                    Vector3 way = best.Asked.Into.sqrMagnitude > 1e-8f ? best.Asked.Into.normalized : Vector3.forward;
                    darts = way * (postureLunge * PostureMaker.LungeFar * PostureMaker.LungeOnTheMove / Locomotor.ClipLeg);
                }

                if (posture == null || !Poser.PlayMadeOver(posture, point, postureLunge > 0f ? 0f : turnsAt, 0f, darts, Vector3.zero))
                {
                    return null;
                }

                FakeBegun(posture, cutFrom, turnsAt);
                best.Clip = posture;
                best.Feint = postureFeint;
                best.FakeLunge = postureLunge;
                best.ContactAt = turnsAt;
                best.DriveAt = turnsAt;
                best.Held = 0f;
                best.StepIn = Vector3.zero;
                Postures++;
            }
            else if (!Poser.PlayMadeOver(best.Clip, point, best.DriveAt, move.Check, best.Asked.Into, stepIn))
            {
                return null;
            }

            Made = best;
            movedFrom = Loco.Position;
            movedFacing = Loco.Facing;
            movedTold = false;
            return best;
        }

        /// <summary>
        /// A blow thrown on the move, each step until it lands: the walk has not gone quite as the blow's plan had
        /// it (it is steered as it goes), so where the blade will land is worked out again from where he is now and
        /// what the plan still has him do, and the weapon hand is pulled by what that misses the target by.
        /// </summary>
        private void PullOnTheMove()
        {
            var made = Made;
            if (made == null || !made.OnTheMove || !Loco.ShotOverWalk || Loco.Shot != made.Clip)
            {
                return;
            }

            var asked = made.Asked;
            float leg = Mathf.Max(0.1f, Loco.Leg);
            AttackOnTheMove.Walked(asked.Walks, asked.Turns, asked.Faces ? asked.FacesAt : (Vector3?)null, made.StepIn, Loco.ShotTime, made.DriveAt, made.ContactAt, asked.Move.Check, out Vector3 at, out float turned);
            if (AttackOnTheMove.Trace != null && !movedTold && Loco.ShotTime >= made.ContactAt)
            {
                // (For the reports: how the walk and the layered body were at the landing against what the plan had.)
                movedTold = true;
                var back = Quaternion.Euler(0f, -movedFacing, 0f);
                Vector3 went = back * (Loco.Position - movedFrom) / leg;
                var raw = Loco.RawPoints;
                for (int i = 0; i < walkPose.Length; i++)
                {
                    walkPose[i] = raw[i] / leg;
                }

                var now = BodyMeasures.OfClips.Decompose(walkPose);
                AttackScore.SwordOf(now, BodyMeasures.OfClips, made.Hold, false, out _, out _, out Vector3 grip, out Vector3 way);
                Vector3 strike = grip + way * made.Hold.Strike;
                Vector3 flat = went + Quaternion.Euler(0f, Mathf.DeltaAngle(movedFacing, Loco.Facing), 0f) * new Vector3(strike.x, 0f, strike.z);
                AttackMaker.PlannedOnTheMove(made, out Vector3 planHips, out float planYaw, out Vector3 planLands);
                AttackOnTheMove.Trace(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "{0} at its landing ({1:0.00} s of its clip): the walk had him at ({2:0.00}, {3:0.00}) turned {4:0}, planned ({5:0.00}, {6:0.00}) turned {7:0}; hips on the root ({8:0.00}, {9:0.00}, {10:0.00}) turned {11:0}, planned ({12:0.00}, {13:0.00}, {14:0.00}) turned {15:0}; the blade's striking part as layered ({16:0.00}, {17:0.00}, {18:0.00}), planned ({27:0.00}, {28:0.00}, {29:0.00}), the target ({19:0.00}, {20:0.00}, {21:0.00}): {22:0} cm off the target, {30:0} cm off its plan (leg lengths, the frame it began in: right, up, forward); the plan: turned {23:0}, short {24:0} cm, hand {25:0} cm, to land {26:0.00} s, a step in of {31:0} cm",
                    made.Clip.Name, Loco.ShotTime, went.x, went.z, Mathf.DeltaAngle(movedFacing, Loco.Facing), at.x, at.z, turned,
                    now.Pelvis.x, now.Pelvis.y, now.Pelvis.z, now.PelvisTurn.y, planHips.x, planHips.y, planHips.z, planYaw,
                    flat.x, strike.y, flat.z, asked.Target.x, asked.Target.y, asked.Target.z, 100f * leg * (new Vector3(flat.x, strike.y, flat.z) - asked.Target).magnitude,
                    made.Turn, 100f * leg * made.Short, 100f * leg * made.HandMoved, made.ContactAt, planLands.x, planLands.y, planLands.z, 100f * leg * (new Vector3(flat.x, strike.y, flat.z) - planLands).magnitude, 100f * leg * made.StepIn.magnitude * Mathf.Sign(Vector3.Dot(made.StepIn, asked.Target))));
            }

            if (Loco.ShotTime > made.ContactAt)
            {
                return;
            }

            // (The target against where the plan has him now, and so against where he is.)
            Vector3 ahead = Quaternion.Euler(0f, -turned, 0f) * new Vector3(asked.Target.x - at.x, 0f, asked.Target.z - at.z);
            Vector3 lands = Loco.Position + Quaternion.Euler(0f, Loco.Facing, 0f) * ahead * leg;
            Vector3 want = movedFrom + Quaternion.Euler(0f, movedFacing, 0f) * new Vector3(asked.Target.x, 0f, asked.Target.z) * leg;
            Vector3 pull = want - lands;
            pull.y = 0f;
            if (AttackOnTheMove.Pulls)
            {
                Poser.AimPullIs(pull);
            }
        }

        /// <summary>A made attack may be put on another recording's legs when those fit the distance better (the scene's Combat Feel, "Attacks Borrow Legs").</summary>
        public static bool MadeBorrowsLegs = true;

        /// <summary>What borrowed legs cost a plan (the maker's count): its own legs are kept unless another's fit that much better.</summary>
        public const float BorrowedLegsCost = 0.5f;

        private static readonly List<PartId> metParts = new List<PartId>();

        /// <summary>
        /// What a made attack's blade would meet first on its way, with <paramref name="target"/> standing as it stands
        /// (the plan "Aimed attacks": is the way open?): the blade is swept through the last of its wind-up and its cut
        /// against the target's parts and its shield. The way is open when the first it meets is a part it was made for.
        /// </summary>
        private void FirstMet(MadeAttack made, ClipFighter target, System.Func<PartId, bool> counts)
        {
            float leg = Mathf.Max(0.1f, Loco.Leg), length = Poser.WeaponLength / leg, radius = 0.035f * Size;
            var turn = Quaternion.Euler(0f, Loco.Facing, 0f);
            Vector3 World(Vector3 own) => new Vector3(Loco.Position.x, groundLevel, Loco.Position.z) + turn * new Vector3(own.x, 0f, own.z) * leg + Vector3.up * (own.y * leg);
            made.Meets = "nothing";
            made.WayOpen = false;
            for (float t = made.ContactAt - 0.3f; t <= made.ContactAt + 0.04f; t += 1f / 240f)
            {
                made.Blade(t, length, out Vector3 grip, out Vector3 tip);
                Vector3 g = World(grip), p = World(tip);
                metParts.Clear();
                bool shield = false;
                foreach (var collider in Physics.OverlapCapsule(Vector3.Lerp(g, p, 0.1f), p, radius, ~0, QueryTriggerInteraction.Collide))
                {
                    if (target.ShieldColliders.Contains(collider))
                    {
                        shield = true;
                    }
                    else if (target.PartOf(collider, out var id))
                    {
                        metParts.Add(id);
                    }
                }

                if (shield && metParts.Count == 0)
                {
                    made.Meets = "its shield";
                    return;
                }

                if (metParts.Count > 0)
                {
                    foreach (var id in metParts)
                    {
                        if (counts(id))
                        {
                            made.Meets = "the " + id;
                            made.WayOpen = true;
                            return;
                        }
                    }

                    made.Meets = "the " + metParts[0];
                    return;
                }
            }
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
        /// The best a recorded strike can do at a point asked for (the plan "Aimed attacks", step 0: today's numbers):
        /// of its strikes, the one that comes nearest <paramref name="point"/> with its turn, its lunge and its arm's
        /// pull, played aimed at that point. How far it is expected to miss by (<paramref name="miss"/>, metres; 0: it
        /// reaches). Null with no strikes.
        /// </summary>
        public LocoClip StrikeNearest(ClipFighter target, Vector3 point, out float miss)
        {
            miss = 0f;
            if (StrikeClips.Count == 0)
            {
                return null;
            }

            KeepOff(target);
            LocoClip best = null;
            float least = float.MaxValue, leastBefore = float.MaxValue;
            foreach (var clip in StrikeClips)
            {
                float after = Poser.MissAfterPull(clip, point);
                float before = Poser.MissBeforePull(clip, point, 0f, Loco.Position, Loco.Facing);
                if (after < least - 1e-4f || (Mathf.Abs(after - least) <= 1e-4f && before < leastBefore))
                {
                    best = clip;
                    least = after;
                    leastBefore = before;
                }
            }

            miss = least;
            Poser.PlayStrike(best, point);
            return best;
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
        /// A leap (the plan "Fight Rhythm from Recordings", step 2; the duel lab's): from a stand in its stance it
        /// springs <paramref name="metres"/> (at a Medium's size) the way given, in <paramref name="seconds"/>: both feet
        /// leave the ground together, each goes in one arc to where the stance has it under the body as it arrives, and
        /// they come down together, the hips sinking a little as the legs take the weight (<see cref="LocoBody.Hop"/>,
        /// the hop a step back from a blow can be). Its body is carried as ground given to a blow is, no faster than
        /// <see cref="RetireSpeed"/>. False: it cannot (down, in a clip of its own, held where it is). Not standing
        /// in its stance, its feet step after the body one at a time instead.
        /// </summary>
        public bool LeapNow(Vector3 way, float metres, float seconds)
        {
            way.y = 0f;
            if (Fallen || Anchored || Loco.Current == Locomotor.Mode.OneShot || way.sqrMagnitude < 1e-6f)
            {
                return false;
            }

            way.Normalize();
            retireLeft = way * (metres * Size);
            retireVoids = false;
            voidedFor = -1;
            testRetireFor = seconds;
            Leaps++;
            // (No push of its walk as well, as a step back from a blow has: a leap lands where it is asked to.)
            LeapHopped = Poser.Hop(seconds / 0.8f);
            return true;
        }

        /// <summary>
        /// A leap made of the recorded jump (<see cref="LeapMaker"/>): from its guard it crouches, pushes off the leg
        /// behind and springs <paramref name="metres"/> (at a Medium's size) the way given, its feet in the air for
        /// <paramref name="flight"/> seconds, and lands. A clip its walk plays, as it plays a made attack. False: it
        /// cannot (down, in a clip of its own, held where it is, no guard to leap from, no jump recorded).
        /// </summary>
        public bool LeapMade(Vector3 way, float metres, float flight, LeapStyle? style = null)
        {
            way.y = 0f;
            var jump = LocoLibrary.Get("jump_spot");
            if (Fallen || Anchored || Loco.Current == Locomotor.Mode.OneShot || way.sqrMagnitude < 1e-6f || jump == null || Loco.Guard == null || Loco.Guard.Idle == null)
            {
                return false;
            }

            Vector3 own = Quaternion.Euler(0f, -Loco.Facing, 0f) * way.normalized;
            var clip = LeapMaker.Make(jump, Loco.Guard.Idle, own, metres * Size / Mathf.Max(0.1f, Loco.Leg), flight, Loco.Leg, style ?? LeapMaker.Style);
            if (clip == null)
            {
                return false;
            }

            Leaps++;
            LeapHopped = false;
            Loco.PlayAimed(clip, 1f, 0f, 0f);
            return true;
        }

        /// <summary>How many leaps it has been asked for, and whether the last was hopped (both feet together) or stepped after.</summary>
        public int Leaps { get; private set; }
        public bool LeapHopped { get; private set; }

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
        public bool TurnNow(float degrees, bool quickly = false) => !Fallen && !Anchored && Loco.TurnNow(degrees, quickly);

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
                    // (In a bash the wrist gives more: the board is to look square at who it is driven into.)
                    float wrist = CombatFeel.ShieldWristTurn * (Bashing ? 2.5f : 1f);
                    float by = wrist * (1f - Mathf.Exp(-off / Mathf.Max(1f, wrist))) * weight;
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
        /// The shield gives way to the sword (18 November, Renier's word: "the shield gives way to the sword"; Combat
        /// Feel "The Shield Gives Way To The Sword"). His two arms were each posed alone, and his own sword arm and
        /// blade went through his own board (in a fight of six about once in three seconds for each shield bearer,
        /// mostly in his own blows: the board is held before his chest through them). Now, as the last thing but the
        /// keeping of the board out of his body: where his sword forearm or his blade passes through the board's
        /// face, now or within <see cref="SwordSeenAhead"/> seconds at the speed they are moving, the board steps
        /// aside in its own plane, the least way that leaves the whole of that path a lane
        /// (<see cref="SwordLane"/> clear of its rim; outward and downward rather than across his own chest), and
        /// comes back when the path is clear. What it is asked is then followed smoothly, as all his shield hand is
        /// (<see cref="SettleHeld"/>, at its quicker rate while it gives way). Off: as before.
        /// </summary>
        public static bool ShieldGivesWayToSword = true;

        /// <summary>The lane left the sword: how far clear of the board's rim its path is kept (metres at a Medium's size; its forearm that and the forearm's own thickness), and how far ahead its path is looked for (seconds).</summary>
        public static float SwordLane = 0.06f, SwordSeenAhead = 0.3f;

        /// <summary>How long the board takes to come back once the sword's path is clear of it (seconds).</summary>
        public const float LaneCloses = 0.25f;

        private readonly Vector3[] laneWas = new Vector3[8];
        private bool laneKnown;
        private Vector3 laneGive;

        /// <summary>For the reports: steps the board was asked aside for the sword, and the furthest (metres).</summary>
        public int GaveWayToSword;
        public float GaveWayToSwordMost;

        /// <summary>The board giving way to the sword now (0 to 1): its hand follows at the quicker rate.</summary>
        private float GivingWay => Mathf.Clamp01(laneGive.magnitude / (0.05f * Size));

        private void GiveWayToSword(float dt, Transform upper, Transform fore, Transform hand)
        {
            var slot = Anim.SlotWeaponL;
            var sword = Anim.SlotWeaponR;
            if (!ShieldGivesWayToSword || stance != Stances.Shield || slot == null || sword == null || Rig.IsSevered(PartId.HandR) || !BoardOnSlot(out var boardLocal) || dt <= 0f)
            {
                laneKnown = false;
                laneGive = Vector3.zero;
                return;
            }

            // (His sword's two lines, in the frame of where he stands: his forearm to his knuckles, and his blade from a
            // hand's breadth up it to its tip. Where each end is going by how it moved this step.)
            Vector3 rootAt = Loco.Position + Vector3.up * Poser.GroundY;
            Quaternion rootTurn = Quaternion.Euler(0f, Loco.Facing, 0f), rootBack = Quaternion.Inverse(rootTurn);
            Vector3 elbow = Anim.Part(PartId.ForearmR).position, wrist = Anim.Part(PartId.HandR).position, along = sword.rotation * Vector3.forward;
            // (And the same two lines as they are seen, on the physical body: a blade stopped on the other's shield, or an
            // arm knocked off its pose, is not where the pose has it (the tip up to 57 cm off, in the aim test), and it
            // is the seen sword that goes through the seen board.)
            var seen = Rig.SlotWeaponR;
            bool seenToo = Authority != null && seen != null;
            Vector3 elbowSeen = seenToo ? Rig.Part(PartId.ForearmR).transform.position : elbow, wristSeen = seenToo ? Rig.Part(PartId.HandR).transform.position : wrist;
            Vector3 alongSeen = seenToo ? seen.rotation * Vector3.forward : along, gripSeen = seenToo ? seen.position : sword.position;
            var now = new[]
            {
                rootBack * (elbow - rootAt), rootBack * (wrist + (wrist - elbow).normalized * (0.08f * Size) - rootAt),
                rootBack * (sword.position + along * (0.1f * Size) - rootAt), rootBack * (sword.position + along * Poser.WeaponLength - rootAt),
                rootBack * (elbowSeen - rootAt), rootBack * (wristSeen + (wristSeen - elbowSeen).normalized * (0.08f * Size) - rootAt),
                rootBack * (gripSeen + alongSeen * (0.1f * Size) - rootAt), rootBack * (gripSeen + alongSeen * Poser.WeaponLength - rootAt),
            };
            // (His sword upper arm is not among them. Added as a fifth line it made nothing better that could be read: the
            // aim test's crossings 0.60 s against 0.35 s, and the board still in his upper arm 0.08 s of a fight of six.)
            var going = new Vector3[8];
            for (int i = 0; i < 8; i++)
            {
                going[i] = laneKnown ? Vector3.ClampMagnitude((now[i] - laneWas[i]) / dt, 30f) : Vector3.zero;
                laneWas[i] = now[i];
            }

            laneKnown = true;

            // (The board as the pose has it, in the same frame: its middle, the way its face looks, and two ways across it.)
            Vector3 centre = rootBack * (slot.TransformPoint(boardLocal) - rootAt), looks = rootBack * slot.forward;
            float radius = ShieldRadius * Size;
            Vector3 across = Vector3.Cross(looks, Vector3.up);
            across = across.sqrMagnitude > 1e-6f ? across.normalized : Vector3.Cross(looks, Vector3.forward).normalized;
            Vector3 upward = Vector3.Cross(across, looks);

            // (Where those lines go through the board's face, now and over the moment ahead: each such place, and how far clear of it the rim is to be.)
            lanePoints.Clear();
            const int Looks = 6;
            for (int k = 0; k <= Looks; k++)
            {
                for (int line = 0; line < (seenToo ? 4 : 2); line++)
                {
                    // (How far ahead: all of it for a line that is going somewhere (1.5 m/s and over at its faster end), none
                    // for one that is not (under half a metre a second): a sword at rest by the board is where it is, and
                    // looked ahead for by the little it stirs, the board stirred with it. The seen sword a third as far:
                    // how it moves from step to step is rougher than the pose.)
                    float quick = Mathf.Max(going[2 * line].magnitude, going[2 * line + 1].magnitude);
                    float ahead = SwordSeenAhead * (line < 2 ? 1f : 0.33f) * Mathf.Clamp01((quick - 0.5f) / 1f) * k / Looks;
                    if (k > 0 && ahead <= 0f)
                    {
                        continue;
                    }

                    Vector3 from = now[2 * line] + going[2 * line] * ahead, to = now[2 * line + 1] + going[2 * line + 1] * ahead;
                    float a = Vector3.Dot(from - centre, looks), b = Vector3.Dot(to - centre, looks);
                    if (a * b > 0f || Mathf.Abs(a - b) < 1e-6f)
                    {
                        continue;
                    }

                    Vector3 at = Vector3.Lerp(from, to, a / (a - b)) - centre;
                    float clear = radius + (SwordLane + (line % 2 == 0 ? 0.05f : 0f)) * Size;
                    float flatX = Vector3.Dot(at, across), flatY = Vector3.Dot(at, upward);
                    if (flatX * flatX + flatY * flatY < clear * clear)
                    {
                        lanePoints.Add(new Vector3(flatX, flatY, clear));
                    }
                }
            }

            Vector3 need = Vector3.zero;
            if (lanePoints.Count > 0)
            {
                // (The least way aside, of sixteen ways across its face, that leaves every such place clear of the rim. Ways
                // across his own chest, to his sword side, cost half as much again; upward, before his face, a quarter more.)
                Vector3 swordSide = rootBack * (Anim.Part(PartId.UpperArmR).position - Anim.Part(PartId.UpperArmL).position);
                float inX = Vector3.Dot(swordSide, across), inY = Vector3.Dot(swordSide, upward), inLong = Mathf.Sqrt(inX * inX + inY * inY);
                if (inLong > 1e-4f)
                {
                    inX /= inLong;
                    inY /= inLong;
                }

                float best = float.MaxValue, bestBy = 0f;
                Vector3 bestWay = Vector3.zero;
                for (int w = 0; w < 16; w++)
                {
                    float angle = w * (Mathf.PI / 8f), wayX = Mathf.Cos(angle), wayY = Mathf.Sin(angle);
                    float by = 0f;
                    foreach (var p in lanePoints)
                    {
                        // (How far along this way the middle must go for this place to be the lane's width from it.)
                        float alongWay = p.x * wayX + p.y * wayY;
                        float off = p.x * p.x + p.y * p.y - alongWay * alongWay;
                        by = Mathf.Max(by, alongWay + Mathf.Sqrt(Mathf.Max(0f, p.z * p.z - off)));
                    }

                    float cost = by * (1f + 0.5f * Mathf.Max(0f, wayX * inX + wayY * inY) + 0.25f * Mathf.Max(0f, wayY));
                    // (The way it is already giving is kept unless another is clearly less: it does not flit from side to side.)
                    Vector3 wayWorld = across * wayX + upward * wayY;
                    if (laneGive.sqrMagnitude > 1e-6f)
                    {
                        cost *= 1f - 0.25f * Mathf.Max(0f, Vector3.Dot(wayWorld, laneGive.normalized));
                    }

                    if (cost < best)
                    {
                        best = cost;
                        bestWay = wayWorld;
                        bestBy = by;
                    }
                }

                need = bestWay * bestBy;
            }

            if (need.sqrMagnitude > 1e-8f)
            {
                laneGive = need;
                GaveWayToSword++;
                GaveWayToSwordMost = Mathf.Max(GaveWayToSwordMost, need.magnitude);
            }
            else
            {
                // (The path clear: it comes back.)
                laneGive *= Mathf.Exp(-dt * 3f / LaneCloses);
                if (laneGive.sqrMagnitude < 1e-6f)
                {
                    laneGive = Vector3.zero;
                }
            }

            if (laneGive.sqrMagnitude < 1e-8f)
            {
                return;
            }

            float reach = (Anim.RestLocalPosition(PartId.ForearmL).magnitude + Anim.RestLocalPosition(PartId.HandL).magnitude) * 0.97f;
            Vector3 target = hand.position + rootTurn * laneGive;
            target = upper.position + Vector3.ClampMagnitude(target - upper.position, reach);
            var turned = hand.rotation;
            TwoBoneIK.Solve(upper, fore, hand, target, TwoBoneIK.BendWay(upper, fore, hand));
            hand.rotation = turned;
        }

        private readonly List<Vector3> lanePoints = new List<Vector3>();

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
                foreach (var id in UpperBodyArticulation.ShieldStoppedOnFeet ? ShieldKeptOutOfLegsToo : ShieldKeptOutOf)
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

        /// <summary>
        /// And of its thighs (18 November, with <see cref="UpperBodyArticulation.ShieldStoppedOnFeet"/>): the part its
        /// board was in most often (0.85 s of 1.17 s in a dance), which the pose never kept it out of. With the board
        /// solid against the body, a pose that asks for it in a thigh has the arm press it there each stride.
        /// </summary>
        private static readonly PartId[] ShieldKeptOutOfLegsToo = { PartId.Head, PartId.Chest, PartId.Spine, PartId.Pelvis, PartId.ThighL, PartId.ThighR };

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

        /// <summary>
        /// The shield is held in cover (Combat Feel "The Shield Is Held In Cover"). Whatever the body is doing (standing,
        /// walking, a gesture of the sword arm, a blow of his own) the board is held before his chest where it covers
        /// him best, looking at the enemy, in the body's own frame: it goes with the body as it steps, sways and
        /// rises, and keeps a little of what the recording has the arm do (so it is not stiff), but it stays in place.
        /// It leaves there only to meet a blow, and comes back. Off: the shield arm is the recording's, as it was:
        /// it swung open in every cut and every fake of one.
        /// </summary>
        public static bool ShieldCover = true;

        /// <summary>How much of the shield arm is the cover's (the rest the recording's own), standing, walking and gesturing; and in a real blow of his own.</summary>
        public static float CoverHold = 0.85f, CoverInBlow = 0.7f;

        /// <summary>Where the board's middle is held, from the chest, in the body's own frame (metres at a Medium's size: x to his right, y up, z ahead); and the way it looks.</summary>
        public static Vector3 CoverAt = new Vector3(-0.08f, -0.1f, 0.3f), CoverFace = new Vector3(-0.2f, 0.05f, 1f);

        /// <summary>How quickly the cover takes the arm up and lets it go (seconds).</summary>
        public const float CoverEase = 0.25f;

        private float coverWeight;

        /// <summary>How much of the shield arm the cover has now (0 to 1): for the reports.</summary>
        public float CoverNow => coverWeight;

        // ---- The shield's bash ---------------------------------------------------------------------------------------

        /// <summary>
        /// A shield bashes what comes too near (Combat Feel "A Shield Bashes What Comes Too Near"), and the bash itself
        /// (the plan's step 9a: breaking the guard). There are three (<see cref="BashKinds"/>): a small one, thrown from
        /// where he stands (he sinks behind his board and his body goes in with it); a medium one, in which he leaps in
        /// behind his board and stops short of the enemy; and a hard one, in which he crouches deep and leaps fast all
        /// the way into the enemy's space and goes on through. In each the board is driven straight ahead, looking at
        /// the enemy. What a bash does is not set: it is the speed the board arrives at the enemy with (his body's and
        /// his arm's together), times the bash's weight. Under <see cref="BashStumblesFrom"/> the enemy is shoved back
        /// a step; from there he is thrown back stumbling, his guard open; from <see cref="BashFellsFrom"/> he goes
        /// down. Sized at first from the recording "Sword And Shield Block".
        /// </summary>
        public static bool ShieldBash = true;

        /// <summary>How near an enemy must come to be bashed (metres, root to root, at a Medium's size); no bash sooner than this after the last (seconds).</summary>
        public static float BashWithin = 1.2f, BashEvery = 2.5f;

        /// <summary>The three bashes: small, medium, hard (<see cref="BashLevers"/>), each with its own levers and its own leap.</summary>
        public static BashLevers[] BashKinds = { BashLevers.Small(), BashLevers.Medium(), BashLevers.Hard() };

        /// <summary>The shove a fighter is thrown back stumbling from, and the one he goes down from (m/s at a Medium's size).</summary>
        public static float BashStumblesFrom = 1.5f, BashFellsFrom = 4.5f;

        /// <summary>How hard a bash throws the one it knocks down, times its shove: at 1 he goes over about where he stood; the shove times this from 9 m/s up, he is thrown off his feet and lands some metres back (the falls' own "thrown").</summary>
        public static float BashThrows = 1.6f;

        /// <summary>How much of the arm's own speed counts in what a bash does, against the body's (1: as much; 0: the body's speed alone).</summary>
        public static float BashArmCounts = 0.4f;

        /// <summary>The bash in time (seconds): drawn in, held out, brought back; how far up the board goes as it is driven (metres); how near the board's middle must come to the enemy to meet him, and how far ahead of his own middle the board is carried in its cover (metres, flat).</summary>
        public const float BashWind = 0.1f, BashHeld = 0.2f, BashBack = 0.3f, BashUp = 0.05f, BashMeets = 0.46f, BashCarried = 0.45f;

        private float bashFor = -1f, bashReadyAt, bashDriveAt, bashDriveTime, bashOutFar, bashDrawIn, bashShoulder, bashEnds, bashLeapt, bashLeaptAt;
        private BashLevers bashKind;
        private ClipFighter bashAt;
        private bool bashStruck, bashSurged, bashBoardKnown;
        private Vector3 bashBoardWas, bashWay;
        private readonly float[] bashBoardSpeeds = new float[16], bashSorted = new float[16];
        private int bashBoardSpeedAt;

        /// <summary>He is in a bash of his shield.</summary>
        public bool Bashing => bashFor >= 0f;

        /// <summary>Bashes thrown; how many of them met the enemy; of those, how many threw him back stumbling and how many put him down.</summary>
        public int Bashes, BashesLanded, BashesStumbled, BashesFelled;

        /// <summary>The last bash, in words: how it was thrown and what it did.</summary>
        public string LastBash = "";

        /// <summary>How far out the board is now in its bash (below 0: drawn in before it; 1: all the way out).</summary>
        private float BashOutNow
        {
            get
            {
                if (bashFor < 0f)
                {
                    return 0f;
                }

                float t = bashFor;
                if (t < bashDriveAt)
                {
                    // (Drawn in, and held drawn until the drive.)
                    return -0.25f * Mathf.Sin(Mathf.PI * 0.5f * Mathf.Clamp01(t / BashWind));
                }

                t -= bashDriveAt;
                if (t < bashDriveTime)
                {
                    float u = t / bashDriveTime;
                    return Mathf.Lerp(-0.25f, 1f, u * u * (3f - 2f * u));
                }

                t -= bashDriveTime;
                if (t < BashHeld)
                {
                    return 1f;
                }

                float back = Mathf.Clamp01((t - BashHeld) / BashBack);
                return 1f - back * back * (3f - 2f * back);
            }
        }

        /// <summary>
        /// He bashes now, at this enemy (null: straight ahead, at nobody, to be seen), with one of the three bashes
        /// (0: small, 1: medium, 2: hard). False: he cannot (no shield, down, in a blow, meeting one, one just thrown).
        /// </summary>
        public bool Bash(ClipFighter at, bool whenever = false, int kind = 0, bool braced = false, bool extra = false)
        {
            if (Bashing || Fallen || ShieldColliders.Count == 0 || Striking || interceptWeight > 0f || GuardOpen || IsUncovering || (!whenever && clock < bashReadyAt))
            {
                return false;
            }

            kind = Mathf.Clamp(kind, 0, BashKinds.Length - 1);
            var levers = BashKinds[kind];
            Vector3 ahead = Quaternion.Euler(0f, Loco.Facing, 0f) * Vector3.forward;
            Vector3 to = at != null ? Flat(at.Loco.Position - Loco.Position) : ahead;
            float gap = at != null ? to.magnitude : float.MaxValue;
            bashWay = at != null && gap > 1e-3f ? to / gap : ahead;
            float arm = Mathf.Clamp01(levers.Arm);
            // (How far the arm goes: drawn in toward his chest first, then out to its whole length, his shield shoulder
            // thrown in behind it. How hard it drives is how quickly.)
            float armReach = Mathf.Clamp01(levers.Reach);
            bashOutFar = Mathf.Lerp(0.18f, 0.5f, armReach);
            bashDrawIn = Mathf.Lerp(0.05f, 0.22f, armReach);
            bashShoulder = Mathf.Lerp(8f, 40f, armReach);
            bashDriveTime = Mathf.Lerp(0.22f, 0.08f, arm);
            // (How far he leaps: to where this bash ends, so far short of where the enemy stands, and no further than its most.)
            // (Braced: he stands where he is and meets what comes with his board, a counter to a hard bash.)
            float far = braced ? 0f : at != null ? Mathf.Clamp(gap / Size - levers.EndsShort, 0f, levers.MostLeap) : levers.MostLeap;
            bashLeapt = 0f;
            bashLeaptAt = 0f;
            float speed = Mathf.Max(1f, levers.LeapSpeed);
            if (far >= 0.15f && Loco.Current != Locomotor.Mode.OneShot)
            {
                float flight = Mathf.Clamp(far / speed, 0.1f, 0.7f);
                var leap = new LeapStyle { Crouch = levers.Crouch, Height = levers.Height, Lean = levers.Lean, Arms = 0f, Body = 0.7f, PrepTime = levers.Prep, PushTime = 0.1f, LandTime = 0.12f, BackTime = 0.3f };
                if (LeapMade(bashWay, far, flight, leap))
                {
                    bashLeapt = far;
                    bashLeaptAt = far / flight;
                    // (The board is driven so that it is all the way out as it reaches him: in the air if his leap goes
                    // on into the enemy's space, as his feet land if it ends short of him.)
                    float off = leap.PrepTime + leap.PushTime, lands = off + flight;
                    float reaches = at != null ? off + Mathf.Max(0f, gap / Size - BashCarried - bashOutFar - BashLandsFrom) / bashLeaptAt : lands;
                    bashDriveAt = Mathf.Max(BashWind, Mathf.Min(lands, reaches) - bashDriveTime);
                }
            }

            if (bashLeapt <= 0f)
            {
                // (Standing: he sinks behind the board, and his body goes in a little with it.)
                bashDriveAt = BashWind;
                Loco.Dip(levers.Sinks, levers.Lean * 0.4f, BashWind + 0.5f * bashDriveTime, BashHeld, BashBack);
            }

            bashKind = levers;
            bashKindAt = kind;
            bashExtra = extra && kind == 2;
            bashFor = 0f;
            bashAt = at;
            bashStruck = bashSurged = bashBoardKnown = false;
            // (A counter does nothing by itself: what comes of the two boards meeting is settled as the hard bash arrives.)
            countering = braced;
            bashStruck = braced;
            bashBoardSpeedAt = 0;
            bashEnds = Mathf.Max(bashDriveAt + bashDriveTime + BashHeld + BashBack, bashLeapt > 0f ? levers.Prep + 0.1f + bashLeapt / bashLeaptAt + 0.2f : 0f);
            bashReadyAt = clock + BashEvery;
            Bashes++;
            LastBash = string.Format(System.Globalization.CultureInfo.InvariantCulture, "a {0} bash: {1}, his board driven {2:0} cm ahead in {3:0.00} s", levers.Name, bashLeapt > 0f ? string.Format(System.Globalization.CultureInfo.InvariantCulture, "he leapt {0:0.00} m in at {1:0.0} m/s", bashLeapt, bashLeaptAt) : "from where he stood", 100f * bashOutFar, bashDriveTime) + ": under way";
            if (braced)
            {
                LastBash = LastBash.Replace("a " + levers.Name + " bash:", "a " + levers.Name + " bash, braced to counter a hard one:");
            }

            if (Diagnostics.PlaytestTrace.Enabled)
            {
                Diagnostics.PlaytestTrace.Note(FightUnit, "bash", (at != null ? $"at {at.FightUnit.name}, {gap:0.00} m off" : "asked for, at nobody") + "; " + LastBash);
            }

            if (kind == 2 && at != null && !braced)
            {
                // (A hard bash is seen coming: its crouch and its leap are its wind-up.)
                at.SeesHardBash(this, bashDriveAt + bashDriveTime);
            }

            return true;
        }

        // ---- Impacts that carry weight (18 November): the meter's eyes, and the door ----

        private float ShouldersSeen()
        {
            Vector3 across = Anim.Part(PartId.UpperArmR).position - Anim.Part(PartId.UpperArmL).position;
            return Mathf.Atan2(across.x, across.z) * Mathf.Rad2Deg;
        }

        /// <summary>The line from his hips to his head: what his spine and his chest turn together shows in it.</summary>
        private Vector3 UpSeen() => (Anim.Part(PartId.Head).position - Anim.Part(PartId.Pelvis).position).normalized;

        /// <summary>For the impact meter: how he is posed as an impact begins.</summary>
        internal void ImpactPose(ref ImpactShown shown)
        {
            Vector3 pelvis = Anim.Part(PartId.Pelvis).position;
            shown = new ImpactShown
            {
                Up0 = UpSeen(), Turn0 = ShouldersSeen() - Loco.Facing, Pelvis0 = pelvis, Head0 = Anim.Part(PartId.Head).position - pelvis,
                Arm0 = Anim.Part(PartId.HandL).position - Anim.Part(PartId.Chest).position, At0 = Loco.Position,
            };
        }

        /// <summary>For the impact meter: what he shows now, so long after the impact began.</summary>
        internal void ImpactWatch(ref ImpactShown shown, float since)
        {
            if (shown.To)
            {
                return;
            }

            Vector3 pelvis = Anim.Part(PartId.Pelvis).position;
            Vector3 head = Anim.Part(PartId.Head).position - pelvis - shown.Head0;
            head.y = 0f;
            shown.Rock = Mathf.Max(shown.Rock, Vector3.Angle(UpSeen(), shown.Up0));
            shown.Reacted = Mathf.Max(shown.Reacted, lean.magnitude * Mathf.Rad2Deg);
            shown.SwordArmGave = Mathf.Max(shown.SwordArmGave, knock[0].magnitude);
            shown.ShieldArmGave = Mathf.Max(shown.ShieldArmGave, knock[1].magnitude);
            shown.Turn = Mathf.Max(shown.Turn, Mathf.Abs(Mathf.DeltaAngle(shown.Turn0, ShouldersSeen() - Loco.Facing)));
            shown.Drop = Mathf.Max(shown.Drop, shown.Pelvis0.y - pelvis.y);
            shown.Head = Mathf.Max(shown.Head, head.magnitude);
            shown.Arm = Mathf.Max(shown.Arm, (Anim.Part(PartId.HandL).position - Anim.Part(PartId.Chest).position - shown.Arm0).magnitude);
            // (Bounce: the rock at its largest, and how far it then goes past where it set out from.)
            if (lean.sqrMagnitude > shown.LeanPeak.sqrMagnitude)
            {
                shown.LeanPeak = lean;
            }
            else if (shown.LeanPeak.sqrMagnitude > 1e-6f)
            {
                shown.SwungBack = Mathf.Max(shown.SwungBack, -Vector3.Dot(lean, shown.LeanPeak.normalized) * Mathf.Rad2Deg);
            }

            // (Shake: the seen body against its pose, at the head and the two hands.)
            if (Authority != null)
            {
                shown.OffPeak ??= new Vector3[3];
                shown.OffWas ??= new Vector3[3];
                shown.OffWay ??= new float[3];
                shown.OffRun ??= new float[3];
                for (int i = 0; i < 3; i++)
                {
                    var id = i == 0 ? PartId.Head : i == 1 ? PartId.HandL : PartId.HandR;
                    var part = Rig.Part(id);
                    if (part == null || Rig.IsSevered(id))
                    {
                        continue;
                    }

                    Vector3 off = part.transform.position - Anim.Part(id).position;
                    if (off.sqrMagnitude > shown.OffPeak[i].sqrMagnitude)
                    {
                        shown.OffPeak[i] = off;
                        shown.Off[i] = off.magnitude;
                    }
                    else if (shown.OffPeak[i].sqrMagnitude > 1e-6f)
                    {
                        shown.OffPast[i] = Mathf.Max(shown.OffPast[i], -Vector3.Dot(off, shown.OffPeak[i].normalized));
                    }

                    // (A change of direction: along the line it was knocked on, after a centimetre's travel one way.)
                    if (shown.OffPeak[i].sqrMagnitude > 1e-6f)
                    {
                        float moved = Vector3.Dot(off - shown.OffWas[i], shown.OffPeak[i].normalized);
                        if (moved * shown.OffWay[i] >= 0f)
                        {
                            shown.OffRun[i] += Mathf.Abs(moved);
                            shown.OffWay[i] = moved != 0f ? Mathf.Sign(moved) : shown.OffWay[i];
                        }
                        else if (Mathf.Abs(moved) > 1e-5f)
                        {
                            if (shown.OffRun[i] >= 0.01f * Size)
                            {
                                shown.HeadTurns += i == 0 ? 1 : 0;
                                shown.ShieldHandTurns += i == 1 ? 1 : 0;
                                shown.SwordHandTurns += i == 2 ? 1 : 0;
                            }

                            shown.OffRun[i] = Mathf.Abs(moved);
                            shown.OffWay[i] = Mathf.Sign(moved);
                        }
                    }

                    shown.OffWas[i] = off;
                }

                if (ImpactMeter.Trace)
                {
                    var peaks = shown.OffPeak;
                    Vector3 Along(int i) => peaks[i].sqrMagnitude > 1e-6f ? peaks[i].normalized : Vector3.forward;
                    Debug.Log(string.Format(System.Globalization.CultureInfo.InvariantCulture, "SHAKE {0} +{1:0.000}: off its pose, cm along the way each was knocked: head {2:0.0} shield hand {3:0.0} sword hand {4:0.0}; the rock {5:0.0} deg; shield arm knock {6:0.0} cm; hit stop {7}",
                        FightUnit != null ? FightUnit.name : "?", since, 100f * Vector3.Dot(shown.OffWas[0], Along(0)), 100f * Vector3.Dot(shown.OffWas[1], Along(1)), 100f * Vector3.Dot(shown.OffWas[2], Along(2)),
                        lean.magnitude * Mathf.Rad2Deg, 100f * knock[1].magnitude, ImpactStop > 0f ? "on" : "off"));
                }
            }

            if (!shown.From && since >= ImpactMeter.SpeedFrom)
            {
                shown.From = true;
                shown.AtFrom = Loco.Position;
            }

            if (!shown.Sped && since >= ImpactMeter.SpeedTo)
            {
                shown.Sped = true;
                shown.AtTo = Loco.Position;
            }

            if (since >= ImpactMeter.Watches)
            {
                shown.To = true;
            }
        }

        /// <summary>
        /// A bash lands as an impact (the plan "Impacts that carry weight", step 2; Combat Feel "A Bash Lands As An
        /// Impact"). What it hands the other is an amount of momentum at the point the board meets him, through the
        /// reactions a sword blow has: his trunk rocked about his hips by the lever of where it landed, his body bent
        /// there with his knees taking it, the part gone soft, force on his physical body; on a shield (held in
        /// cover, or driven out in a counter) his shield arm driven back and his guard knocked open by its weight.
        /// The basher takes the same amount back at his own shield arm: equal and opposite. Both stand still a
        /// moment as it lands. Nothing of it is drawn by lot: what differs from one bash to the next is where the
        /// board really met him, which way it was really going and how the two really stood. Off: a bash only
        /// changes the other's speed over the ground, or knocks him over, as before.
        /// </summary>
        public static bool BashIsImpact = true;

        /// <summary>
        /// How much of a bash's momentum rocks a trunk (against a sword blow's 1: a body's whole weight arriving is ten
        /// times a cut's, and a trunk can only rock so far), how much of it is put on the physical body as force, and
        /// how much of what a bash meets the basher takes back when it meets a body that gives (met by a shield he
        /// takes all of it).
        /// </summary>
        public static float BashRocks = 0.17f, BashForce = 0.12f, BashBackOffBody = 0.5f;

        /// <summary>The most a body's impact may rock a trunk (radians, the spine and the chest together; a blade's blow, <see cref="MostLean"/>).</summary>
        public static float BashRocksMost = 1f;

        /// <summary>How long both stand still as a bash lands: so much (seconds) and so much more for each m/s of the shove, within the blows' own least and most.</summary>
        public const float BashStopBase = 0.03f, BashStopPerSpeed = 0.02f;

        /// <summary>Both stand still a moment as a bash lands (off: no hit stop for a bash, as before).</summary>
        public static bool BashStops = true;

        /// <summary>The poise a bash costs the one it meets, for each m/s of its shove.</summary>
        public const float BashPoise = 3f;

        /// <summary>Both fighters of an impact stand still this long yet (seconds): the hit stop of an impact that is no blade's.</summary>
        public float ImpactStop;

        /// <summary>
        /// The door for a body's impact: <paramref name="impulse"/> (N·s, world) at <paramref name="point"/>, on his
        /// shield or on his body. His reactions only: what it does to his speed over the ground is the caller's.
        /// </summary>
        /// <param name="poise">The poise it costs him.</param>
        /// <param name="taken">He is the one it was dealt to (it may knock his guard open, and he was struck); off: it is what comes back to the one who dealt it.</param>
        public void TakeImpact(Vector3 impulse, Vector3 point, bool onShield, float rocks, float force, float poise = 0f, bool taken = true)
        {
            float j = impulse.magnitude;
            if (j < 1e-3f || Fallen)
            {
                return;
            }

            if (taken)
            {
                lastHit = clock;
                Poise = Mathf.Max(0f, Poise - poise);
            }

            if (Upper != null && CombatFeel.HitForce > 0f && force > 0f)
            {
                HitForceGiven += Upper.Strike(onShield ? PartId.ForearmL : PartId.Chest, impulse * (force * CombatFeel.HitForce * (onShield ? HeldJolt : 1f)), point);
                HitForces++;
            }

            // (The trunk about the hips, by the lever of where it really landed: high, he is rocked back; off his
            // middle, he is turned.)
            // (What his shield takes comes into his trunk at his shield shoulder: that is where its lever is.)
            Recoil(impulse, onShield ? Anim.Part(PartId.UpperArmL).position : point, onShield ? PartId.HandL : PartId.Chest, rocks * ReactScale, BashRocksMost);
            float weight = j / Mathf.Max(1f, Mass);
            if (onShield)
            {
                // (The shield arm gives by the weight of it, as to a blow; heavy enough, the guard is knocked open.)
                bool open = taken && weight >= CombatFeel.GuardOpenFrom * GuardHolds;
                float give = Mathf.Min(CombatFeel.ShieldGivePerWeight * weight / GuardHolds, CombatFeel.MostShieldGive) * Size;
                // (No further than the room his hand has before his chest: a board driven back stops against him.)
                float room = Vector3.Dot(Anim.Part(PartId.HandL).position - Anim.Part(PartId.Chest).position, -impulse / j) - 0.2f * Size;
                give = Mathf.Min(open ? Mathf.Max(give, CombatFeel.MostShieldGive * Size) : give, Mathf.Max(0.03f * Size, room));
                KnockArmBy(1, impulse, point, give, ArmKnockFrequency);
                if (open)
                {
                    guardOpenUntil = Mathf.Max(guardOpenUntil, clock + CombatFeel.GuardOpenFor);
                    GuardsOpened++;
                }

                Soften(BodyRegion.ArmL, 0.4f, 0.4f);
                // (What the arm does not take goes on into the body behind it.)
                AddImpact(point, impulse * (rocks * 0.5f));
            }
            else
            {
                AddImpact(point, impulse * rocks);
                Soften(BodyRegion.Torso, 0.3f, 0.6f);
            }

            Touches++;
        }

        // ---- The bash after Renier's first look at stumbles (19 November, morning) ----------------------------------

        /// <summary>
        /// A bash lands when its board touches him (Combat Feel "A Bash Lands When The Board Touches Him"): the board's
        /// middle within <see cref="BashTouches"/> of his own middle, or within <see cref="BoardsTouch"/> of his board if
        /// that is between them. Off: as before, from <see cref="BashMeets"/> off (46 cm from his middle, some 30 cm of
        /// air before the board). Seen on the 19th: with the ledger the basher is checked the moment it "lands", so the
        /// board stopped short and he flew off untouched.
        /// </summary>
        public static bool BashLandsOnTouch = true;

        /// <summary>How near his middle the board's middle is when it touches his body, and how near his board's middle when it touches his board (metres at a Medium's size, along the bash).</summary>
        public const float BashTouches = 0.22f, BoardsTouch = 0.12f;

        /// <summary>How far from his middle a bash lands, for planning its leap (metres at a Medium's size).</summary>
        public static float BashLandsFrom => BashLandsOnTouch ? BashTouches : BashMeets;

        /// <summary>
        /// How much speed his arm and legs add to his board while the two are pressed together (m/s), for the small, the
        /// medium, the hard and the extra hard bash (Combat Feel "… Bash Pushes Through"). He drives on off the ground,
        /// so it is not taken out of his own going. It is what makes a bash a push and not two bodies colliding.
        /// </summary>
        public static readonly float[] BashPushesThrough = { DefaultSmallPushes, DefaultMediumPushes, DefaultHardPushes, DefaultExtraHardPushes };

        public const float DefaultSmallPushes = 2.4f, DefaultMediumPushes = 0.3f, DefaultHardPushes = 2f, DefaultExtraHardPushes = 6f;

        private static Vector3 theirsOrBoard(ClipFighter enemy, Vector3 board) => enemy.ShieldBoard(out Vector3 theirs, out _, out _) ? theirs : board + Vector3.right * 9f;

        /// <summary>This bash is the extra hard one: a hard bash driven through with everything he has (<see cref="BashPushesThrough"/>'s last).</summary>
        private bool bashExtra;

        /// <summary>
        /// After a bash that leaves the other on his feet the basher does not walk on into him: what is left of his
        /// going forward his own legs take (Combat Feel "After A Bash He Holds His Ground"). Off: he goes on at what the
        /// ledger left him, and the two end board against board.
        /// </summary>
        public static bool BashHoldsGround = true;

        // ---- Off balance: stumbles and falls (19 November) ---------------------------------------------------------

        /// <summary>His stumble, while he is off balance (<see cref="StumbleSteps"/>: real stumble steps, turned round, or down).</summary>
        public readonly StumbleSteps Stagger = new StumbleSteps();

        /// <summary>Stumbles on real stumble steps: begun, ended on his feet, ended down.</summary>
        public int StumblesBegun, StumblesCaught, StumblesDown;

        /// <summary>The last stumble, in words: what threw him, his steps, and how it ended.</summary>
        public string LastStumble = "";

        /// <summary>A stumble began or ended: who, and what happened, in words. For the logs and the lab.</summary>
        public static event System.Action<ClipFighter, string> Stumbled;

        /// <summary>What he turns like about the upright: so much of his weight times his size squared (kg m2 for each kg, at a Medium's size), his arms, shield and sword out from him.</summary>
        public const float TurnsLike = 0.03f;

        /// <summary>How far each m/s a landing foot takes out of his hips rocks his upper body on, the way he was going (radians), and the most of it.</summary>
        public const float FootfallRocks = 0.09f, MostFootfallRock = 0.3f;

        private Vector3 staggerGoing, staggerAt, staggerLead, pendingPoint, pendingImpulse;
        private string staggerWhy = "";

        /// <summary>
        /// The door for whatever throws his body: a change of his speed over the ground (m/s, world), with the impulse
        /// that made it and the point it came in at (its lever about his middle is what turns him). What comes of it is
        /// his legs' to decide (<see cref="StumbleSteps"/>): inside what his feet hold, his stance steps it off
        /// ("shoved"); past it he is off balance and stumbles ("stumbled"), and at the end of that he has kept his feet
        /// or is down. Nothing is drawn by lot.
        /// </summary>
        public string ThrowBody(Vector3 change, Vector3 point, Vector3 impulse, string why)
        {
            change.y = 0f;
            if (Fallen)
            {
                return "down";
            }

            PressInto(impulse, change);

            if (change.sqrMagnitude < 1e-6f)
            {
                return "shoved";
            }

            float twist = impulse.sqrMagnitude > 1e-6f ? Vector3.Cross(Flat(point - Loco.Position), Flat(impulse)).y : 0f;
            float spin = StumbleSteps.TurnedBy * Mathf.Rad2Deg * twist / (TurnsLike * Mathf.Max(1f, Mass) * Size * Size);
            if (Stagger.Active)
            {
                Stagger.Add(change, spin);
                staggerGoing = Stagger.Vel;
                return "stumbled";
            }

            if (Anchored || !Loco.Guarding || !Loco.Ready)
            {
                Loco.Push(change);
                return "shoved";
            }

            Vector3 going = Flat(Loco.Velocity) + change;
            Poser.FeetNow(out Vector3 left, out Vector3 right, out float leftYaw, out float rightYaw);
            // (His weight is where his hips are: in a lunge or a leap that is not where his root is.)
            Vector3 hips = Flat(Anim.Part(PartId.Pelvis).position);
            // (Off balance by what was done to him, not by his own going: a man walking is not falling.)
            if (!StumbleSteps.OffBalance(hips, change, left, right, Loco.Leg))
            {
                Loco.Push(change);
                return "shoved";
            }

            // (A blow of his own is dropped: he has his feet to find.)
            Combo?.Stop();
            Built?.End();
            float already = going.sqrMagnitude > 1e-4f ? Mathf.Max(0f, Vector3.Dot(Flat(Loco.Velocity), going.normalized)) : 0f;
            Stagger.Begin(hips, going, Loco.Facing, spin, left, right, leftYaw, rightYaw, Loco.Leg, already);
            staggerLead = hips - Flat(Loco.Position);
            Poser.Bear(Stagger);
            // (19 November, tried and taken out: his sword arm made loose for 0.7 s as a stumble began, so it would be
            // thrown out by his body's going. In a fight of six his loose blade went through his own board (the seen tip
            // 27 to 31 cm off its pose) and the crossings went over their budget. Arms in a stumble want a pose of their own.)
            Loco.Velocity = new Vector3(going.x, Loco.Velocity.y, going.z);
            staggerGoing = going;
            staggerAt = Flat(Loco.Position);
            staggerWhy = why;
            StumblesBegun++;
            StumbleHandsBegan = StumbleHandsWidest = StumbleArmOutMost = 0f;
            thrownAt = clock;
            pushAt = -1f;
            pendingKnock = Vector3.zero;
            string said = string.Format(System.Globalization.CultureInfo.InvariantCulture, "off balance from {0}: thrown at {1:0.0} m/s (his speed changed by {3:0.0}), turning at {2:0} degrees a second", why, going.magnitude, Stagger.Spin, change.magnitude);
            LastStumble = said;
            Stumbled?.Invoke(this, said);
            if (Diagnostics.PlaytestTrace.Enabled)
            {
                Diagnostics.PlaytestTrace.Note(FightUnit, "stumble", said);
            }

            return "stumbled";
        }

        /// <summary>A step of his stumble: his body carried by it, his feet stepping for it, his upper body rocked on as each foot checks his hips.</summary>
        private void StepStagger(float dt)
        {
            // (Whatever has moved him at once since the last step (two bodies kept apart): the stumble goes with it. Put
            // somewhere else altogether, it is over.)
            Vector3 moved = Flat(Loco.Position) - staggerAt;
            if (moved.sqrMagnitude > 0.25f)
            {
                Poser.Unbear();
                Stagger.End();
                Loco.Step(dt, Vector3.zero, Loco.Facing);
                return;
            }

            Stagger.Com += moved;
            // (His hips were ahead of his root as it began (a lunge, a leap): his stand comes under them.)
            staggerLead *= Mathf.Exp(-dt / 0.1f);
            // (Whatever has pushed him since the last step went into his speed: the stumble has it.)
            Vector3 pushed = Flat(Loco.Velocity) - staggerGoing;
            if (pushed.sqrMagnitude > 1e-6f)
            {
                Stagger.Add(pushed, 0f);
            }

            Stagger.Step(dt);
            staggerGoing = Stagger.Vel;
            Loco.Carried(dt, Stagger.Com - staggerLead, Stagger.Facing, Stagger.Vel);
            staggerAt = Flat(Loco.Position);
            Vector3 braked = Stagger.TakeBraked();
            if (braked.sqrMagnitude > 1e-6f)
            {
                // (A foot checks his hips; what is above them goes on.)
                Vector3 axis = Vector3.Cross(Vector3.up, -braked.normalized);
                float amount = Mathf.Min(FootfallRocks * braked.magnitude, MostFootfallRock) * CombatFeel.LeanScale;
                leanMost = Mathf.Max(leanMost, BashRocksMost);
                leanV += axis * (amount * Kick * 2f * Mathf.PI * ReactFrequency * ReactGain);
            }

            // (His upper body tips with his weight: past the foot behind him the way he goes, back against a foot
            // landed ahead of him.)
            Vector3 tilt = Stagger.Tilt;
            tipWanted = tilt.sqrMagnitude > 1e-8f ? Vector3.Cross(Vector3.up, tilt.normalized) * Mathf.Min(StumbleSteps.TrunkTips * tilt.magnitude, MostTip) : Vector3.zero;
            thrownAt = clock;
            if (!Stagger.Active)
            {
                EndStagger();
            }
        }

        // ---- Kicks (the fight rhythm plan's step 9b: a first build, 20 November) ----

        /// <summary>
        /// A fighter can kick (Combat Feel "A Kick Throws A Body"; lab only for now: "Kick him: Front kick"). The
        /// sword-and-shield pack's one kick, a front kick with the back foot (<see cref="KickClip"/>, 1.2 s, the foot
        /// furthest out and chest high 0.57 s in), played whole, turned to the other and stretched or shortened so the
        /// foot ends a little way into him. As the foot meets his body, or his board, he is thrown through the door
        /// every impact uses (<see cref="ThrowBody"/>): shoved, stumbling or down is his legs' to decide. No fighter
        /// kicks by himself yet.
        /// </summary>
        public static bool KicksThrow = true;

        /// <summary>How fast a kick throws a man of the kicker's own weight (m/s at a Medium's size: the lever "Kick power"), and how far past the other's front the foot is aimed (metres at a Medium's size: "Kicks into him").</summary>
        public static float KickPower = DefaultKickPower, KickGoesIn = DefaultKickGoesIn;

        public const float DefaultKickPower = 2.6f, DefaultKickGoesIn = 0.25f;

        /// <summary>The recording, when its foot is furthest out (seconds in), from when and until when it can land, how far ahead of where he began the foot then is and how far his hips have gone (leg lengths), how near the foot must come to the other's middle line or his board (metres at a Medium's size), how much of a kick a board takes, and how long before he may kick again.</summary>
        public const string KickClip = "sns_kick";
        public const float KickLandsAt = 0.57f, KickLandsFrom = 0.38f, KickLandsUntil = 0.68f, KickFootOut = 1.81f, KickHipsGo = 0.62f, KickMeetsBody = 0.24f, KickMeetsBoard = 0.3f, KickOnBoard = 0.6f, KickEvery = 2f;

        /// <summary>How far a kick rocks his upper body back for each m/s it throws him, and at the most (radians).</summary>
        public const float KickRocks = 0.09f, MostKickRock = 0.4f;

        private float kickFor = -1f, kickEnds, kickReadyAt, kickFootFastest;
        private ClipFighter kickAt;
        private bool kickStruck, kickFootKnown;
        private Vector3 kickWay, kickFootWas;

        public bool Kicking => kickFor >= 0f;
        public int Kicks, KicksLanded, KickedTimes;
        public string LastKick = "";

        /// <summary>He kicks now, at this enemy (null: straight ahead, at nobody, to be seen). False: he cannot (down, stumbling, in a blow, a bash or another recording, or one just thrown).</summary>
        public bool KickAt(ClipFighter at, bool whenever = false)
        {
            if (!KicksThrow || Kicking || Bashing || Fallen || Striking || Stagger.Active || !Loco.Ready || Loco.Current == Locomotor.Mode.OneShot || (!whenever && clock < kickReadyAt))
            {
                return false;
            }

            var clip = LocoLibrary.Get(KickClip);
            if (clip == null)
            {
                return false;
            }

            Vector3 ahead = Quaternion.Euler(0f, Loco.Facing, 0f) * Vector3.forward;
            Vector3 to = at != null ? Flat(at.Loco.Position - Loco.Position) : ahead;
            float gap = at != null ? to.magnitude : 0f;
            kickWay = at != null && gap > 1e-3f ? to / gap : ahead;
            // (The foot is to end "Kicks into him" past his front. The recording's foot goes so far; what is over or
            // short of that is made up by how far his hips go in.)
            float reach = KickFootOut * Loco.Leg, hips = KickHipsGo * Loco.Leg;
            float wanted = at != null ? gap - 0.2f * at.Size + KickGoesIn * Size : reach;
            float stretch = Mathf.Clamp(1f + (wanted - reach) / Mathf.Max(0.05f, hips), 0.2f, 1.8f);
            float turn = Mathf.DeltaAngle(Loco.Facing, Mathf.Atan2(kickWay.x, kickWay.z) * Mathf.Rad2Deg);
            Loco.PlayAimed(clip, stretch, turn, 0.25f);
            kickFor = 0f;
            kickEnds = clip.Duration;
            kickAt = at;
            kickStruck = kickFootKnown = false;
            kickFootFastest = 0f;
            kickReadyAt = clock + KickEvery;
            Kicks++;
            LastKick = string.Format(System.Globalization.CultureInfo.InvariantCulture, "a front kick{0}, his hips going in {1:0.00} of the recording's: under way", at != null ? $" from {gap:0.00} m" : " at nobody", stretch);
            if (Diagnostics.PlaytestTrace.Enabled)
            {
                Diagnostics.PlaytestTrace.Note(FightUnit, "kick", (at != null ? $"at {at.FightUnit.name}, {gap:0.00} m off" : "asked for, at nobody") + "; " + LastKick);
            }

            return true;
        }

        /// <summary>A kick under way, a step on: his foot followed, and the kick landed as the foot meets the other's body or his board.</summary>
        private void StepKick(float dt)
        {
            if (kickFor < 0f)
            {
                return;
            }

            kickFor += dt;
            Vector3 foot = Anim.Part(PartId.FootR).position;
            Vector3 footGoes = kickFootKnown && dt > 0f ? (foot - kickFootWas) / dt : Vector3.zero;
            kickFootWas = foot;
            kickFootKnown = true;
            kickFootFastest = Mathf.Max(kickFootFastest, footGoes.magnitude);
            if (!kickStruck && kickAt != null && !kickAt.Fallen && kickFor >= KickLandsFrom && kickFor <= KickLandsUntil)
            {
                Vector3 low = kickAt.Anim.Part(PartId.Pelvis).position, high = kickAt.Anim.Part(PartId.Head).position;
                Vector3 line = high - low;
                float along = line.sqrMagnitude > 1e-6f ? Mathf.Clamp01(Vector3.Dot(foot - low, line) / line.sqrMagnitude) : 0f;
                Vector3 on = low + line * along;
                bool board = kickAt.ShieldColliders.Count > 0 && !kickAt.Rig.IsSevered(PartId.HandL)
                    && (foot - kickAt.Anim.Part(PartId.HandL).position).magnitude <= KickMeetsBoard * kickAt.Size;
                bool bodyMet = (foot - on).magnitude <= KickMeetsBody * kickAt.Size;
                if (board || bodyMet)
                {
                    kickStruck = true;
                    KicksLanded++;
                    float weights = Mass / Mathf.Max(1f, kickAt.Mass);
                    float speed = KickPower * Mathf.Sqrt(Size) * Mathf.Min(weights, 1.5f) * (board && !bodyMet ? KickOnBoard : 1f);
                    Vector3 impulse = kickWay * (speed * kickAt.Mass);
                    string did = kickAt.Kicked(kickWay, speed, this, foot, impulse, board && !bodyMet);
                    // (What his leg met comes back up it: he is checked, and it presses him as any impact does.)
                    Loco.StopShotTravel();
                    PressInto(-0.5f * impulse, Vector3.zero);
                    LastKick = LastKick.Replace(": under way", string.Format(System.Globalization.CultureInfo.InvariantCulture,
                        ": it landed {0:0.00} s in on his {1}, {2:0} cm over the ground, the foot going {3:0.0} m/s: he was thrown at {4:0.0} m/s and {5}",
                        kickFor, board && !bodyMet ? "board" : "body", 100f * (foot.y - groundLevel), footGoes.magnitude, speed, did == "down" ? "went down" : did == "stumbled" ? "stumbled" : "was shoved"));
                    if (Diagnostics.PlaytestTrace.Enabled)
                    {
                        Diagnostics.PlaytestTrace.Note(FightUnit, "kick", LastKick);
                    }
                }
            }

            if (kickFor >= kickEnds || (kickFor > 0.2f && Loco.Current != Locomotor.Mode.OneShot) || Fallen)
            {
                if (!kickStruck)
                {
                    LastKick = LastKick.Replace(": under way", kickAt != null ? $": it met nothing (the foot at its fastest {kickFootFastest:0.0} m/s)" : ": at nobody");
                }

                kickFor = -1f;
                kickAt = null;
            }
        }

        /// <summary>He is kicked: thrown through the door, his upper body rocked back from it, his guard open a moment unless his board took it.</summary>
        public string Kicked(Vector3 way, float speed, ClipFighter by, Vector3 point, Vector3 impulse, bool onBoard)
        {
            if (Fallen)
            {
                return "down";
            }

            KickedTimes++;
            string did = ThrowBody(way * speed, point, impulse, "a kick");
            Vector3 axis = Vector3.Cross(Vector3.up, way);
            float amount = Mathf.Min(KickRocks * speed, MostKickRock) * CombatFeel.LeanScale * (onBoard ? 0.5f : 1f);
            leanMost = Mathf.Max(leanMost, BashRocksMost);
            leanV += axis * (amount * Kick * 2f * Mathf.PI * ReactFrequency * ReactGain);
            if (!onBoard)
            {
                guardOpenUntil = Mathf.Max(guardOpenUntil, clock + (did == "shoved" ? 0.3f : 0.8f));
            }

            if (Diagnostics.PlaytestTrace.Enabled)
            {
                Diagnostics.PlaytestTrace.Note(FightUnit, "kick", $"kicked by {(by != null && by.FightUnit != null ? by.FightUnit.name : "a foot")} on his {(onBoard ? "board" : "body")}: {(did == "down" ? "knocked down" : did == "stumbled" ? "thrown back stumbling" : "shoved back")} at {speed:0.0} m/s");
            }

            return did;
        }

        // ---- Bodies that absorb an impact (the plan "Impacts that carry weight", step 4: a first build, 20 November) ----

        /// <summary>
        /// A body takes an impact in its knees and hips (Combat Feel "A Body Takes An Impact In Its Knees And Hips";
        /// <c>FIGHT_PRESS=0</c> for the old way). Whatever comes in through the door (<see cref="ThrowBody"/>: a bash
        /// on him, his own bash meeting a board, a heavy blow) presses him down as it lands: his knees give and his
        /// hips drop, by how much it changes his speed, over about a fifth of a second, and he comes up again over
        /// half a second; and his trunk folds a little toward where it came from. Both men of a bash. Before, only
        /// a man thrown off balance sank (his stumble's knees), so a basher who kept his feet stood on stiff legs
        /// while his board met a body (his hips down 4 cm on a held hard counter).
        /// </summary>
        public static bool BodiesTakeImpacts = true;

        /// <summary>How long an impact takes to press him down (seconds: the lever "Contact time"); how far his hips drop for an impact that changes his speed by <see cref="PressFullAt"/> (metres on a Medium: "Knees give"); how far his trunk folds toward it then (degrees: "Trunk folds").</summary>
        public static float ContactTime = DefaultContactTime, KneesGiveToImpact = DefaultKneesGiveToImpact, TrunkFoldsToImpact = DefaultTrunkFoldsToImpact;

        public const float DefaultContactTime = 0.2f, DefaultKneesGiveToImpact = 0.10f, DefaultTrunkFoldsToImpact = 4f;

        /// <summary>The change of speed that presses him the whole of "Knees give" (m/s), and the most of it one impact presses him.</summary>
        public const float PressFullAt = 3f, PressMostOf = 1.5f;

        private float press, pressV, pressPosed;
        private Vector3 pressFrom;

        /// <summary>For the reports: the furthest his hips were pressed down by an impact since the last one began (metres), and how many impacts pressed him.</summary>
        public float PressedMost;
        public int PressedTimes;

        /// <summary>An impact comes in: it presses him down by how much it changes his speed.</summary>
        private void PressInto(Vector3 impulse, Vector3 change)
        {
            if (!BodiesTakeImpacts || KneesGiveToImpact <= 0f)
            {
                return;
            }

            impulse.y = 0f;
            float turned = Mathf.Max(impulse.magnitude / Mathf.Max(1f, Mass), change.magnitude);
            if (turned < 0.3f)
            {
                return;
            }

            // (A spring that stands him up again as quickly as it lets him down, set going so that it is at its
            // lowest, by this much, a contact time after the impact.)
            float w = 1f / Mathf.Max(0.05f, ContactTime);
            float lowest = KneesGiveToImpact * Size * Mathf.Min(turned / PressFullAt, PressMostOf);
            pressV += lowest * Mathf.Exp(1f) * w;
            Vector3 from = impulse.sqrMagnitude > 1e-6f ? -impulse.normalized : change.sqrMagnitude > 1e-6f ? -change.normalized : Vector3.zero;
            pressFrom = from;
            PressedMost = 0f;
            PressedTimes++;
        }

        /// <summary>The press of an impact, a step on: his hips down by it, his trunk folded toward where it came from.</summary>
        private void StepPress(float dt)
        {
            // (What his hands have settled to goes down and up with him. It is kept against his root, which does not
            // dip: left where it was, his board lagged his body's dip and his sword did not, and in the fights of six
            // his blade was through his own board more on his feet, 0.7% of the time to 0.8% and more.)
            float dipped = Stagger.Active ? pressPosed : Poser.Pressed;
            if (Mathf.Abs(dipped - pressPosed) > 1e-6f)
            {
                for (int a = 0; a < heldAt.Length; a++)
                {
                    heldAt[a] += Vector3.down * (dipped - pressPosed);
                }
            }

            pressPosed = dipped;
            if (press <= 1e-5f && Mathf.Abs(pressV) <= 1e-5f)
            {
                Poser.Pressed = 0f;
                return;
            }

            float w = 1f / Mathf.Max(0.05f, ContactTime);
            pressV += (-w * w * press - 2f * w * pressV) * dt;
            press += pressV * dt;
            if (press < 0f)
            {
                press = 0f;
                pressV = Mathf.Max(0f, pressV);
            }

            press = Mathf.Min(press, PressMostOf * KneesGiveToImpact * Size);
            PressedMost = Mathf.Max(PressedMost, press);
            Poser.Pressed = Fallen ? 0f : press;
            float folds = TrunkFoldsToImpact * press / Mathf.Max(1e-3f, KneesGiveToImpact * Size);
            if (folds > 0.05f && pressFrom.sqrMagnitude > 1e-6f && !Fallen)
            {
                Vector3 axis = Vector3.Cross(Vector3.up, pressFrom);
                var spine = Anim.Part(PartId.Spine);
                var chest = Anim.Part(PartId.Chest);
                spine.rotation = Quaternion.AngleAxis(0.45f * folds, axis) * spine.rotation;
                chest.rotation = Quaternion.AngleAxis(0.55f * folds, axis) * chest.rotation;
            }
        }

        // ---- A stumbling fighter's arms (the night of 19 November) ----

        /// <summary>
        /// A stumbling fighter's arms go out for balance (Combat Feel "A Stumbling Fighter's Arms Go Out For Balance";
        /// <c>FIGHT_ARMSOUT=0</c> for the old way, <c>STUMBLE_ARMS=1.5</c> the lever). Until now his arms did nothing
        /// of their own in a stumble: they held the guard they had while his legs and trunk fought for his footing.
        /// Now each hand goes out to its side and up a little by how fast he is going, the one on the far side from
        /// where he tips the further, and both are left behind a little by the way he is thrown. They come back as
        /// he catches himself. (Tried first: turning his upper arms out at the shoulder. His guard has his hands
        /// straight ahead of his shoulders, so his elbows flared 25 to 64 degrees and his hands hardly moved.)
        /// </summary>
        public static bool ArmsGoOutInAStumble = true;

        /// <summary>How much of it (the lever "Arms go out by"): 1 as built, 0 none.</summary>
        public static float ArmsGoOutBy = DefaultArmsGoOutBy;

        public const float DefaultArmsGoOutBy = 1f;

        /// <summary>How far each hand goes out to its side for each m/s he goes and at the most (metres, on a Medium); how much of that it also lifts; how far both are left behind for each m/s and at the most (metres); how quickly they go out and come back (1/s).</summary>
        public const float ArmsOutPerSpeed = 0.09f, ArmsOutMost = 0.36f, ArmsOutLifts = 0.4f, ArmsBehindPerSpeed = 0.05f, ArmsBehindMost = 0.2f, ArmsOutQuick = 12f, ArmsOutBack = 5f;

        /// <summary>How far what a hand holds is turned out to its side with the hand, at the most the hand goes out (degrees).</summary>
        public const float ArmsOutTurns = 40f;

        private readonly float[] armOut = new float[2];
        private Vector3 armBehind;

        /// <summary>For the reports, of the stumble under way or the last one: how far apart his hands were as it began and at the widest (metres), and the furthest a hand was moved out (metres).</summary>
        public float StumbleHandsBegan, StumbleHandsWidest, StumbleArmOutMost;

        private void StepArmsOut(float dt)
        {
            float outL = 0f, outR = 0f;
            Vector3 behind = Vector3.zero;
            Vector3 across = Anim.Part(PartId.UpperArmR).position - Anim.Part(PartId.UpperArmL).position;
            across.y = 0f;
            across = across.sqrMagnitude > 1e-8f ? across.normalized : Quaternion.Euler(0f, Loco.Facing, 0f) * Vector3.right;
            if (Stagger.Active && StumbleHandsBegan <= 0f)
            {
                StumbleHandsBegan = (Anim.Part(PartId.HandR).position - Anim.Part(PartId.HandL).position).magnitude;
            }

            if (ArmsGoOutInAStumble && Stagger.Active && ArmsGoOutBy > 0f)
            {
                float speed = Stagger.Vel.magnitude;
                float by = Mathf.Min(ArmsOutMost, ArmsOutPerSpeed * speed) * ArmsGoOutBy * Size;
                Vector3 tilt = Stagger.Tilt;
                // (Tipping to his right, his left arm goes out the further: the far arm is the one that balances him.)
                float tips = tilt.sqrMagnitude > 1e-8f ? Mathf.Clamp(Vector3.Dot(tilt.normalized, across), -1f, 1f) : 0f;
                outL = by * (1f + 0.4f * tips);
                outR = by * (1f - 0.4f * tips);
                if (speed > 0.05f)
                {
                    // (Left behind by the way he is thrown.)
                    behind = -Stagger.Vel / speed * (Mathf.Min(ArmsBehindMost, ArmsBehindPerSpeed * speed) * ArmsGoOutBy * Size);
                }
            }

            armOut[0] = Mathf.Lerp(armOut[0], outL, 1f - Mathf.Exp(-(outL > armOut[0] ? ArmsOutQuick : ArmsOutBack) * dt));
            armOut[1] = Mathf.Lerp(armOut[1], outR, 1f - Mathf.Exp(-(outR > armOut[1] ? ArmsOutQuick : ArmsOutBack) * dt));
            armBehind = Vector3.Lerp(armBehind, behind, 1f - Mathf.Exp(-(behind.sqrMagnitude > armBehind.sqrMagnitude ? ArmsOutQuick : ArmsOutBack) * dt));
            if (armOut[0] < 0.002f && armOut[1] < 0.002f && armBehind.sqrMagnitude < 4e-6f)
            {
                return;
            }

            // (Each hand out to its own side and up a little, and both left behind: the arm follows its hand, bent the
            // way it was bent. Not an arm in a blow of its own, nor one cut off.)
            for (int a = 0; a < 2; a++)
            {
                bool isLeft = a == 0;
                if (Rig.IsSevered(isLeft ? PartId.HandL : PartId.HandR) || (!isLeft && (Striking || clock < weaponArmHeldUntil)))
                {
                    continue;
                }

                var upper = Anim.Part(isLeft ? PartId.UpperArmL : PartId.UpperArmR);
                var fore = Anim.Part(isLeft ? PartId.ForearmL : PartId.ForearmR);
                var hand = Anim.Part(isLeft ? PartId.HandL : PartId.HandR);
                Quaternion turned = hand.rotation;
                Vector3 to = hand.position + across * ((isLeft ? -1f : 1f) * armOut[a]) + Vector3.up * (ArmsOutLifts * armOut[a]) + armBehind;
                // (Never further than the arm is long.)
                float reach = 0.97f * ((fore.position - upper.position).magnitude + (hand.position - fore.position).magnitude);
                Vector3 from = to - upper.position;
                if (from.magnitude > reach)
                {
                    to = upper.position + from.normalized * reach;
                }

                TwoBoneIK.Solve(upper, fore, hand, to, TwoBoneIK.BendWay(upper, fore, hand));
                // (What it holds turns out with it, each to its own side: moved out and still turned as his guard had
                // it, his blade went through his own board, 0.15 s in a fight of six.)
                float turnsOut = ArmsOutTurns * Mathf.Clamp01(armOut[a] / Mathf.Max(1e-4f, ArmsOutMost * Size));
                hand.rotation = Quaternion.AngleAxis((isLeft ? -1f : 1f) * turnsOut, Vector3.up) * turned;
            }

            if (Stagger.Active)
            {
                StumbleHandsWidest = Mathf.Max(StumbleHandsWidest, (Anim.Part(PartId.HandR).position - Anim.Part(PartId.HandL).position).magnitude);
                StumbleArmOutMost = Mathf.Max(StumbleArmOutMost, Mathf.Max(armOut[0], armOut[1]));
            }
        }

        /// <summary>The most his upper body tips with a stumble (radians), and how quickly it follows (1/s).</summary>
        public const float MostTip = 0.7f, TipFollows = 14f;

        private Vector3 tip, tipWanted;

        /// <summary>His stumble is over: on his feet, which his stand has again; or down, the fall taking him as he goes.</summary>
        private void EndStagger()
        {
            bool down = Stagger.Ended == StumbleSteps.Ends.Down;
            tipWanted = Vector3.zero;
            Poser.Unbear();
            Stagger.End();
            string told = Stagger.Told();
            if (StumbleHandsBegan > 0f)
            {
                told += $"; his hands went out {100f * StumbleArmOutMost:0} cm at the most, from {100f * StumbleHandsBegan:0} to {100f * Mathf.Max(StumbleHandsWidest, StumbleHandsBegan):0} cm apart";
            }

            if (down)
            {
                StumblesDown++;
                float speed = Stagger.Vel.magnitude;
                Vector3 way = speed > 1e-2f ? Stagger.Vel / speed : -(Quaternion.Euler(0f, Loco.Facing, 0f) * Vector3.forward);
                LastStumble = $"{staggerWhy}: {told}; his legs could not catch him ({Stagger.Why}): down";
                KnockOver(way, Mathf.Clamp(speed, 0.5f, 3f), LastStumble, Mathf.Max(0.1f, speed));
            }
            else
            {
                StumblesCaught++;
                LastStumble = $"{staggerWhy}: {told}; he kept his feet";
                LastReaction = LastStumble;
            }

            Stumbled?.Invoke(this, LastStumble);
            if (Diagnostics.PlaytestTrace.Enabled)
            {
                Diagnostics.PlaytestTrace.Note(FightUnit, "stumble", LastStumble);
            }
        }

        // ---- A hard bash countered (18 November) ----

        /// <summary>
        /// A hard bash seen coming is met with a medium bash of his own (Combat Feel "A Hard Bash Is Countered By A
        /// Medium Bash"): he braces where he stands and drives his board out to be there as the other's arrives.
        /// Whether that stops the hard bash knocking him over is a draw (<see cref="CounterHolds"/>): held, the two
        /// boards meet, he is shoved back a step and the basher is stopped where he met him; not held, he goes down
        /// as if he had not countered. One who cannot bash just then (in a blow of his own, his guard open, no
        /// shield) does not counter. Off: a hard bash that meets him knocks him over every time, as before.
        /// </summary>
        public static bool CountersHardBash = true;

        /// <summary>How often a counter-bash stops a hard bash knocking him over (0 to 1). To be tied to his defensive skill later; a lever for now.</summary>
        public static float CounterHolds = 0.75f;

        /// <summary>How soon after a hard bash begins he can answer it (seconds), and how long before it arrives his own board must start out to be there.</summary>
        public const float CounterSeenIn = 0.12f, CounterLead = 0.3f;

        /// <summary>How near the two boards' middles must come for a hard bash to have met a counter (metres at a Medium's size).</summary>
        public const float BoardsMeet = 0.4f;

        /// <summary>How fast a basher is thrown back off the board that stopped him (m/s at a Medium's size).</summary>
        public const float CounterRebound = 1f;

        /// <summary>A hard bash was countered: who countered, whose bash it was, and whether the counter held. For the logs and the lab.</summary>
        public static event System.Action<ClipFighter, ClipFighter, bool> HardBashCountered;

        /// <summary>Counters he tried against a hard bash, and how many held; hard bashes of his own that were stopped by a counter.</summary>
        public int CountersTried, CountersHeld, BashesStopped;

        private ClipFighter counterAgainst;
        private float counterAt = -1f, counterUntil;
        private bool countering, counterHeld;
        private int bashKindAt;

        /// <summary>He is in a counter-bash against this basher.</summary>
        public bool Countering(ClipFighter basher) => countering && Bashing && bashAt == basher;

        private void SeesHardBash(ClipFighter by, float arrivesIn)
        {
            if (!CountersHardBash || ShieldColliders.Count == 0 || Fallen)
            {
                return;
            }

            counterAgainst = by;
            counterAt = clock + Mathf.Max(CounterSeenIn, arrivesIn - CounterLead);
            counterUntil = clock + arrivesIn + 0.25f;
        }

        private void StepCounter()
        {
            if (counterAt < 0f)
            {
                return;
            }

            var by = counterAgainst;
            if (by == null || !by.Bashing || by.bashStruck || Fallen || clock > counterUntil)
            {
                counterAt = -1f;
                counterAgainst = null;
                return;
            }

            if (clock < counterAt || Bashing)
            {
                return;
            }

            // (A gesture of his own is let go for it, as for a dodge; it ends over the next step, so he tries again.)
            if (DodgeDropsGesture && Posturing && Loco.Current == Locomotor.Mode.OneShot)
            {
                Loco.EndShotNow();
            }

            if (Bash(by, true, 1, true))
            {
                counterHeld = (float)shieldLuck.NextDouble() < CounterHolds;
                CountersTried++;
                counterAt = -1f;
                counterAgainst = null;
            }
        }

        /// <summary>He bashes this enemy if it is too near and before him (the fight asks it each step).</summary>
        public bool BashIfNear(ClipFighter enemy)
        {
            if (!ShieldBash || enemy == null || enemy.Fallen || Bashing || ShieldColliders.Count == 0)
            {
                return false;
            }

            Vector3 to = Flat(enemy.Loco.Position - Loco.Position);
            float far = to.magnitude;
            if (far > BashWithin * Size || far < 1e-3f || Vector3.Angle(Quaternion.Euler(0f, Loco.Facing, 0f) * Vector3.forward, to) > 60f)
            {
                return false;
            }

            return Bash(enemy);
        }

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);

        private void StepBash(float dt)
        {
            StepCounter();
            if (bashFor < 0f)
            {
                countering = false;
                return;
            }

            bashFor += dt;
            if (Fallen || GuardOpen)
            {
                LastBash = LastBash.Replace(": under way", ": broken off (he was struck or went down)");
                bashFor = -1f;
                return;
            }

            if (bashLeapt <= 0f && !bashSurged && bashFor >= bashDriveAt)
            {
                // (Standing: his body goes in behind the board.)
                bashSurged = true;
                Loco.Push(bashWay * (bashKind.Surge * Mathf.Sqrt(Size)));
            }

            // (The board meets him: its middle within reach of his middle, he before it. What it arrives with is what
            // it does: how fast it closes on him, his body's speed and his arm's together.)
            if (ShieldBoard(out Vector3 board, out _, out _))
            {
                // (The speed the board comes in at: the fastest it has gone toward him over the last sixth of a second, taken
                // over several steps. Step to step the physical board jitters; and one met just as the arm is all the way
                // out is met by the drive that got it there.)
                Vector3 stepped = bashBoardKnown && dt > 1e-5f ? (board - bashBoardWas) / dt : Vector3.zero;
                bashBoardWas = board;
                bool known = bashBoardKnown;
                bashBoardKnown = true;
                bashBoardSpeeds[bashBoardSpeedAt++ % bashBoardSpeeds.Length] = Vector3.Dot(Flat(stepped), bashWay);
                var enemy = bashAt;
                // (Not before the board is well on its way out, nor before he is off the ground in a leap: one that stood against
                // him already has no speed yet to meet him with.)
                if (!bashStruck && known && enemy != null && !enemy.Fallen && bashFor >= Mathf.Max(bashDriveAt + 0.5f * bashDriveTime, bashLeapt > 0f ? bashKind.Prep + 0.13f : 0f) && bashFor <= Mathf.Max(bashDriveAt + bashDriveTime + BashHeld + 0.1f, bashEnds - BashBack))
                {
                    Vector3 to = Flat(enemy.Loco.Position - board);
                    float off = to.magnitude;
                    Vector3 way = off > 1e-3f ? to / off : bashWay;
                    // (One who counters holds his own board out before him: the hard bash meets that, board on board,
                    // before it is near his middle.)
                    bool boards = bashKindAt == 2 && enemy.Countering(this) && enemy.ShieldBoard(out Vector3 theirs, out _, out _) && Flat(theirs - board).magnitude <= BoardsMeet * enemy.Size;
                    bool lands = off <= BashMeets * enemy.Size && Vector3.Dot(way, bashWay) > 0.2f;
                    if (BashLandsOnTouch)
                    {
                        // (On touch: his board against the other's body, or against the other's board if that is held between them.)
                        lands = off <= BashTouches * enemy.Size && Vector3.Dot(way, bashWay) > 0.2f;
                        if (!lands && !enemy.GuardOpen && enemy.ShieldBoard(out Vector3 hisBoard, out _, out float hisBoardRadius))
                        {
                            Vector3 across = hisBoard - board;
                            float along = Vector3.Dot(Flat(across), bashWay);
                            Vector3 aside = across - bashWay * Vector3.Dot(across, bashWay);
                            Vector3 outward = Flat(enemy.Loco.Position - Loco.Position).normalized;
                            lands = Vector3.Dot(Flat(hisBoard - enemy.Loco.Position), -outward) > 0.08f * enemy.Size && along > -0.1f * Size && along <= BoardsTouch * enemy.Size && aside.magnitude <= hisBoardRadius + 0.2f * Size;
                        }

                        boards &= Flat(theirsOrBoard(enemy, board) - board).magnitude <= (BoardsTouch + 0.1f) * enemy.Size;
                    }

                    if (lands || boards)
                    {
                        bashStruck = true;
                        // (Over the last eighth of a second: a board that was driven out a while ago and is only carried
                        // in by his body now arrives with his body's speed, not with the drive's.)
                        int counted = Mathf.Min(bashBoardSpeedAt, Mathf.Clamp(Mathf.RoundToInt(0.125f / Mathf.Max(1e-4f, dt)), 3, bashBoardSpeeds.Length));
                        for (int k = 0; k < counted; k++)
                        {
                            bashSorted[k] = bashBoardSpeeds[(bashBoardSpeedAt - 1 - k) % bashBoardSpeeds.Length];
                        }

                        System.Array.Sort(bashSorted, 0, counted);
                        float fastest = 0f;
                        int top = Mathf.Min(counted <= 8 ? 3 : 5, counted);
                        for (int k = 0; k < top; k++)
                        {
                            fastest += bashSorted[counted - 1 - k] / top;
                        }

                        // (A board pressed against him goes no faster than he gives way: it comes in at least as fast as the
                        // body behind it does.)
                        // (His body's own speed in: no more than his leap was asked to go, whatever one step's catching up
                        // made of it. And the arm's share of the board's speed counts for less than the body's: an arm
                        // alone has little behind it, a body arriving has all of him.)
                        float body = Mathf.Clamp(Vector3.Dot(Flat(Loco.Velocity), bashWay), 0f, bashLeapt > 0f ? 1.2f * bashLeaptAt * Size : 1.5f * bashKind.Surge * Mathf.Sqrt(Size));
                        fastest = Mathf.Max(fastest, body);
                        fastest = body + BashArmCounts * (fastest - body);
                        float closing = Mathf.Max(0f, fastest - Vector3.Dot(Flat(enemy.Loco.Velocity), way));
                        float shove = bashKind.Weight * closing * Mathf.Sqrt(Mass / Mathf.Max(1f, enemy.Mass));
                        BashesLanded++;
                        // (A hard bash he met with a bash of his own: if his counter holds, the boards meet, he is shoved
                        // back a step and the basher comes down where he met him; if not, he goes down for all that.)
                        bool countered = bashKindAt == 2 && enemy.Countering(this);
                        bool held = countered && enemy.counterHeld;
                        Vector3 out_ = Flat(enemy.Loco.Position - Loco.Position).normalized;
                        // (The ledger, the impacts plan's step 3: the speeds the two leave with are worked out from what
                        // each brought, not written in. How much of him is behind his board is the bash's weight. One who
                        // counters has set himself: his feet pass what their grip allows to the ground, by how many of
                        // them are down. And whether a counter holds is not drawn: it is what his legs can catch.)
                        bool worked = ImpactLedger.On;
                        ImpactLedger.Result ledger = default;
                        float hisSpeed = Vector3.Dot(Flat(enemy.Loco.Velocity), way);
                        if (worked)
                        {
                            float set = countered ? 0.5f * ((enemy.Poser.Feet[0].Locked ? 1f : 0f) + (enemy.Poser.Feet[1].Locked ? 1f : 0f)) : 0f;
                            ledger = ImpactLedger.Work(Mass * bashKind.Weight, fastest, enemy.Mass, hisSpeed, set, BashPushesThrough[bashExtra ? 3 : Mathf.Clamp(bashKindAt, 0, 2)]);
                            shove = Mathf.Max(0f, ledger.Taker - hisSpeed);
                            held = false;
                        }

                        // (What it meets: his board, driven out in a counter or held in cover between the two of them;
                        // or his body. And where: between the boards, or where this board is against him.)
                        bool onShield = countered;
                        Vector3 met = board;
                        float arrived = shove;
                        if (enemy.ShieldBoard(out Vector3 his, out _, out float hisRadius) && !enemy.GuardOpen)
                        {
                            Vector3 across = his - board;
                            float along = Vector3.Dot(Flat(across), bashWay);
                            Vector3 aside = across - bashWay * Vector3.Dot(across, bashWay);
                            bool between = Vector3.Dot(Flat(his - enemy.Loco.Position), -out_) > 0.08f * enemy.Size && along > -0.1f * Size && along < 0.6f * Size && aside.magnitude <= hisRadius + 0.2f * Size;
                            onShield |= between;
                            if (onShield)
                            {
                                met = 0.5f * (board + his);
                            }
                        }

                        var seen = ImpactMeter.Begin($"a {bashKind.Name} bash on {(countered ? "a counter-bash" : onShield ? "a shield in cover" : "the body")}", this, enemy, out_, Time.time);
                        if (BashIsImpact)
                        {
                            // (Through the door: the momentum the board arrives with, at the point it met him, the way the
                            // board was driven; and the same back at this one's own shield arm. Both stand still a moment.)
                            float handed = worked ? ledger.Through : shove * enemy.Mass, back = worked ? handed : handed * (onShield ? 1f : BashBackOffBody);
                            enemy.TakeImpact(bashWay * handed, met, onShield, BashRocks, BashForce, BashPoise * shove, true);
                            TakeImpact(-bashWay * back, met, true, BashRocks, BashForce, 0f, false);
                            if (seen != null)
                            {
                                seen.Handed = handed;
                                seen.HandedBack = back;
                                seen.Worked = worked;
                                seen.WorkedBy = Vector3.Dot(Flat(Loco.Velocity), way) - Mathf.Max(0f, ledger.Through - ledger.Drive) / Mathf.Max(1f, Mass);
                                seen.Drive = ledger.Drive;
                                seen.WorkedOn = ledger.Taker;
                                seen.Ground = ledger.Ground;
                            }
                        }

                        if (held)
                        {
                            shove = Mathf.Min(shove, 0.8f * BashStumblesFrom * Mathf.Sqrt(enemy.Size));
                        }

                        string did = worked ? enemy.Bashed(way, shove, this, met, bashWay * ledger.Through) : enemy.Bashed(out_, shove, this);
                        if (BashIsImpact && BashStops && did != "down" && !(worked && StumbleSteps.On))
                        {
                            // (Both stand still a moment as it lands. Not when it knocks him over: a body held still as it
                            // is thrown is not thrown; headless it went 0.8 m where it should go 2.5.)
                            float stop = Mathf.Clamp(BashStopBase + BashStopPerSpeed * arrived, CombatFeel.LeastHitStop, CombatFeel.MostHitStop);
                            ImpactStop = Mathf.Max(ImpactStop, stop);
                            enemy.ImpactStop = Mathf.Max(enemy.ImpactStop, stop);
                        }

                        BashesStumbled += did == "stumbled" ? 1 : 0;
                        BashesFelled += did == "down" ? 1 : 0;
                        if (worked && countered)
                        {
                            // (A counter held if it kept him off the ground at once: what his stumble makes of it comes after.)
                            held = did != "down";
                        }

                        if (countered)
                        {
                            enemy.CountersHeld += held ? 1 : 0;
                            BashesStopped += held ? 1 : 0;
                            enemy.LastBash = enemy.LastBash.Replace(": under way", held ? ": it held: the boards met and he kept his feet" : ": it did not hold");
                            HardBashCountered?.Invoke(enemy, this, held);
                            if (Diagnostics.PlaytestTrace.Enabled)
                            {
                                Diagnostics.PlaytestTrace.Note(enemy.FightUnit, "bash", held ? "his counter-bash held: the hard bash did not knock him over" : "his counter-bash did not hold");
                            }
                        }
                        LastBash = LastBash.Replace(": under way", string.Format(System.Globalization.CultureInfo.InvariantCulture, "; the board met him {0:0.00} s after it began, closing at {1:0.0} m/s: he was {2} at {3:0.0} m/s", bashFor, closing, did == "down" ? "knocked down, thrown back" : did == "stumbled" ? "thrown back stumbling" : "shoved back", shove));
                        if (held)
                        {
                            LastBash += " (met by his counter-bash: stopped)";
                        }

                        if (worked)
                        {
                            // (What the ledger leaves the basher: his body's speed changed by as much as his board's was.
                            // A leap's own travel ends where the boards met; he goes on, stops or is thrown back as it
                            // came out, and his legs decide what comes of that as they do for the other.)
                            float mine = Vector3.Dot(Flat(Loco.Velocity), way);
                            Loco.StopShotTravel();
                            // (Equal and opposite: what the other was handed is taken from the whole of him.)
                            // (Less what his own legs put in through the ground as he pushed on through.)
                            float lost = Mathf.Max(0f, ledger.Through - ledger.Drive) / Mathf.Max(1f, Mass);
                            string me = ThrowBody(-way * lost, met, -bashWay * ledger.Through, string.Format(System.Globalization.CultureInfo.InvariantCulture, "his own {0} bash meeting {1} (his board at {2:0.0} m/s, his body at {3:0.0}, the other at {4:0.0})", bashKind.Name, countered ? "a counter" : onShield ? "a shield" : "a body", fastest, body, hisSpeed));
                            LastBash += string.Format(System.Globalization.CultureInfo.InvariantCulture, " (worked out: {0:0} N·s handed over; he leaves at {1:0.0} m/s, the other at {2:0.0}; the other's feet passed {3:0} N·s to the ground; he himself was {4})", ledger.Through, mine - lost, ledger.Taker, ledger.Ground, me == "stumbled" ? "thrown off balance" : "checked");
                            if (countered)
                            {
                                guardOpenUntil = Mathf.Max(guardOpenUntil, clock + 0.3f);
                            }

                            if (BashHoldsGround && did != "down")
                            {
                                // (He does not walk on into a man still on his feet: his own legs take what is left of his going forward.)
                                Vector3 going = Stagger.Active ? Stagger.Vel : Flat(Loco.Velocity);
                                float forward = Vector3.Dot(going, way);
                                if (forward > 0f)
                                {
                                    if (Stagger.Active)
                                    {
                                        Stagger.Add(-way * forward, 0f);
                                        staggerGoing = Stagger.Vel;
                                    }

                                    Loco.Velocity -= way * forward;
                                }
                            }
                        }
                        else if (held)
                        {
                            // (Stopped by the other's board: he is thrown back off it a step himself, his own guard open
                            // a moment, and the two are apart again.)
                            float mine = Mathf.Max(0f, Vector3.Dot(Flat(Loco.Velocity), out_));
                            Loco.StopShotTravel();
                            Loco.Push(-out_ * (mine + CounterRebound * Mathf.Sqrt(Size)));
                            guardOpenUntil = Mathf.Max(guardOpenUntil, clock + 0.3f);
                        }
                        else if (!bashKind.GoesThrough)
                        {
                            // (He comes down where he met him, not on through him. A bash that goes through carries on into
                            // the space the enemy is thrown out of.)
                            Loco.StopShotTravel();
                        }
                    }
                }
            }

            if (bashFor >= bashEnds)
            {
                if (!bashStruck)
                {
                    LastBash = LastBash.Replace(": under way", bashAt != null ? ": it met nothing (he was out of its reach)" : ": at nobody");
                }
                else if (countering)
                {
                    LastBash = LastBash.Replace(": under way", ": the hard bash never reached him");
                }

                bashFor = -1f;
                bashAt = null;
            }
        }

        /// <summary>
        /// A shield's bash met him, shoving him back at this speed (m/s): a step back, his guard open a moment; from
        /// <see cref="BashStumblesFrom"/> thrown back stumbling, his guard open longer and a blow of his own dropped;
        /// from <see cref="BashFellsFrom"/> knocked down. What it did: "shoved", "stumbled", "down".
        /// </summary>
        /// <param name="point">Where it met him, and the impulse it handed him there (N·s): given, <paramref name="speed"/> is the ledger's and what comes of it is his legs' to decide; not given, it is by the two speeds above.</param>
        /// <summary>His bash has landed and its board is on its way back: the rest of it is dropped (for a pounce on the opening it made). Nothing if it has not landed or its drive is not over.</summary>
        public void CutBashShort()
        {
            if (Bashing && bashStruck && !countering && bashFor >= bashDriveAt + bashDriveTime + 0.5f * BashHeld && bashFor < bashEnds)
            {
                bashEnds = bashFor;
            }
        }

        public string Bashed(Vector3 way, float speed, ClipFighter by, Vector3 point = default, Vector3 impulse = default)
        {
            if (Fallen)
            {
                return "down";
            }

            BashedTimes++;
            float size = Mathf.Sqrt(Size);
            string did = speed >= BashFellsFrom * size ? "down" : speed >= BashStumblesFrom * size ? "stumbled" : "shoved";
            if (impulse.sqrMagnitude > 0f && StumbleSteps.On)
            {
                // (Off balance or not, on his feet or not: his legs decide, on real stumble steps.)
                did = ThrowBody(way * speed, point, impulse, "a shield's bash");
            }
            else if (did == "down")
            {
                KnockOver(way, Mathf.Min(speed, 3f), $"a shield's bash, thrown back at {speed:0.0} m/s", speed * BashThrows);
            }
            else if (speed > 0.05f)
            {
                Loco.Push(way * speed, did == "stumbled");
            }

            guardOpenUntil = Mathf.Max(guardOpenUntil, clock + (did == "shoved" ? 0.3f : 0.8f));
            if (Diagnostics.PlaytestTrace.Enabled)
            {
                Diagnostics.PlaytestTrace.Note(FightUnit, "bash", $"bashed by {(by != null ? by.FightUnit.name : "a shield")}: {(did == "down" ? "knocked down" : did == "stumbled" ? "thrown back stumbling" : "shoved back")} at {speed:0.0} m/s");
            }

            return did;
        }

        /// <summary>How many times a shield's bash has met him.</summary>
        public int BashedTimes;

        private void RaiseShield(float dt, Vector3 striker)
        {
            StepBash(dt);
            StepKick(dt);
            bool dodgesFake = FlinchAtFake();
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
                        && toContact <= Mathf.Min(SeesBefore(1f - lack), CombatFeel.ShieldMovesWithin) && Blocking && !GuardOpen
                        // (Uncovering, he has taken his shield away himself: it does not go to meet a blow until it is back.)
                        && !IsUncovering
                        // (In a fake of his own he is committed to it: his shield does not start to meet a blow until it is over.)
                        && !(PostureCommitsShield && Posturing)
                        // (A fake he dodges: his shield stays where it is.)
                        && !dodgesFake;
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
            float wantIntercept = !GuardOpen && !IsUncovering && !(PostureCommitsShield && Posturing) && (sees || (backIn > 0f && Blocking && interceptWeight > 0f)) ? 1f : 0f;
            // (A poor shield lets its guard fall back once the blow it met has passed, as a combo's next comes on unseen: a
            // good one keeps it where it was until it sees where the next is going.)
            if (Blow.HasValue && !sees && seenSerial != Blow.Value.Serial && blowLack > 0.5f)
            {
                wantIntercept = 0f;
            }
            interceptWeight = Mathf.MoveTowards(interceptWeight, wantIntercept, dt / BlockRaise);
            // (The cover: not while he has taken the shield away himself, or it is knocked open, or it has no board.)
            float wantCover = ShieldCover && !IsUncovering && !GuardOpen && !ShieldLeaves && ShieldColliders.Count > 0 ? (Striking && !Posturing ? CoverInBlow : CoverHold) : 0f;
            // (A bash is the cover's own board driven out: the arm is all the cover's for it, at once.)
            wantCover = Bashing ? 1f : wantCover;
            coverWeight = Mathf.MoveTowards(coverWeight, wantCover, dt / (Bashing ? 0.06f : CoverEase));
            float weight = Mathf.Max(blockWeight, interceptWeight, coverWeight);
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
            bool covers = interceptWeight <= 0f && coverWeight > 0f;
            bool fresh = interceptWeight > 0f && seen && PlanMeet(Blow.Value, out freshAt, out freshNormal, out freshMotion, out freshWay, out meetIn, out inside);
            bool havePlan = interceptWeight > 0f && Blow.HasValue && planSerial == Blow.Value.Serial;
            bool holdLast = interceptWeight > 0f && !seen && planSerial >= 0 && easedSerial == planSerial;
            if (covers)
            {
                // (No blow to meet: the board before his chest, looking at the enemy. Its place is from the chest, so it
                // rises, sways and steps with the body; the hand behind it by the board's own place in the fist.)
                var rootTurn = Quaternion.Euler(0f, Loco.Facing, 0f);
                face = rootTurn * CoverFace.normalized;
                Vector3 boardAt = chest + rootTurn * (CoverAt * Size);
                if (Bashing)
                {
                    // (The bash: the board brought before his middle and driven straight ahead into the enemy, looking at
                    // him, a little up: not across like a cut.)
                    float outNow = BashOutNow, square = Mathf.Clamp01(Mathf.Abs(outNow) * 4f);
                    // (His shield shoulder goes with it: drawn back with the board, then thrown in behind it, his chest
                    // turned about the upright and his head left looking where it did. An arm alone reaches a hand's
                    // breadth past its cover; the shoulder behind it is what carries the board into the enemy.)
                    float thrown = bashShoulder * (outNow >= 0f ? outNow : 2f * outNow);
                    var chestPart = Anim.Part(PartId.Chest);
                    var headPart = Anim.Part(PartId.Head);
                    var headWas = headPart.rotation;
                    chestPart.rotation = Quaternion.AngleAxis(thrown, Vector3.up) * chestPart.rotation;
                    headPart.rotation = headWas;
                    Vector3 before = new Vector3(Mathf.Lerp(CoverAt.x, 0f, square), CoverAt.y, CoverAt.z);
                    boardAt = chest + rootTurn * ((before + new Vector3(0f, BashUp * Mathf.Max(0f, outNow), outNow >= 0f ? bashOutFar * outNow : bashDrawIn * 4f * outNow)) * Size);
                    face = Vector3.Slerp(face, (bashWay + 0.05f * Vector3.up).normalized, square);
                }
                Vector3 boardFromHand = -face * (0.08f * Size);
                var posedSlot = Anim.SlotWeaponL;
                if (CombatFeel.ShieldFixed && shieldBoardKnown)
                {
                    boardFromHand = rootTurn * shieldBoardFrom;
                }
                else if (posedSlot != null && BoardOnSlot(out var boardLocal))
                {
                    boardFromHand = (posedSlot.position - hand.position) + posedSlot.rotation * boardLocal;
                }

                want = boardAt - boardFromHand;
            }
            else if (fresh || havePlan || holdLast)
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

            // (In a bash the arm goes to its whole length.)
            want = upper.position + Vector3.ClampMagnitude(want - upper.position, Bashing ? reach * 0.98f / 0.9f : reach);
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
            if (Bashing)
            {
                // (A bash is driven, not eased.)
                most = 2f * ShieldSpeed * Size;
                settle = Mathf.Min(settle, 0.07f);
            }
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
                HitForceGiven += Upper.Strike(shield ? PartId.ForearmL : parried ? PartId.HandR : part, impulse * (CombatFeel.HitForce * (shield || parried ? HeldJolt : 1f)), point);
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
                    Built?.CutShort(StrikeHits.RewindMost, StrikeHits.ReboundSpeed);
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
                if (StumbleSteps.On && Loco.Guarding && !Anchored)
                {
                    // (Off balance: his legs decide, on real stumble steps. No speed is written in for what they can catch.)
                    pendingKnock = thrownBy;
                    pendingPoint = point;
                    pendingImpulse = impulse;
                    pushAt = clock;
                    LastReaction = $"{(swept.Length > 0 ? swept + "; " : "")}thrown off balance at {thrownAtSpeed:0.0} m/s";
                    return;
                }

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

        /// <summary>For the reports: knocks that were still waiting as he fell, and went with the fall (all fighters).</summary>
        public static int StaleKnocks;
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

        // (The most the trunk may lean now: a blade's, until a body's impact asks for more, and again once it has settled.)
        private float leanMost = MostLean;

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

        /// <summary>
        /// Impacts are absorbed (18 November, a first trial; Combat Feel "Impacts Are Absorbed" and "Absorbed How
        /// Much"). What made a hit look like toys meeting was in how the reactions come back, not in how far they go:
        /// a struck trunk and head came back on a spring that swings past where it set out from and back again (its
        /// damping 0.55 of what stops a spring dead); a knocked arm or leg came back as quickly as it went; and the
        /// striker's hand was thrown back off a shield or a parry "so even a light blow visibly bounces". Absorbed: the
        /// trunk and the head go as far as before and settle without swinging past (the kick made up for the damping,
        /// so the size of a reaction is unchanged); a rocked trunk, a snapped head and a knocked leg come back slower
        /// than they went (a knocked arm as quickly as before: see <see cref="React"/>); a blade stopped by a shield or
        /// a parry stays on it instead of springing off. <see cref="Absorbed"/> is how much:
        /// 0 as before, 1 the most. Off: as before.
        /// </summary>
        public static bool ImpactsAbsorbed = true;
        public static float Absorbed = 0.6f;

        private static float Absorb => ImpactsAbsorbed ? Mathf.Clamp01(Absorbed) : 0f;

        /// <summary>The damping the trunk's and the head's springs settle with (1 stops a spring dead, without swinging past).</summary>
        public static float ReactSettles => Mathf.Lerp(ReactDamping, 1f, Absorb);

        /// <summary>How quickly what a blow knocked comes back, as a share of how quickly it went.</summary>
        public static float ComesBack => 1f - 0.5f * Absorb;

        /// <summary>
        /// How much of a blow's force is put on the physical shield arm, or on a parrying sword's hand, at once
        /// (absorbed: less). That force threw the seen hand some 11 cm off its pose within three steps of a blade
        /// meeting the board and let it come back as quickly: a jolt out and back on the very thing being watched. What
        /// the shield gives to a blow is shown by its arm's own give, which goes out and settles.
        /// </summary>
        public static float HeldJolt => 1f - 0.7f * Absorb;

        /// <summary>How much of its spring back off a shield or a parry a stopped blade keeps.</summary>
        public static float ReboundKept => 1f - 0.75f * Absorb;

        /// <summary>
        /// How far the trunk's spring goes at its furthest when kicked at speed 1, with this damping: stepped as
        /// <see cref="Spring"/> steps it, 60 times a second (worked out from the spring's own formula it came out a
        /// tenth too far for the heavier damping, which the steps damp more than the formula does).
        /// </summary>
        private static float PeakOf(float damping)
        {
            if (Mathf.Abs(damping - peakFor) < 1e-4f)
            {
                return peakIs;
            }

            Vector3 x = Vector3.zero, v = Vector3.right;
            float furthest = 0f;
            for (int i = 0; i < 90; i++)
            {
                Spring(ref x, ref v, ReactFrequency, damping, 1f / 60f);
                furthest = Mathf.Max(furthest, x.x);
            }

            peakFor = damping;
            peakIs = furthest;
            return furthest;
        }

        private static float peakFor = -1f, peakIs;
        private static readonly float PeakBefore = PeakOf(ReactDamping);

        /// <summary>What a kick is made larger by, so a reaction goes as far with the new damping as it did with the old.</summary>
        private static float ReactGain => Absorb <= 0f ? 1f : PeakBefore / Mathf.Max(1e-4f, PeakOf(ReactSettles));

        /// <summary>Is a spring on its way back (toward its rest)?</summary>
        private static bool Returning(Vector3 x, Vector3 v) => Vector3.Dot(x, v) < 0f;

        /// <summary>A struck arm's knock aside: its return (Hz, critically damped), the most it moves the hand (m at a Medium's size) and turns it (radians).</summary>
        public const float ArmKnockFrequency = 2.5f, MostArmKnock = 0.12f, MostArmTurn = 0.44f;

        private static float Tanh(float x)
        {
            float e = Mathf.Exp(-2f * Mathf.Abs(x));
            return Mathf.Sign(x) * (1f - e) / (1f + e);
        }

        /// <summary>The upper body turned about the hips by the blow's lever; the head snapped by one to it.</summary>
        /// <param name="most">The most it may rock the trunk (radians): a blade's blow, <see cref="MostLean"/>; a body's impact, further.</param>
        private void Recoil(Vector3 impulse, Vector3 point, PartId part, float scale, float most = MostLean)
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
                // (A body's impact may rock it further than a blade's: by as much more, it takes as much more to get there.)
                float amount = most * Tanh(1.5f * j * leverage / (Mass * leg) * (MostLean / Mathf.Max(MostLean, most))) * CombatFeel.LeanScale;
                leanMost = Mathf.Max(leanMost, most);
                leanV += axis.normalized * (amount * Kick * w * ReactGain);
            }

            if (part == PartId.Head)
            {
                Vector3 headLever = point - Anim.Part(PartId.Head).position + Vector3.up * (0.1f * leg);
                Vector3 headAxis = Vector3.Cross(headLever, dir);
                if (headAxis.sqrMagnitude > 1e-8f)
                {
                    float snap = MostHeadSnap * Tanh(6f * j / (Mass * leg)) * CombatFeel.HeadSnapScale;
                    headV += headAxis.normalized * (snap * Kick * 2f * Mathf.PI * HeadFrequency * ReactGain);
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
        /// <param name="whole">All of it, as the one who blocked it is rocked (<see cref="BlockRocks"/>); off: by the scene's "Jar Scale".</param>
        public void Jar(Vector3 impulse, Vector3 point, bool whole = false)
        {
            if (Fallen || (!whole && CombatFeel.JarScale <= 0f))
            {
                return;
            }

            Recoil(impulse, point, PartId.HandR, (whole ? BlockRocks : CombatFeel.JarScale) * ReactScale);
        }

        /// <summary>How much of a blow on a shield rocks its bearer's trunk, against a blow on his body.</summary>
        public const float BlockRocks = 0.6f;

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
            // (Absorbed: no swinging past, and back slower than out.)
            float back = ComesBack;
            Spring(ref lean, ref leanV, ReactFrequency * (Returning(lean, leanV) ? back : 1f), ReactSettles, dt);
            Spring(ref headSnap, ref headV, HeadFrequency * (Returning(headSnap, headV) ? back : 1f), ReactSettles, dt);
            lean = Vector3.ClampMagnitude(lean, leanMost);
            if (lean.sqrMagnitude < 0.0025f && leanV.sqrMagnitude < 0.01f)
            {
                leanMost = MostLean;
            }

            headSnap = Vector3.ClampMagnitude(headSnap, MostHeadSnap);
            for (int a = 0; a < 2; a++)
            {
                float wk = 2f * Mathf.PI * armKnockHz[a];
                // (A knocked arm comes back as quickly as before, absorbed or not. Coming back slower, a shield knocked
                // back onto his own sword stayed there: headless, in a fight of six, his own blade was through his own
                // board 4.2 s of 80, against 1.5 s. It waits for the pass that keeps what he holds clear of himself.)
                CriticalSpring(ref knock[a], ref knockV[a], wk, dt);
                CriticalSpring(ref knockTurn[a], ref knockTurnV[a], wk, dt);
            }

            float wl = 2f * Mathf.PI * LegKnockFrequency;
            for (int l = 0; l < 2; l++)
            {
                CriticalSpring(ref legTurn[l], ref legTurnV[l], wl * (Returning(legTurn[l], legTurnV[l]) ? back : 1f), dt);
                legTurn[l] = Vector3.ClampMagnitude(legTurn[l], MostLegTurn);
                if (legTurn[l].sqrMagnitude > 1e-8f)
                {
                    var thigh = Anim.Part(l == 0 ? PartId.ThighL : PartId.ThighR);
                    thigh.rotation = Quaternion.AngleAxis(legTurn[l].magnitude * Mathf.Rad2Deg, legTurn[l].normalized) * thigh.rotation;
                }
            }

            StepPress(dt);
            // (A stumble's own tipping, eased in and out.)
            if (!Stagger.Active)
            {
                tipWanted = Vector3.zero;
            }

            tip = Vector3.Lerp(tip, tipWanted, 1f - Mathf.Exp(-TipFollows * dt));
            if (tip.sqrMagnitude > 1e-6f)
            {
                float tipped = tip.magnitude * Mathf.Rad2Deg;
                var spine = Anim.Part(PartId.Spine);
                var chest = Anim.Part(PartId.Chest);
                spine.rotation = Quaternion.AngleAxis(0.45f * tipped, tip.normalized) * spine.rotation;
                chest.rotation = Quaternion.AngleAxis(0.55f * tipped, tip.normalized) * chest.rotation;
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
