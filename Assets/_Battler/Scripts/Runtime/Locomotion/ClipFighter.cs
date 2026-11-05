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

        /// <summary>Blows its shield has read (for the logs).</summary>
        public int ShieldBlowsRead;

        private void NewShieldBlow()
        {
            ShieldBlowsRead++;
            float lack = ShieldLack;
            Vector3 r = new Vector3((float)shieldLuck.NextDouble() * 2f - 1f, (float)shieldLuck.NextDouble() * 2f - 1f, (float)shieldLuck.NextDouble() * 2f - 1f);
            shieldMisread = (r.sqrMagnitude > 1e-4f ? r.normalized : Vector3.up) * (NoviceMisread * lack * (0.4f + 0.6f * (float)shieldLuck.NextDouble()) * Size);
            Vector3 axis = new Vector3((float)shieldLuck.NextDouble() * 2f - 1f, (float)shieldLuck.NextDouble() * 2f - 1f, (float)shieldLuck.NextDouble() * 2f - 1f);
            shieldMisline = Quaternion.AngleAxis(NoviceMisline * lack * (0.4f + 0.6f * (float)shieldLuck.NextDouble()), axis.sqrMagnitude > 1e-4f ? axis.normalized : Vector3.up);
            coversLow = shieldLuck.NextDouble() >= lack;
            shieldReadHeld = false;
            shieldReread = 0f;
            shieldSpeedNow = 0f;
        }

        private Vector3 shieldAt;
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
            height < 0 ? 0.5f * (PartAt(PartId.ShinL) + PartAt(PartId.ShinR))
            : height == 0 ? PartAt(PartId.Pelvis)
            : height == 1 ? 0.5f * (PartAt(PartId.Spine) + PartAt(PartId.Chest))
            : PartAt(PartId.Head) + Vector3.up * (0.08f * Size);

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
                }
            }
            else
            {
                RaiseShield(dt, opponent);
            }

            React(dt);
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
                Authority.Apply(dt);
            }
            else
            {
                Carry();
                snapped = true;
            }
        }

        /// <summary>The physical body has been put on its pose (the first <see cref="Apply"/> carries it there; after that it is driven).</summary>
        public bool Snapped => snapped;

        // ---- Attacking -------------------------------------------------------------------------------------------

        /// <summary>Its strikes' hits (null until <see cref="EnableStrikes"/>): their blades swept against the bodies about it.</summary>
        public StrikeHits Strikes { get; private set; }

        /// <summary>Its combos (set by whoever plays them); stepped with its walk.</summary>
        public ComboPlayer Combo;

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
        public bool Busy => Fallen || Striking || Loco.Current == Locomotor.Mode.OneShot;

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
            float lack = ShieldLack;
            float coming = Blow.HasValue ? Blow.Value.Coming : -1f;
            if (Blow.HasValue && (lastComing < 0f || coming < lastComing - 0.05f))
            {
                NewShieldBlow();
                incomingFor = 0f;
            }

            lastComing = coming;
            incomingFor = IncomingBlade.HasValue ? incomingFor + dt : 0f;
            // (A novice sees the blow late; once it has passed, the shield stays up a moment, longer for a novice.)
            // (An expert reads the wind-up; a novice moves only once the blade itself is coming, the last moment before contact.)
            float toContact = Blow.HasValue ? Blow.Value.ToContact : 0f;
            bool sees = IncomingBlade.HasValue && incomingFor > Mathf.Lerp(SeeBlowAfter, NoviceSeeAfter, lack)
                        && toContact <= SeesBefore(1f - lack) && Blocking;
            backIn = sees ? Mathf.Lerp(ExpertBackAfter, NoviceBackAfter, lack) : Mathf.Max(0f, backIn - dt);
            float wantIntercept = sees || (backIn > 0f && Blocking && interceptWeight > 0f) ? 1f : 0f;
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
            // (How well it reads the blow: a novice's shield goes where it guessed (off by its misread, and its line),
            // keeps to that first guess, and holds at the chest against a low cut.)
            if (IncomingBlade.HasValue && interceptWeight > 0f)
            {
                shieldReread -= dt;
                if (shieldReadHeld && shieldReread > 0f)
                {
                    aimed = shieldHeldAim;
                    toBlade = shieldHeldLine;
                }
                else
                {
                    float hips = Anim.Part(PartId.Pelvis).position.y;
                    if (!coversLow && aimed.y < hips)
                    {
                        aimed = new Vector3(aimed.x, chest.y, aimed.z);
                    }

                    // (Off to one side of the line, not along it, where a misread would make no difference.)
                    aimed += Vector3.ProjectOnPlane(shieldMisread, toBlade).normalized * shieldMisread.magnitude;
                    toBlade = (shieldMisline * toBlade).normalized;
                    shieldHeldAim = aimed;
                    shieldHeldLine = toBlade;
                    shieldReadHeld = true;
                    shieldReread = lack >= 0.999f ? float.MaxValue : 0.4f * lack;
                }
            }

            // (Not behind itself: the shield stays in front of the body.)
            Vector3 ahead = Quaternion.Euler(0f, Loco.Facing, 0f) * Vector3.forward;
            if (Vector3.Dot(toBlade, ahead) < 0.2f)
            {
                toBlade = (toBlade + ahead * (0.2f - Vector3.Dot(toBlade, ahead))).normalized;
            }

            Vector3 want = aimed + toBlade * (0.32f * Size);
            float reach = (Anim.RestLocalPosition(PartId.ForearmL).magnitude + Anim.RestLocalPosition(PartId.HandL).magnitude) * 0.9f;
            want = upper.position + Vector3.ClampMagnitude(want - upper.position, reach);
            if (!shieldAtKnown)
            {
                shieldAt = hand.position;
                shieldAtKnown = true;
            }

            // (Its speed builds up, a novice's to less.)
            float most = Mathf.Lerp(ShieldSpeed, NoviceShieldSpeed, lack) * Size;
            shieldSpeedNow = Mathf.MoveTowards(shieldSpeedNow, most, most / ShieldSpeedUp * dt);
            shieldAt = Vector3.MoveTowards(shieldAt, want, Mathf.Max(0.2f * most, shieldSpeedNow) * dt);
            Vector3 target = Vector3.Lerp(hand.position, shieldAt, weight);
            var turn = Quaternion.Euler(0f, Loco.Facing, 0f);
            TwoBoneIK.Solve(upper, fore, hand, target, turn * new Vector3(-1f, -0.6f, 0f));
            // (The wrist straight along the forearm, the shield turned in the fist to face the blow: an expert's slanted so a
            // cut glances off, a novice's square on.)
            Vector3 face = toBlade;
            float slant = ExpertSlant * (1f - lack);
            if (slant > 0.5f && IncomingFrom.HasValue)
            {
                Vector3 across = Vector3.Cross(IncomingFrom.Value, Vector3.up);
                if (across.sqrMagnitude > 1e-4f)
                {
                    face = Quaternion.AngleAxis(slant, across.normalized) * face;
                }
            }

            // (And turned no faster than the arm can turn it: a novice's shield can't whip round to catch a blade at the last
            // moment.)
            if (!shieldFaceKnown || shieldFace.sqrMagnitude < 1e-6f)
            {
                shieldFace = Quaternion.Euler(0f, Loco.Facing, 0f) * Vector3.forward;
                shieldFaceKnown = true;
            }

            shieldFace = Vector3.RotateTowards(shieldFace, face, Mathf.Lerp(ShieldTurnSpeed, NoviceShieldTurnSpeed, lack) * Mathf.Deg2Rad * dt, 0f);
            Poser.FaceShieldAt(shieldFace, Mathf.Max(weight, Poser.ClipArms * Poser.ShieldTurned));
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
        public const float MostKnock = 2.2f;

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
                KnockArm(0, impulse, point);
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

        public void TakeHit(PartId part, bool shield, Vector3 impulse, float partShare, float poiseDamage, Combat.Reaction reaction, Vector3 point, bool parried = false)
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
                KnockArm(0, impulse, point);
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
            var armRegion = shield ? BodyRegion.ArmL : AuthorityController.RegionOf(part);
            if (armRegion == BodyRegion.ArmL || armRegion == BodyRegion.ArmR)
            {
                KnockArm(armRegion == BodyRegion.ArmR ? 0 : 1, impulse * (shield ? 1f : partShare), point);
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

            reacted ??= clip != null ? clipName.Replace("sns_impact_", "") + " reaction" : shield ? "the shield knocked back" : "no reaction clip";
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
        public float ReactScale = 1.8f, KnockScale = 2.5f;

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
                float amount = MostLean * Tanh(1.5f * j * leverage / (Mass * leg));
                leanV += axis.normalized * (amount * Kick * w);
            }

            if (part == PartId.Head)
            {
                Vector3 headLever = point - Anim.Part(PartId.Head).position + Vector3.up * (0.1f * leg);
                Vector3 headAxis = Vector3.Cross(headLever, dir);
                if (headAxis.sqrMagnitude > 1e-8f)
                {
                    float snap = MostHeadSnap * Tanh(6f * j / (Mass * leg));
                    headV += headAxis.normalized * (snap * Kick * 2f * Mathf.PI * HeadFrequency);
                }
            }
        }

        /// <summary>A struck arm (or the shield it holds) knocked aside and turned, then back.</summary>
        private void KnockArm(int a, Vector3 impulse, Vector3 point)
        {
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
            float wk = 2f * Mathf.PI * ArmKnockFrequency;
            for (int a = 0; a < 2; a++)
            {
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
