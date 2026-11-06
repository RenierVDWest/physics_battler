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

        /// <summary>Its sword parry (with <see cref="Stances.Sword"/>; made by whoever gives it its sword).</summary>
        public SwordParry Parry;

        /// <summary>The blow coming, as it reads it (null: none).</summary>
        public BlowRead? Blow;

        /// <summary>The parries it has made.</summary>
        public int Parries;

        /// <summary>The blow coming, read from the striker's hits (or none).</summary>
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
        public static void KeepApart(ClipFighter a, ClipFighter b)
        {
            if (a == null || b == null || a.Fallen || b.Fallen)
            {
                return;
            }

            Vector3 gap = b.Loco.Position - a.Loco.Position;
            gap.y = 0f;
            float apart = gap.magnitude, least = LeastGap * FightingDistance(a.Size, b.Size);
            if (apart >= least)
            {
                return;
            }

            Vector3 way = apart > 1e-4f ? gap / apart : Quaternion.Euler(0f, a.Loco.Facing, 0f) * Vector3.forward;
            float into = least - apart, shareA = b.Mass / Mathf.Max(1f, a.Mass + b.Mass);
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

        /// <summary>The shield's speed builds up over this long (seconds): it accelerates, it doesn't jump.</summary>
        public const float ShieldSpeedUp = 0.08f;

        /// <summary>How far from an expert, 0 to 1 (on the curve the parry uses: skill 0.5 midway).</summary>
        private float ShieldLack => 1f - Mathf.Clamp01(ShieldSkill) * Mathf.Clamp01(ShieldSkill);

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
            float skill = Mathf.Clamp01(ShieldSkill);
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
                    : p.id == PartId.ThighR || p.id == PartId.ShinR || p.id == PartId.FootR;
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
            if (Authority == null)
            {
                // (Headless, no body to fall: down a moment, then standing.)
                if (footingTime > LeastDown + 0.5f)
                {
                    Footing = Footings.Standing;
                }

                return;
            }

            var pelvis = Rig.Part(PartId.Pelvis).body;
            if (Footing == Footings.Down)
            {
                stillFor = pelvis.linearVelocity.magnitude < 0.25f * Mathf.Sqrt(Size) ? stillFor + dt : 0f;
                Authority.Apply(dt);
                // (Its place goes with the body on the ground: what keeps its distance from it, or aims at it, follows.)
                Loco.Position = new Vector3(pelvis.position.x, groundLevel, pelvis.position.z);
                if ((footingTime >= LeastDown && stillFor >= StillFor) || footingTime >= MostDown)
                {
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

                LastReaction = "got up";
            }
        }

        private void BeginGetUp()
        {
            var pelvis = Rig.Part(PartId.Pelvis).body;
            Vector3 front = pelvis.rotation * Vector3.forward;
            var kind = front.y > 0f ? GetUp.Kind.FaceUp : GetUp.Kind.FaceDown;
            Vector3 headward = Rig.Part(PartId.Head).body.position - pelvis.position;
            headward.y = 0f;
            if (headward.sqrMagnitude < 0.01f * Size * Size)
            {
                headward = pelvis.rotation * Vector3.up;
                headward.y = 0f;
            }

            GetUp.Place(kind, pelvis.position, headward, body.SizeFactor, out var position, out float yaw);
            position.y = groundLevel;
            getUpRoot.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
            getUpStance.Plant(position, yaw);
            // (Where it will stand: its walk placed there, the feet let go to land on their spots.)
            Loco.Place(position, yaw);
            Poser.ReleaseFeet();
            lying.Capture(Rig);
            getUp.HoldsMain = stance == Stances.Sword;
            getUp.HoldsOff = stance == Stances.Shield;
            getUp.Begin(kind, lying);
            Footing = Footings.GettingUp;
            footingTime = 0f;
            LastReaction = $"getting up {(kind == GetUp.Kind.FaceDown ? "face down" : "face up")}";
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

        private void MoveOn(float dt, Vector3 want, float yaw, Vector3 opponent, bool frozen)
        {
            if (frozen)
            {
                return;
            }

            GiveGround(dt);

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

            if (pushAt >= 0f && clock >= pushAt)
            {
                Loco.Push(pendingKnock);
                pushAt = -1f;
                pendingKnock = Vector3.zero;
            }

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
            HandOff(dt);
            if (!Defends)
            {
                return;
            }

            if (stance == Stances.Sword && Parry != null)
            {
                // (The body's sway first, then the parry solved on it: swayed after, the sword went off its line.)
                React(dt);
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

                    Parry.Skill = ParrySkill;
                    Parry.Step(dt, Blow, opponent);
                    MaybeRetire(opponent);
                }
            }
            else
            {
                RaiseShield(dt, opponent);
                CountArmCut();
                React(dt);
            }

            Recover(dt);
            Poise = Mathf.Min(MaxPoise, Poise + PoiseBack * MaxPoise * dt);
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
                if (striking || (Defends && stance == Stances.Sword && Parry != null))
                {
                    Authority.Set(BodyRegion.ArmR, AuthorityLevel.Kinematic, 1f);
                }
                else if (pinLeft[(int)BodyRegion.ArmR] <= 0f)
                {
                    Hold(BodyRegion.ArmR, 1f);
                }
            }

            // (The shield as turned in the hand: the shield that is seen, and blocks, is on the physical hand's slot.)
            if (Rig.SlotWeaponL != null && Anim.SlotWeaponL != null)
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

                Authority.Apply(dt);
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
                    combo.Carried = CarryBy;
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

        /// <summary>A strike alone follows its target, as far as the arm's pull goes: where its target is now (null: none to follow).</summary>
        public System.Func<LocoClip, Vector3?> Follow;

        /// <summary>The weapon arm is held to its pose this long after a strike (seconds).</summary>
        public const float WeaponArmHeldAfter = 0.4f;

        private float weaponArmHeldUntil = -1f;

        /// <summary>It strikes with <paramref name="weapon"/> (null: a plain sword of its poser's weapon length): its blade's hits, swept from the physical hand's slot when it is physical.</summary>
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
        public bool ComboAt(ClipFighter target, LocoClip first, int variant, out LocoClip opener, out List<StrikeLink> chain)
        {
            opener = first;
            chain = first != null && first.ContactFrame >= 0 ? StrikeCombos.CoreChain(first, StrikeClips, 3, variant) : new List<StrikeLink>();
            if (target == null || chain.Count == 0)
            {
                return false;
            }

            if (Combo == null)
            {
                Combo = new ComboPlayer(Loco, Poser) { Strikes = StrikeClips };
            }

            Combo.TargetFor = clip => AimAt(target, clip);
            Combo.TargetVelocity = () => target.Loco.Velocity;
            KeepOff(target);
            Combo.Keep = first;
            opener = chain[0].From;
            Combo.Start(opener, chain);
            return true;
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
            if (!GivesGround || !Blow.HasValue || Parry == null || Anchored || retiredFor == Blow.Value.Serial || Parry.Late <= RetireLate)
            {
                return;
            }

            // (An expert always gives the ground it needs; a novice only now and then, and less of it.)
            retiredFor = Blow.Value.Serial;
            float lack = 1f - Mathf.Clamp01(ParrySkill) * Mathf.Clamp01(ParrySkill);
            if (strikeLuck.NextDouble() < lack)
            {
                return;
            }

            Vector3 away = Loco.Position - opponent;
            away.y = 0f;
            away = away.sqrMagnitude > 1e-4f ? away.normalized : -(Quaternion.Euler(0f, Loco.Facing, 0f) * Vector3.forward);
            retireLeft = away * (Mathf.Lerp(ExpertRetire, NoviceRetire, lack) * Size);
            voidedFor = Blow.Value.Serial;
            Retires++;
            // (The walk steps after it, as a body driven back does.)
            if (Loco.Current != Locomotor.Mode.OneShot)
            {
                Loco.Push(away * (0.6f * Mathf.Sqrt(Size)));
            }
        }

        private void GiveGround(float dt)
        {
            if (retireLeft.sqrMagnitude > 1e-8f)
            {
                Vector3 move = Vector3.ClampMagnitude(retireLeft, RetireSpeed * Mathf.Sqrt(Size) * dt);
                Loco.Position += move;
                CarryBy(move);
                retireLeft -= move;
                Voided += move;
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
                if (posedSlot != null && BoardOnSlot(out var boardLocal))
                {
                    var slotThen = Quaternion.FromToRotation(posedSlot.forward, face) * posedSlot.rotation;
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
                    ShieldTrace(string.Format(System.Globalization.CultureInfo.InvariantCulture, "blow {0}: {1:0.00} s to contact, its path comes no nearer the body than {2:0.00} m (the guard {3:0.00}): held across its line",
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
            ShieldTrace(string.Format(System.Globalization.CultureInfo.InvariantCulture, "blow {0}{1}: {2:0.00} s to contact, meets the blade in {3:0.00} s at {4}, the face {5}{6}",
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

        public void TakeHit(PartId part, bool shield, Vector3 impulse, float partShare, float poiseDamage, Combat.Reaction reaction, Vector3 point, bool parried = false, bool bound = false)
        {
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
            float react = (reaction == Combat.Reaction.Stagger ? 1.6f : reaction == Combat.Reaction.Flinch ? 1.25f : 1f) * ReactScale;
            Recoil(impulse, point, shield ? PartId.HandL : part, shield ? 0.6f * react : react);
            if (!shield)
            {
                AddImpact(point, impulse);
            }

            var armRegion = shield ? BodyRegion.ArmL : AuthorityController.RegionOf(part);
            bool knockedOpen = false;
            if (shield)
            {
                // (The plan "Weight and momentum", step 4: the shield arm gives by the blow's weight; heavy enough, the
                // guard is knocked open, the arm thrown wide and slow to come back, the shield meeting nothing till then.)
                float weight = impulse.magnitude / Mathf.Max(1f, Mass);
                knockedOpen = weight >= CombatFeel.GuardOpenFrom;
                float give = Mathf.Min(CombatFeel.ShieldGivePerWeight * weight, CombatFeel.MostShieldGive) * Size;
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

            // Driven back: what the struck part doesn't take moves the whole body (the fight's rule), with steps
            // when it is hard enough. The body first takes the blow where it stands (its reaction clip), then
            // the steps carry it back (a push at once would cut the reaction off before it showed).
            Vector3 knock = impulse * (1f - partShare) * KnockScale / Mass;
            knock.y = 0f;
            bool driven = !Anchored && knock.magnitude > DrivenFrom;
            string clipName = shield ? null : ReactionClip(part, impulse.magnitude / Mass, reaction);
            var clip = clipName != null ? LocoLibrary.Get(clipName) : null;
            string reacted = null;
            // (Heavy enough (the plan "Weight and momentum", step 3: the Large's club on a Medium), it is thrown back
            // stumbling at once, its walk taking the catch steps, no reaction clip to wait out first.)
            bool stumbles = driven && !shield && !Striking && impulse.magnitude / Mathf.Max(1f, Mass) >= CombatFeel.StumbleFrom;
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
                if (reaction == Combat.Reaction.Stagger || reaction == Combat.Reaction.Flinch)
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

            if (clip != null && Loco.Current == Locomotor.Mode.OneShot && Loco.Shot != null && Loco.Shot.Name.StartsWith("sns_impact_"))
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
                pendingKnock = Vector3.ClampMagnitude(knock * CombatFeel.StumbleBoost + (pushAt > clock ? pendingKnock : Vector3.zero), CombatFeel.MostStumble);
                pushAt = clock;
                LastReaction = $"{(swept.Length > 0 ? swept + "; " : "")}thrown back stumbling at {pendingKnock.magnitude:0.0} m/s";
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
        private string ReactionClip(PartId part, float strength, Combat.Reaction reaction)
        {
            bool high = part == PartId.Head || PartAt(part).y > PartAt(PartId.Chest).y - 0.05f * Size;
            if (reaction == Combat.Reaction.Stagger || reaction == Combat.Reaction.Killed || strength >= HeavyFrom)
            {
                return "sns_impact_heavy";
            }

            // (An arm struck jerks the body less than a blow to it.)
            var region = AuthorityController.RegionOf(part);
            if ((strength < LightBelow && reaction == Combat.Reaction.Recoil) || region == BodyRegion.ArmL || region == BodyRegion.ArmR)
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
        public float ParryHolds => Mathf.Lerp(CombatFeel.ParryHolds, CombatFeel.ExpertParryHolds, Mathf.Clamp01(ParrySkill));

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

        private void AddImpact(Vector3 point, Vector3 impulse)
        {
            float weight = impulse.magnitude / Mathf.Max(1f, Mass);
            float peak = Mathf.Min(BendPerWeight * weight, MostBend) * Size;
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

        public void Destroy()
        {
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
