using System.Collections.Generic;
using PhysicsBattler.Combat;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// A fighter's strikes meeting the bodies about it (the combat plan's step 4, first at the lab's dummy): about
    /// each strike's contact the weapon is swept from where it was last step to where it is now, and the first
    /// part, shield or sword of anyone's it meets is struck, by the fight's own damage rules
    /// (<see cref="DamageRules.Resolve"/>). Both stand still a moment (the hit stop, as the fight has it);
    /// a blow that cuts on through is slowed by what it gave up; one that is stopped (a shield, a body too
    /// heavy for it) backs out a little and fades into the guard (<see cref="Locomotor.CutShort"/>).
    /// </summary>
    /// <summary>What a defender reads of a blow coming (world): <see cref="StrikeHits.Read"/>.</summary>
    public struct BlowRead
    {
        /// <summary>The striking point now, where the blow will land, and the way it comes in there (unit: back up its path).</summary>
        public Vector3 Blade, Aim, ComesFrom;

        /// <summary>The way the attacker's blade will lie at contact (grip to tip, unit).</summary>
        public Vector3 BladeWay;

        /// <summary>Seconds until contact, and how long the blow has been coming (seconds as they pass).</summary>
        public float ToContact, Coming;

        /// <summary>Read ahead: a combo's next blow, seen coming out of the last one's follow-through before its own clip starts.</summary>
        public bool Predicted;

        /// <summary>The weapon's length (metres).</summary>
        public float Length;

        /// <summary>The striking point's path about contact (world, earliest first; <see cref="StrikeHits.ContactPoint"/> is at contact, <see cref="Aim"/>), the tip's and the grip's.</summary>
        public Vector3[] Path, TipPath, GripPath;

        /// <summary>How far apart the path's points are (seconds as they pass).</summary>
        public float PathSeconds;

        /// <summary>Seconds (as they pass) until the path reaches a point along it (an index, fractional).</summary>
        public float SecondsTo(float along) => Mathf.Max(0f, ToContact + (along - StrikeHits.ContactPoint) * PathSeconds);

        public bool Thrust;

        /// <summary>Which blow this is: it changes as each new blow starts, a combo's next one too (its clip taken up partway, its time into the clip doesn't start again from nought).</summary>
        public int Serial;
    }

    public sealed class StrikeHits
    {
        private readonly Locomotor loco;
        private readonly LocoBody poser;
        private readonly AnimRig anim;

        /// <summary>The weapon struck with (null: a plain sword of the poser's weapon length).</summary>
        public WeaponDefinition Weapon;

        /// <summary>The weapon's slot as it is seen (the physical hand's), or null: the posed one.</summary>
        public Transform Slot;

        /// <summary>The weapon's size against a Medium's, and the striker's mass (kg).</summary>
        public float Scale = 1f, Mass = 70f;

        /// <summary>Seconds of hit stop left: the striker and the struck stand still.</summary>
        public float HitStop { get; private set; }

        /// <summary>The blow playing is a combo's showstopper (its last): it lands harder (<see cref="FinisherKnock"/>) and its hit stop is longer (<see cref="FinisherStop"/>).</summary>
        public System.Func<bool> Finishing;

        /// <summary>What a blade met, for the sounds: a body, a shield, a blade it was stopped by, a blade it glanced off, or a light touch on a shield or a blade.</summary>
        public enum Met
        {
            Flesh,
            Shield,
            Parry,
            Glance,
            TouchShield,
            TouchBlade,
        }

        /// <summary>A blow met something (any lab attacker's): what, where, how fast (m/s), with a club, as a combo's showstopper.</summary>
        public static event System.Action<Met, Vector3, float, bool, bool> Sounded;

        /// <summary>A blow is about to land (its swing heard just before contact): where its blade is, with a club, as a showstopper.</summary>
        public static event System.Action<Vector3, bool, bool> Swung;

        /// <summary>The swing is heard this long before contact (seconds of the clip).</summary>
        public const float SwingHeardBefore = 0.14f;

        private bool swingHeard;

        private bool Club => Weapon != null && Weapon.kind == WeaponKind.Club;

        /// <summary>A blow of a combo: it drives the body it lands on back harder than a single blow (<see cref="ComboKnock"/>; the combo pulls itself in after it), and its slash sweeps on through the body rather than stopping in it (slowed to no less than <see cref="ComboCarry"/>).</summary>
        public System.Func<bool> Comboing;

        /// <summary>A showstopper's knock on the body it lands on (times a blow's), and a combo's blows' before it; and the showstopper's hit stop (seconds, least and most).</summary>
        public float FinisherKnock = 2f, ComboKnock = 1.5f, ComboCarry = 0.9f;
        public Vector2 FinisherStop = new Vector2(0.13f, 0.17f);

        private bool pausedThisBlow;

        /// <summary>The last blow that landed was a showstopper.</summary>
        public bool LastFinished { get; private set; }

        /// <summary>What the last blow did, for the panel and the log ("" before any).</summary>
        public string Last = "";

        /// <summary>A line for each blow, as it lands (the lab logs them).</summary>
        public event System.Action<string> Landed;

        public int Hits, Blocks, Cleaves;

        /// <summary>How long before and after its contact moment a strike's weapon can strike (seconds of the clip's
        /// own time: its blow, not its wind-up, which can pass close by the body it is about to strike).</summary>
        public float LiveBefore = 0.12f, LiveAfter = 0.2f;

        /// <summary>A cut-short blow backs out this much of its own time (seconds; a clip's downswing is quick, and
        /// backed out further it would go back up into its wind-up), at half its speed, then fades into the guard.</summary>
        public const float RewindMost = 0.06f, ReboundSpeed = 0.5f;

        /// <summary>A slowed blow's speed comes back over this long (seconds).</summary>
        public const float SlowedFor = 0.3f;

        private LocoClip blow;
        private bool had;
        private Vector3 lastGrip, lastTip, lastStrike;
        private float momentumLeft = 1f, lastShotTime;
        private readonly List<(ClipFighter body, Collider collider, PartId part, bool shield, float order)> found = new List<(ClipFighter, Collider, PartId, bool, float)>();

        /// <summary>Whose strikes these are: its own body is never struck by them.</summary>
        public ClipFighter Owner;

        public StrikeHits(Locomotor loco, LocoBody poser, AnimRig anim)
        {
            this.loco = loco;
            this.poser = poser;
            this.anim = anim;
        }

        private static bool IsThrust(LocoClip clip) => clip != null && clip.Kind != null && clip.Kind.StartsWith("thrust");

        private StrikeWith With(LocoClip clip) =>
            clip != null && IsThrust(clip) ? StrikeWith.Tip : Weapon != null && Weapon.kind == WeaponKind.Club ? StrikeWith.Head : StrikeWith.Edge;

        /// <summary>The weapon now: the swept stretch (from near the hand to the tip) and the striking point.</summary>
        public bool Blade(LocoClip clip, out Vector3 grip, out Vector3 tip, out Vector3 strike)
        {
            var slot = Slot != null ? Slot : anim.SlotWeaponR;
            grip = tip = strike = Vector3.zero;
            if (slot == null)
            {
                return false;
            }

            if (Weapon != null)
            {
                float from = Weapon.kind == WeaponKind.Club ? 0.35f : 0.1f;
                grip = slot.position + slot.rotation * (Weapon.tip * (from * Scale));
                tip = slot.position + slot.rotation * (Weapon.tip * Scale);
                strike = slot.position + slot.rotation * (Weapon.StrikePoint(With(clip)) * Scale);
                return true;
            }

            Vector3 way = slot.rotation * Vector3.forward * poser.WeaponLength;
            grip = slot.position + 0.1f * way;
            tip = slot.position + way;
            strike = slot.position + (IsThrust(clip) ? 1f : poser.SweetSpot) * way;
            return true;
        }

        /// <summary>
        /// A blow is coming (a strike playing, up to just after its contact): where its striking point is now, and
        /// where it is aimed. What a defender watches to meet it.
        /// </summary>
        public bool Incoming(out Vector3 blade, out Vector3 aim) => Incoming(out blade, out aim, out _);

        /// <summary>
        /// The same, and the way the blow comes in at its contact (world, unit: back up its striking point's path,
        /// from the clip): the line a defender reads from the wind-up, to put a shield across.
        /// </summary>
        public bool Incoming(out Vector3 blade, out Vector3 aim, out Vector3 comesFrom)
        {
            blade = aim = comesFrom = Vector3.zero;
            var clip = loco.Current == Locomotor.Mode.OneShot ? loco.Shot : null;
            // (A blow already met, set aside or slid off, is no longer coming: backing out it is short of its contact again.)
            if (clip == null || clip.ContactFrame < 0 || loco.ShotTime > clip.ContactFrame / clip.Rate + 0.1f || (stopped && clip == blow) || !Blade(clip, out _, out _, out blade))
            {
                return false;
            }

            aim = poser.AimLands;
            float contact = clip.ContactFrame / clip.Rate;
            Vector3 way = StrikeAt(clip, Mathf.Max(0f, contact - ReadOver)) - StrikeAt(clip, contact);
            // (Into the world as the root will face at contact: the clip's turn still to come on the facing now.)
            clip.RootMotion(Mathf.Min(loco.ShotTime, contact), contact, out _, out float turnLeft);
            comesFrom = way.sqrMagnitude > 1e-6f ? Quaternion.Euler(0f, loco.Facing + turnLeft, 0f) * way.normalized : (blade - aim).normalized;
            return true;
        }

        /// <summary>
        /// All a defender reads of the blow coming (<see cref="BlowRead"/>): the above, and the way the attacker's blade
        /// will lie at contact (grip to tip, world), how long until then, how long the blow has been coming, and
        /// whether it is a thrust.
        /// </summary>
        public bool Read(out BlowRead read)
        {
            read = default;
            if (!Incoming(out read.Blade, out read.Aim, out read.ComesFrom))
            {
                return ReadUpcoming(out read);
            }

            var clip = loco.Shot;
            // (A new blow: another clip, or the same one started again; the combo's next blow, read coming, is the same
            // blow it was read as.)
            if (clip != readClip || loco.ShotTime < readTime - 0.05f)
            {
                readSerial = clip == predictedFor ? predictedSerial : readSerial + 1;
                predictedFor = null;
            }

            readClip = clip;
            readTime = loco.ShotTime;
            read.Serial = readSerial;
            float contact = clip.ContactFrame / clip.Rate;
            clip.Pose(contact, readPose, loco.Leg);
            Vector3 way = StrikeCombos.Blade(readPose);
            clip.RootMotion(Mathf.Min(loco.ShotTime, contact), contact, out _, out float turnLeft);
            read.BladeWay = Quaternion.Euler(0f, loco.Facing + turnLeft, 0f) * way;
            // (In seconds as they pass: a smaller body's clips play faster.)
            float speed = Mathf.Max(0.05f, loco.ShotRate) * loco.TimeScale;
            read.ToContact = Mathf.Max(0f, contact - loco.ShotTime) / speed;
            read.Coming = loco.ShotTime / loco.TimeScale;
            read.Thrust = IsThrust(clip);
            read.Length = Weapon != null ? Weapon.length * Scale : poser.WeaponLength;
            // (The blade's path over the last of the blow and just past its contact: from where the blade is now, on
            // by the clip's arm as the root will carry it (the lunge cut or stretched, the aimed turn) and the aim's
            // pull as it will be then. A blow aimed at the chest from above passes the head first.)
            read.Path = new Vector3[PathPoints];
            read.TipPath = new Vector3[PathPoints];
            read.GripPath = new Vector3[PathPoints];
            read.PathSeconds = PathStep / speed;
            Blade(clip, out Vector3 gripNow, out Vector3 tipNow, out Vector3 strikeNow);
            float now = loco.ShotTime;
            var turnNow = Quaternion.Euler(0f, loco.Facing, 0f);
            Vector3 pullNow = poser.PullAhead(now, 0f);
            // (The blade as it lies now: its way, its length, and where along it the grip and the striking point are,
            // from the hand's end of it.)
            Vector3 wayNow = tipNow - gripNow;
            float gripAlong = Weapon != null ? (Weapon.kind == WeaponKind.Club ? 0.35f : 0.1f) : 0.1f;
            float bladeLength = wayNow.magnitude / Mathf.Max(0.05f, 1f - gripAlong);
            wayNow = wayNow.sqrMagnitude > 1e-8f ? wayNow.normalized : turnNow * Vector3.forward;
            Vector3 handNow = gripNow - wayNow * (gripAlong * bladeLength);
            float strikeAlong = Vector3.Dot(strikeNow - handNow, wayNow) / Mathf.Max(0.05f, bladeLength);
            Vector3 clipHandNow = turnNow * StrikeAt(clip, now, 0f);
            Vector3 ClipWay(Quaternion turn, float at)
            {
                Vector3 w = StrikeAt(clip, at, 1f) - StrikeAt(clip, at, 0f);
                return w.sqrMagnitude > 1e-8f ? turn * w.normalized : wayNow;
            }

            // (What the clip's way misses of the posed blade now, taken on: the hand's fade, the grip.)
            Vector3 steeredNow = poser.SteerAhead(now, handNow, ClipWay(turnNow, now));
            Quaternion wayFix = Quaternion.FromToRotation(steeredNow, wayNow);
            for (int k = 0; k < PathPoints; k++)
            {
                float t = Mathf.Max(now, contact + PathStep * (k - ContactPoint));
                loco.ShotRootAhead(t, out Vector3 rootThen, out float facingThen);
                var turnThen = Quaternion.Euler(0f, facingThen, 0f);
                Vector3 handThen = handNow + (rootThen - loco.Position) + poser.PullAhead(t, (t - now) / speed) - pullNow + turnThen * StrikeAt(clip, t, 0f) - clipHandNow;
                // (The blade's way then: the clip's, as the wrist will steer it.)
                Vector3 wayThen = poser.SteerAhead(t, handThen, wayFix * ClipWay(turnThen, t)).normalized;
                read.Path[k] = handThen + wayThen * (strikeAlong * bladeLength);
                read.TipPath[k] = handThen + wayThen * bladeLength;
                read.GripPath[k] = handThen + wayThen * (gripAlong * bladeLength);
            }

            // (Where it lands, as read so: the striking point at contact.)
            read.Aim = read.Path[ContactPoint];
            return true;
        }

        private LocoClip predictedFor;
        private int predictedSerial;
        private float predictedSince;

        /// <summary>
        /// The combo's next blow, read coming once the one playing is past its contact (<see cref="ComboPlayer.Upcoming"/>):
        /// its path from its core's entry on, played as the strike will be (the root the link and the step in leave it at,
        /// the turn onto its target, the lunge's stretch, the arm's pull as far as it goes). What a fighter reading the
        /// attacker sees of the blow coming out of the last.
        /// </summary>
        private bool ReadUpcoming(out BlowRead read)
        {
            read = default;
            var combo = Owner != null ? Owner.Combo : null;
            var now = loco.Current == Locomotor.Mode.OneShot ? loco.Shot : null;
            // (Past its contact, or met already: a blow set aside or slid off backs out from where it met.)
            if (combo == null || now == null || now.ContactFrame < 0 || (loco.ShotTime <= now.ContactFrame / now.Rate && !stopped)
                || !combo.Upcoming(out var clip, out float entry, out float toContact, out Vector3 rootAt, out float facing, out float lunge, out Vector3 pull))
            {
                return false;
            }

            if (predictedFor != clip)
            {
                predictedFor = clip;
                predictedSerial = readSerial + 1;
                predictedSince = clock;
            }

            float contact = clip.ContactFrame / clip.Rate;
            float pace = Mathf.Max(0.05f, loco.TimeScale);
            read.Serial = predictedSerial;
            read.Predicted = true;
            read.ToContact = toContact;
            // (Seen coming out of the last one's follow-through, which the defender has been watching all along: an
            // expert's eye is already on it; a novice's still lags its usual while behind.)
            read.Coming = clock - predictedSince + SwordParry.ReadAfter;
            read.Thrust = IsThrust(clip);
            read.Length = Weapon != null ? Weapon.length * Scale : poser.WeaponLength;
            read.PathSeconds = PathStep / pace;
            read.Path = new Vector3[PathPoints];
            read.TipPath = new Vector3[PathPoints];
            read.GripPath = new Vector3[PathPoints];
            float gripAlong = Weapon != null && Weapon.kind == WeaponKind.Club ? 0.35f : 0.1f;
            float strikeAlong = IsThrust(clip) ? 1f : poser.SweetSpot;
            var turn0 = Quaternion.Euler(0f, facing, 0f);
            Vector3 Point(float t, float along)
            {
                clip.RootMotion(entry, t, out var moved, out float turned);
                return rootAt + turn0 * new Vector3(moved.x, 0f, moved.y) * (lunge * loco.Leg) + Quaternion.Euler(0f, facing + turned, 0f) * StrikeAt(clip, t, along) + pull;
            }

            for (int k = 0; k < PathPoints; k++)
            {
                float t = Mathf.Max(entry, contact + PathStep * (k - ContactPoint));
                read.Path[k] = Point(t, strikeAlong);
                read.TipPath[k] = Point(t, 1f);
                read.GripPath[k] = Point(t, gripAlong);
            }

            read.Aim = read.Path[ContactPoint];
            Blade(now, out _, out _, out read.Blade);
            clip.RootMotion(entry, contact, out _, out float turnedAt);
            var turnAt = Quaternion.Euler(0f, facing + turnedAt, 0f);
            clip.Pose(contact, readPose, loco.Leg);
            read.BladeWay = turnAt * StrikeCombos.Blade(readPose);
            Vector3 way = StrikeAt(clip, Mathf.Max(entry, contact - ReadOver)) - StrikeAt(clip, contact);
            read.ComesFrom = way.sqrMagnitude > 1e-6f ? turnAt * way.normalized : (read.Blade - read.Aim).normalized;
            return true;
        }

        /// <summary>How many points of the blow's path are read (from 0.28 s before its contact to the end of its live moments), which is at contact, how far apart (seconds of the clip).</summary>
        public const int PathPoints = 13, ContactPoint = 7;
        public const float PathStep = 0.04f;

        /// <summary>How far before contact the blow's line is read from (seconds of the clip).</summary>
        public const float ReadOver = 0.06f;

        private readonly Vector3[] readPose = new Vector3[LocoClip.BoneCount];
        private LocoClip readClip;
        private float readTime = -1f;
        private int readSerial;

        /// <summary>A clip's striking point at a time, against its root then (metres at this body's size).</summary>
        private Vector3 StrikeAt(LocoClip clip, float t, float alongBlade = -1f)
        {
            clip.Pose(t, readPose, loco.Leg);
            float along = alongBlade >= 0f ? alongBlade : IsThrust(clip) ? 1f : poser.SweetSpot;
            return readPose[(int)LocoBone.RightHand] + StrikeCombos.Blade(readPose) * (poser.WeaponLength * along);
        }

        /// <summary>The parts of each body this blow has struck already (a blade that cuts through goes on to the next), whether it was stopped, and the bodies whose blade it has been deflected off.</summary>
        private readonly HashSet<(ClipFighter, PartId)> struckParts = new HashSet<(ClipFighter, PartId)>();
        private readonly HashSet<ClipFighter> deflectedBy = new HashSet<ClipFighter>();
        private bool stopped;

        /// <summary>When each body's parts (and its shield, and its sword) were last touched outside a blow (seconds).</summary>
        private readonly Dictionary<(ClipFighter, int), float> touched = new Dictionary<(ClipFighter, int), float>();
        private readonly List<ClipFighter> one = new List<ClipFighter>(1);
        private float clock;

        /// <summary>A weapon touching a body outside a blow (the wind-up passing through it, a guard too close) pushes it: at most once in this long per part (seconds), with the weapon's weight (kg).</summary>
        public const float TouchEvery = 0.3f, WeaponMass = 1.3f;

        /// <summary>The least speed a touching weapon pushes what it touches at (m/s at a Medium's size).</summary>
        public const float TouchSpeed = 1f;

        public int Touches;

        /// <summary>How many stopped blows were jumping strikes stopped in the air (played on into their landing).</summary>
        public int Landings;

        /// <summary>
        /// One step, after the striker is posed (and while the hit stop lasts, only its clock). In a strike's live
        /// moments the first part, shield or sword the blade meets is struck; a blade that cuts through goes on to
        /// strike the next part it meets. At any other time (the wind-up, the recovery, the guard), whatever the
        /// blade touches is pushed by it: nothing passes through a body without its feeling it.
        /// </summary>
        public void Step(float dt, ClipFighter body)
        {
            one.Clear();
            if (body != null)
            {
                one.Add(body);
            }

            Step(dt, one);
        }

        /// <summary>The same against every body in <paramref name="bodies"/> (its own skipped): the first part, shield or sword the blade meets, whoever's it is.</summary>
        public void Step(float dt, IReadOnlyList<ClipFighter> bodies)
        {
            clock += dt;
            if (HitStop > 0f)
            {
                HitStop = Mathf.Max(0f, HitStop - dt);
                return;
            }

            // (Bound to a parrying blade: a clean parry's sweep holds the strike still while it carries the weapon off; a
            // glance slides it along the blade, its blow tested as it goes.)
            if (bindWith != null && StepBind(dt))
            {
                return;
            }

            if (loco.Current == Locomotor.Mode.OneShot && loco.ShotSpeed > 0f && loco.ShotSpeed < 1f)
            {
                loco.ShotSpeed = Mathf.MoveTowards(loco.ShotSpeed, 1f, dt / SlowedFor);
            }

            var clip = loco.Current == Locomotor.Mode.OneShot ? loco.Shot : null;
            bool striking = clip != null && clip.ContactFrame >= 0;
            if (!striking)
            {
                blow = null;
            }
            else if (clip != blow)
            {
                blow = clip;
                EndBind();
                stopped = false;
                deflectedBy.Clear();
                spentOn.Clear();
                momentumLeft = 1f;
                struckParts.Clear();
                pausedThisBlow = false;
                swingHeard = false;
            }

            if (striking && !swingHeard && loco.ShotTime >= clip.ContactFrame / clip.Rate - SwingHeardBefore)
            {
                swingHeard = true;
                Swung?.Invoke(Slot != null ? Slot.position : anim.Part(PartId.HandR).position, Club, Finishing != null && Finishing());
            }

            if (!Blade(striking ? clip : null, out Vector3 grip, out Vector3 tip, out Vector3 strike))
            {
                had = false;
                return;
            }

            bool wasHad = had;
            Vector3 lastG = lastGrip, lastT = lastTip, lastS = lastStrike;
            int metBefore = Parries + Deflects;
            float shotWas = lastShotTime;
            lastGrip = grip;
            lastTip = tip;
            lastStrike = strike;
            lastShotTime = loco.ShotTime;
            had = true;
            if (!wasHad || bodies == null || bodies.Count == 0)
            {
                return;
            }

            // (Capped: a jump in one step is not a blow's speed.)
            float speed = Mathf.Min(Vector3.Distance(strike, lastS) / dt, 16f);
            float contact = striking ? clip.ContactFrame / clip.Rate : 0f, t = loco.ShotTime;
            bool live = striking && !stopped && t >= contact - LiveBefore && t <= contact + LiveAfter && loco.ShotSpeed > 0f
                        && speed >= (Weapon != null ? Weapon.minHitSpeed : 2f);
            Vector3 direction = strike - lastS;
            float radius = Mathf.Max(0.01f, (Weapon != null ? Weapon.sweepRadius : 0.035f) * Scale);
            float travel = Mathf.Max(Vector3.Distance(grip, lastG), Vector3.Distance(tip, lastT));
            int samples = Mathf.Clamp(Mathf.CeilToInt(travel / (1.5f * radius)), 1, 10);
            // (A blade the swing crossed between two samples is met all the same: the area the blade swept this step,
            // against each parrying sword; met at the sample it crossed it by.)
            ClipFighter crossBody = null;
            float crossS = 2f, crossOn = 0f;
            Vector3 crossAt = Vector3.zero;
            if (live)
            {
                foreach (var body in bodies)
                {
                    if (body == null || body == Owner || body.Stance != ClipFighter.Stances.Sword || body.Parry == null || deflectedBy.Contains(body))
                    {
                        continue;
                    }

                    body.Parry.BladeNow(out var cg, out var ct);
                    if (SegmentCrossesTriangle(cg, ct, lastG, lastT, tip, out float on, out Vector3 at) || SegmentCrossesTriangle(cg, ct, lastG, tip, grip, out on, out at))
                    {
                        Vector3 m0 = 0.5f * (lastG + lastT), m1 = 0.5f * (grip + tip);
                        float sw = Mathf.Clamp01(Vector3.Dot(at - m0, m1 - m0) / Mathf.Max(1e-8f, (m1 - m0).sqrMagnitude));
                        if (sw < crossS)
                        {
                            crossS = sw;
                            crossBody = body;
                            crossOn = on;
                            crossAt = at;
                        }
                    }
                }
            }

            for (int i = 1; i <= samples; i++)
            {
                float s = i / (float)samples;
                Vector3 a = Vector3.Lerp(lastG, grip, s), b = Vector3.Lerp(lastT, tip, s);
                found.Clear();
                // (A defender's own sword, parrying: blade against blade, two segments' nearest points; the nearest the
                // hand first, of all of them.)
                float parryAlong = -1f;
                Vector3 parryAt = Vector3.zero;
                ClipFighter parrier = null;
                foreach (var body in bodies)
                {
                    if (body == null || body == Owner)
                    {
                        continue;
                    }

                    if (body.Stance == ClipFighter.Stances.Sword && body.Parry != null && !deflectedBy.Contains(body))
                    {
                        body.Parry.BladeNow(out var dg, out var dt2);
                        float defRadius = 0.02f * Scale;
                        if (SegmentsNearest(a, b, dg, dt2, out float sa, out float sd, out Vector3 pa, out Vector3 pd) <= radius + defRadius
                            && (parrier == null || sa < parryAlong))
                        {
                            parryAlong = sa;
                            parryAt = 0.5f * (pa + pd);
                            lastParryOn = sd;
                            parrier = body;
                        }
                    }
                }

                if (parrier == null && crossBody != null && s >= crossS - 1e-4f)
                {
                    parrier = crossBody;
                    parryAt = crossAt;
                    lastParryOn = crossOn;
                    parryAlong = Mathf.Clamp01(Vector3.Dot(crossAt - a, b - a) / Mathf.Max(1e-8f, (b - a).sqrMagnitude));
                }

                foreach (var collider in Physics.OverlapCapsule(a, b, radius, ~0, QueryTriggerInteraction.Collide))
                {
                    foreach (var body in bodies)
                    {
                        if (body == null || body == Owner)
                        {
                            continue;
                        }

                        if (body.ShieldColliders.Contains(collider))
                        {
                            found.Add((body, collider, PartId.HandL, true, AlongBlade(collider, a, b) - 0.05f));
                            break;
                        }

                        if (body.PartOf(collider, out var id))
                        {
                            if (!(live && (struckParts.Contains((body, id)) || spentOn.Contains(body))))
                            {
                                found.Add((body, collider, id, false, AlongBlade(collider, a, b)));
                            }

                            break;
                        }
                    }
                }

                if (found.Count == 0 && parryAlong < 0f)
                {
                    continue;
                }

                // (Nearest the hand first: the blade turns about the hand, so what is nearer it is in the way.)
                found.Sort((x, y) => x.order.CompareTo(y.order));
                // (The parrying sword before what it guards: its arm and hand, which hold it behind the blade, and
                // whatever is further along the attacker's blade.)
                bool parryFirst = parryAlong >= 0f && (found.Count == 0 || parryAlong - 0.05f <= found[0].order
                    || (found[0].body == parrier && (found[0].part == PartId.HandR || found[0].part == PartId.ForearmR)));
                if (!live)
                {
                    // Outside a blow: a touch, a push on what it touches.
                    Vector3 velocity = direction / Mathf.Max(1e-4f, dt);
                    if (parryFirst)
                    {
                        Touch(parrier, -2, PartId.HandR, false, true, parryAt, velocity);
                    }
                    else
                    {
                        var ft = found[0];
                        Touch(ft.body, ft.shield ? -1 : (int)ft.part, ft.part, ft.shield, false, ft.collider.ClosestPoint(0.5f * (a + b)), velocity);
                    }

                    return;
                }

                if (parryFirst)
                {
                    float metAt = Mathf.Lerp(shotWas, loco.ShotTime, (i - 1) / (float)samples);
                    Strike(clip, parrier, PartId.HandR, true, parryAt, speed, direction, metAt, parryAlong, b - a);
                    return;
                }

                var f = found[0];
                // (The fist that holds the shield is behind it: met with the shield, the shield takes the blow.)
                if (!f.shield && (f.part == PartId.HandL || f.part == PartId.ForearmL))
                {
                    var holder = f.body;
                    int s0 = found.FindIndex(x => x.shield && x.body == holder);
                    if (s0 >= 0)
                    {
                        f = found[s0];
                    }
                    else if (holder.Stance == ClipFighter.Stances.Shield && holder.ShieldColliders.Count > 0 && DamageRules.Blunt(With(clip)))
                    {
                        // (Not touched this step, the board just behind the arm still takes a blunt blow: stopped in the
                        // forearm before it reached the board, where a cleaving one had gone on into it.)
                        var board = System.Linq.Enumerable.First(holder.ShieldColliders);
                        Vector3 at = f.collider.ClosestPoint(0.5f * (a + b));
                        if (Vector3.Distance(board.ClosestPoint(at), at) < 0.2f * holder.Size)
                        {
                            f = (holder, board, PartId.HandL, true, f.order);
                        }
                    }
                }

                // (Where in this step it met it: a stopped blow is put back there.)
                float met = Mathf.Lerp(shotWas, loco.ShotTime, (i - 1) / (float)samples);
                CountPassing(bodies, lastG, lastT, grip, tip, striking, live, metBefore);
                Strike(clip, f.body, f.part, f.shield, f.collider.ClosestPoint(0.5f * (a + b)), speed, direction, met);
                return;
            }

            CountPassing(bodies, lastG, lastT, grip, tip, striking, live, metBefore);
        }

        /// <summary>
        /// Steps in which the blade swept through another's sword without meeting it (the plan "Parries that divert",
        /// step 1): the area it swept this step against each sword. In a blow's live moments, and outside them (its
        /// wind-up, its recovery). What the bind is to bring to none.
        /// </summary>
        public int PassedThroughLive, PassedThroughOutside;

        private void CountPassing(IReadOnlyList<ClipFighter> bodies, Vector3 g0, Vector3 t0, Vector3 g1, Vector3 t1, bool striking, bool live, int metBefore)
        {
            if (!striking || Parries + Deflects != metBefore)
            {
                return;
            }

            foreach (var body in bodies)
            {
                if (body == null || body == Owner || body.Stance != ClipFighter.Stances.Sword || body.Parry == null)
                {
                    continue;
                }

                body.Parry.BladeNow(out var dg, out var dt2);
                if (SegmentCrossesTriangle(dg, dt2, g0, t0, t1) || SegmentCrossesTriangle(dg, dt2, g0, t1, g1))
                {
                    if (live)
                    {
                        PassedThroughLive++;
                    }
                    else
                    {
                        PassedThroughOutside++;
                    }
                }
            }
        }

        /// <summary>Whether the segment p to q passes through the triangle a, b, c (Moller and Trumbore).</summary>
        public static bool SegmentCrossesTriangle(Vector3 p, Vector3 q, Vector3 a, Vector3 b, Vector3 c) => SegmentCrossesTriangle(p, q, a, b, c, out _, out _);

        /// <summary>The same, with where along the segment (0 to 1) and the point it crosses at.</summary>
        public static bool SegmentCrossesTriangle(Vector3 p, Vector3 q, Vector3 a, Vector3 b, Vector3 c, out float along, out Vector3 at)
        {
            along = 0f;
            at = Vector3.zero;
            Vector3 d = q - p, e1 = b - a, e2 = c - a;
            Vector3 h = Vector3.Cross(d, e2);
            float det = Vector3.Dot(e1, h);
            if (Mathf.Abs(det) < 1e-9f)
            {
                return false;
            }

            float inv = 1f / det;
            Vector3 s0 = p - a;
            float u = inv * Vector3.Dot(s0, h);
            if (u < 0f || u > 1f)
            {
                return false;
            }

            Vector3 qv = Vector3.Cross(s0, e1);
            float v = inv * Vector3.Dot(d, qv);
            if (v < 0f || u + v > 1f)
            {
                return false;
            }

            along = inv * Vector3.Dot(e2, qv);
            at = p + d * along;
            return along >= 0f && along <= 1f;
        }

        /// <summary>A touch outside a blow: what the blade touches is pushed by the weapon's weight at its speed.</summary>
        private void Touch(ClipFighter struck, int key, PartId part, bool shield, bool sword, Vector3 point, Vector3 velocity)
        {
            // (Only a weapon moving into it pushes: one resting against it, swaying with its holder, is felt but not knocked.)
            if (velocity.magnitude < TouchSpeed * Mathf.Sqrt(Scale) || (touched.TryGetValue((struck, key), out float last) && clock - last < TouchEvery))
            {
                return;
            }

            touched[(struck, key)] = clock;
            Touches++;
            if (shield || sword)
            {
                Sounded?.Invoke(shield ? Met.TouchShield : Met.TouchBlade, point, velocity.magnitude, Club, false);
            }

            struck.Touch(part, shield, sword, point, Vector3.ClampMagnitude(velocity, 8f) * (WeaponMass * Scale));
        }

        private void Strike(LocoClip clip, ClipFighter struck, PartId part, bool shield, Vector3 point, float speed, Vector3 direction, float met,
            float parryAlong = -1f, Vector3 bladeWay = default)
        {
            bool parried = parryAlong >= 0f;
            if (!parried && !shield)
            {
                struckParts.Add((struck, part));
            }

            var r = DamageRules.Resolve(Weapon, With(clip), Scale, speed, direction, Mass, struck.Mass, part, shield, momentumLeft,
                1e6f, struck.Poise, struck.MaxPoise);
            // (A blunt blow never cleaves (the plan "Weight and momentum", step 2): a club's head, a fist, a shield's face
            // stop in the first body they meet and their weight goes into it, a shove, not a cut through. The playtest:
            // one Large club blow struck a Medium's head, upper arm and pelvis in turn. The fight's own swing still
            // cleaves until it takes these rules up.)
            if (!shield && DamageRules.Blunt(With(clip)))
            {
                r.StopsSwing = true;
                r.MomentumLeft = 0f;
            }
            bool glanced = false;
            if (parried)
            {
                struck.Parry.BladeNow(out var dg, out var dtip);
                LastCross = Vector3.Angle(bladeWay, dtip - dg);
                LastCross = Mathf.Min(LastCross, 180f - LastCross);
                LastParryAt = parryAlong;
                // (A clean parry stops the blow: taken on the strong of the blade, near square. Taken out on the
                // weak of it, or at a slant, the blow glances off and goes on, slowed.)
                glanced = lastParryOn > CleanOnBlade || LastCross < CleanCross;
                if (glanced)
                {
                    deflectedBy.Add(struck);
                    Deflects++;
                    // (Turned off the dummy's blade: away from the arm that holds it.)
                    struckParts.Add((struck, PartId.HandR));
                    struckParts.Add((struck, PartId.ForearmR));
                    struckParts.Add((struck, PartId.UpperArmR));
                }
                else
                {
                    Parries++;
                }
            }

            // (A combo's showstopper lands harder, and the moment it lands is held longer: Bam! Bam! BAM!)
            bool finishing = Finishing != null && Finishing() && !glanced;
            bool comboing = Comboing != null && Comboing();
            LastFinished = finishing;
            float knock = finishing ? FinisherKnock : comboing ? ComboKnock : 1f;
            // (The plan "Weight and momentum", step 4: a square parry holds a blow up to its skill's weight; a heavier one
            // beats it, the blade crushed back into the body and part of the blow landing through it. A glance, taken at a
            // slant, is never beaten. The showstopper counts double, as it lands.)
            bool beaten = parried && !glanced && !loco.ShotAirborne() && r.Weight * (finishing ? FinisherKnock : 1f) > struck.ParryHolds;
            if (beaten)
            {
                Parries--;
                Beaten++;
            }
            // (A slanted shield turns part of the blow aside; one taken square on takes all of it.)
            if (shield && !parried)
            {
                knock *= 1f - struck.ShieldGlance;
            }
            float onBlade = beaten ? 1f - CombatFeel.BeatenShare : 1f;
            struck.TakeHit(part, shield && !parried, (glanced ? r.Impulse * GlanceLeft : r.Impulse) * (knock * onBlade), r.PartShare, r.PoiseDamage * knock * onBlade, r.Reaction, point, parried,
                bound: parried && !glanced && !loco.ShotAirborne());

            // Hit stop: both stand still a moment, longer for a heavier blow (as the fight has it).
            // (Longer for a heavier blow (the plan "Weight and momentum", step 3), by its weight: by its momentum, a
            // sword already held it the most, 0.1 s, as long as the Large's club. The scene's Combat Feel sets it.)
            HitStop = Mathf.Clamp(CombatFeel.HitStopBase + CombatFeel.HitStopPerWeight * r.Weight * knock, CombatFeel.LeastHitStop, CombatFeel.MostHitStop);
            if (finishing)
            {
                HitStop = Mathf.Clamp(HitStop * 1.7f, FinisherStop.x, FinisherStop.y);
            }

            // (One hit stop a blow: a slash sweeping on through the body strikes part after part, and stopping at each
            // held a combo's showstopper still for 0.6 s.)
            if (pausedThisBlow && !parried && !(shield && !parried))
            {
                HitStop = 0f;
            }

            pausedThisBlow = true;
            string what;
            if (glanced)
            {
                // Glanced off the dummy's blade: on, slowed, its hand jolted; the sweep goes on to what lies beyond.
                momentumLeft = Mathf.Min(momentumLeft, GlanceLeft);
                loco.ShotSpeed = Mathf.Max(0.3f, GlanceLeft);
                // (Back to where the blades met and held there: the bind slides it on along the blade, past its point.)
                loco.WindBack(met);
                poser.Pose(1e-4f);
                loco.ShotSpeed = 0f;
                stopped = true;
                StartBind(struck, point, speed, direction, parryAlong, true, 0f);
                what = $"glanced off the {struck.Parry.WeaponName} ({struck.Parry.CurrentName}, crossed at {LastCross:0} degrees, on its blade {lastParryOn:0.00} from the hilt): on, slowed";
                HitStop = Mathf.Min(HitStop, 0.04f);
            }
            // (A combo's blow sweeps on through a body it would stop in, Bam and on; a blunt one doesn't: it stops there,
            // its weight shoved into the body, and the combo's next blow is taken up from there.)
            else if ((r.StopsSwing && !(comboing && !shield && !DamageRules.Blunt(With(clip)))) || parried)
            {
                stopped = true;
                // (A clean parry ends the combo: its next blow doesn't come. One beaten doesn't.)
                if (parried && !beaten && Owner != null && Owner.Combo != null && Owner.Combo.Playing)
                {
                    Owner.Combo.Stop();
                    ComboEnded++;
                }

                if (parried && !loco.ShotAirborne())
                {
                    // Set aside (the plan "Parries that divert"): back to where the blades met, held there through the
                    // hit stop; then the parrying sword sweeps it off the body at the blow's own speed, the two blades
                    // together, and only then does it back out. A clean parry ends the combo.
                    loco.WindBack(met);
                    poser.Pose(1e-4f);
                    loco.ShotSpeed = 0f;
                    StartBind(struck, point, speed, direction, parryAlong, false, r.Weight * knock);
                    Owner?.Jar(-direction.normalized * (r.Impulse.magnitude * knock * (beaten ? 0.5f : 1f)), point);
                    if (beaten)
                    {
                        BeatenThrough(clip, struck, point, speed, direction, knock);
                    }
                }
                else
                {
                // (The blades or the shield meet: the attacker's hand thrown back along the blow, its arm taking it; by the
                // blow's weight (the plan "Weight and momentum", step 4), the heavier the harder it is stopped and the more
                // it jars the attacker. Stopped in a body, as before.)
                if (shield || parried)
                {
                    poser.KnockHand(-direction.normalized * Rebound(r.Weight * knock));
                    Owner?.Jar(-direction.normalized * (r.Impulse.magnitude * knock), point);
                }
                else
                {
                    poser.KnockHand(-direction.normalized * Mathf.Min(speed * 0.25f, 3f));
                }
                // Back to where the weapon met it, shown through the hit stop; then it backs out a little. A jumping
                // strike met in the air can't back out: it comes down, its landing played on, the arm thrown back.
                if (loco.ShotAirborne())
                {
                    Landings++;
                    poser.HoldWeaponArm();
                    loco.StopShotTravel();
                }
                else
                {
                    loco.WindBack(met);
                    poser.Pose(1e-4f);
                    loco.CutShort(RewindMost, ReboundSpeed);
                }
                }

                what = beaten ? $"beat the parry ({struck.Parry.CurrentName}, crossed at {LastCross:0} degrees, on its blade {lastParryOn:0.00} from the hilt; it holds {struck.ParryHolds:0.0} m/s)"
                    : parried ? $"parried by the {struck.Parry.WeaponName} ({struck.Parry.CurrentName}, crossed at {LastCross:0} degrees, on its blade {lastParryOn:0.00} from the hilt, on the attacker's {parryAlong:0.00}{(Bound ? $"; it gave {bindGiveNeed:0.00} m" : "")})"
                    : shield ? (struck.ShieldGlance > 0.25f ? "blocked by the shield, turned aside" : "blocked by the shield, square on") : "stopped in the " + Name(part);
                if (shield && !parried)
                {
                    Blocks++;
                }
                else if (!parried)
                {
                    Hits++;
                }
            }
            else
            {
                momentumLeft = r.MomentumLeft;
                loco.ShotSpeed = Mathf.Max(comboing ? ComboCarry : 0.3f, r.MomentumLeft);
                what = (comboing && r.StopsSwing ? "swept through the " : "cut through the ") + Name(part);
                Hits++;
                Cleaves++;
            }

            // (Heard once a blow for its first contact: a slash sweeping on through part after part is one blow.)
            if (!(pausedThisBlow && HitStop <= 0f && !parried && !shield) || struckParts.Count <= 1)
            {
                Sounded?.Invoke(glanced ? Met.Glance : parried ? Met.Parry : shield ? Met.Shield : Met.Flesh, point, speed, Club, finishing);
            }

            LastWeight = r.Weight * knock * (glanced ? GlanceLeft : 1f);
            Last = $"{clip.Name} at {speed:0.0} m/s: {what} ({r.Reaction.ToString().ToLowerInvariant()}, impulse {r.Impulse.magnitude:0} N·s, weight {LastWeight:0.00} m/s, hit stop {HitStop:0.00} s{(finishing ? ", the showstopper" : "")})";
            Landed?.Invoke(Last);
        }

        private static string Name(PartId part)
        {
            string n = part.ToString();
            return n.EndsWith("L") ? "left " + n.Substring(0, n.Length - 1).ToLowerInvariant()
                : n.EndsWith("R") ? "right " + n.Substring(0, n.Length - 1).ToLowerInvariant()
                : n.ToLowerInvariant();
        }

        // ---- The bind (the plan "Parries that divert") --------------------------------------------------------

        private ClipFighter bindWith;
        private bool bindGlance;
        private float bindSpeed, bindNeed, bindCarried, bindSlid, bindTime, bindAttackAlong;
        private Vector3 bindOut;

        /// <summary>A bind lasts at most this long (seconds), whatever is left of its sweep or its slide.</summary>
        public const float MostBind = 0.25f;

        /// <summary>A clean parry carries the blow out to this far beyond the parrying body's middle (metres at a Medium's size: half its width and a margin), at least this much, at most this much.</summary>
        public const float SetAsideTo = 0.4f, LeastSetAside = 0.1f;

        /// <summary>Binds begun; blows set aside (a clean parry's sweep); blows slid off the point (a glance); combos a clean parry ended.</summary>
        public int Binds, SetAside, SlidOff, ComboEnded;

        /// <summary>Square parries beaten by a blow too heavy for them (the plan "Weight and momentum", step 4).</summary>
        public int Beaten;

        /// <summary>Bound to a parrying blade now.</summary>
        public bool Bound => bindWith != null;

        /// <summary>
        /// The blades met on <paramref name="with"/>'s sword at <paramref name="point"/>: from here they stay together.
        /// The way out is square to its blade, away from its chest through where they met: where a sweep carries a
        /// blow off the body. A clean parry is carried out that way until the blow is past the body; a glance slides on
        /// along the blade to its point, never through it.
        /// </summary>
        private void StartBind(ClipFighter with, Vector3 point, float speed, Vector3 direction, float attackAlong, bool glance, float weight)
        {
            if (with == null || with.Parry == null)
            {
                return;
            }

            with.Parry.BladeNow(out var dg, out var dtip);
            Vector3 way = (dtip - dg).normalized;
            Vector3 chest = with.Anim.Part(PartId.Chest).position;
            // (Out the side of the blade the blow came in from, which is the side the weapon is on: pushed the other
            // way it would be driven through the blade. Turned toward away from the chest as far as that side allows.)
            Vector3 back = Vector3.ProjectOnPlane(-direction, way);
            back = back.sqrMagnitude > 1e-8f ? back.normalized : Vector3.up;
            Vector3 away = Vector3.ProjectOnPlane(point - chest, way);
            away = away.sqrMagnitude > 1e-8f ? away.normalized : back;
            Vector3 both = back + 0.5f * away;
            bindOut = both.sqrMagnitude > 1e-6f && Vector3.Dot(both.normalized, back) > 0.3f ? both.normalized : back;
            bindWith = with;
            bindGlance = glance;
            bindSpeed = Mathf.Max(2f, speed);
            float sz = with.Size;
            bindNeed = Mathf.Clamp(SetAsideTo * sz - Vector3.Dot(point - chest, bindOut), LeastSetAside * sz, SetAsideTo * sz);
            bindCarried = bindSlid = bindTime = 0f;
            bindAttackAlong = Mathf.Clamp01(attackAlong);
            // (A glance slides on along the blade, out past its point: from where they met to the point and the
            // weapon's own thickness beyond.)
            float length = Vector3.Distance(dg, dtip);
            float onBlade = Mathf.Clamp01(Vector3.Dot(point - dg, way) / Mathf.Max(0.05f, length));
            float thick = Mathf.Max(0.01f, (Weapon != null ? Weapon.sweepRadius : 0.035f) * Scale) + 0.02f * sz;
            // (The sword slides along itself at once so the blow sits on its middle: met by the long shaft of a club near
            // the hilt, over the hand, it is held on the middle instead; the blow stays where it was met.)
            float toMiddle = Mathf.Clamp(onBlade - SwordParry.HoldAt, -0.35f, 0.35f) * length;
            if (!glance && Mathf.Abs(toMiddle) > 0.01f)
            {
                with.Parry.Sweep(way * toMiddle);
                onBlade -= toMiddle / Mathf.Max(0.05f, length);
            }

            bindAlongWay = way;
            bindSlideNeed = glance ? (1f - onBlade) * length + thick : 0f;
            // (The plan "Weight and momentum", step 4: held, the parry first gives along the blow by its weight, the
            // blow on the blade the while, then sweeps it out; never so far the blade reaches the body.)
            bindGiveWay = direction.sqrMagnitude > 1e-8f ? direction.normalized : -bindOut;
            float room = Mathf.Max(0f, Vector3.Dot(chest - point, bindGiveWay) - CrushStop * sz);
            bindGiveNeed = glance ? 0f : Mathf.Min(with.ParryGive(weight), 0.5f * room);
            bindGiven = 0f;
            bindNeed += Mathf.Max(0f, -Vector3.Dot(bindGiveWay * bindGiveNeed, bindOut));
            bindBeaten = false;
            bindWeight = weight;
            Binds++;
        }

        /// <summary>How fast a blow of this weight (m/s) stopped on a shield or a blade throws the attacker's hand back (m/s; the scene's Combat Feel).</summary>
        public static float Rebound(float weight) => Mathf.Clamp(CombatFeel.ReboundPerWeight * weight, CombatFeel.LeastRebound, CombatFeel.MostRebound);

        private float bindWeight;

        /// <summary>A beaten parry's blade is crushed back to this far short of the body's middle (metres at a Medium's size).</summary>
        public const float CrushStop = 0.15f;

        private Vector3 bindGiveWay;
        private float bindGiveNeed, bindGiven;
        private bool bindBeaten;
        private LocoClip crushClip;
        private float crushKnock, crushSpeed;
        private Vector3 crushDirection;

        /// <summary>
        /// The parry bound is beaten (the plan "Weight and momentum", step 4): instead of the sweep out, the blow drives
        /// the blade back into the body, the two weapons together, at the blow's speed; there part of the blow lands
        /// (<see cref="CombatFeel.BeatenShare"/>) on what the blade is crushed into. A combo goes on.
        /// </summary>
        private void BeatenThrough(LocoClip clip, ClipFighter with, Vector3 point, float speed, Vector3 direction, float knock)
        {
            Vector3 chest = with.Anim.Part(PartId.Chest).position;
            float sz = with.Size;
            bindBeaten = true;
            bindNeed = 0f;
            bindGiveNeed = Mathf.Clamp(Vector3.Dot(chest - point, bindGiveWay) - CrushStop * sz, 0.05f * sz, 0.4f * sz);
            crushClip = clip;
            crushKnock = knock;
            crushSpeed = speed;
            crushDirection = direction;
        }

        /// <summary>The beaten parry crushed home: the rest of the blow lands on the part nearest where the blade is.</summary>
        private void Crush(ClipFighter with, Vector3 at)
        {
            var part = PartId.Chest;
            float best = float.MaxValue;
            foreach (var id in new[] { PartId.Head, PartId.Chest, PartId.Spine, PartId.Pelvis, PartId.UpperArmL, PartId.UpperArmR })
            {
                float d = (with.Anim.Part(id).position - at).sqrMagnitude;
                if (d < best)
                {
                    best = d;
                    part = id;
                }
            }

            var r = DamageRules.Resolve(Weapon, crushClip != null ? With(crushClip) : StrikeWith.Head, Scale, crushSpeed, crushDirection, Mass, with.Mass, part, false, 1f,
                1e6f, with.Poise, with.MaxPoise);
            float share = CombatFeel.BeatenShare * crushKnock;
            struckParts.Add((with, part));
            with.TakeHit(part, false, r.Impulse * share, r.PartShare, r.PoiseDamage * share, r.Reaction, at);
            Hits++;
            float weight = r.Weight * share;
            HitStop = Mathf.Clamp(CombatFeel.HitStopBase + CombatFeel.HitStopPerWeight * weight, CombatFeel.LeastHitStop, CombatFeel.MostHitStop);
            Sounded?.Invoke(Met.Flesh, at, crushSpeed, Club, false);
            LastWeight = weight;
            Last = $"{(crushClip != null ? crushClip.Name : "the blow")}: the beaten parry crushed into the {Name(part)} ({r.Reaction.ToString().ToLowerInvariant()}, weight {weight:0.00} m/s, hit stop {HitStop:0.00} s); {with.LastReaction}";
            Landed?.Invoke(Last);
        }

        /// <summary>
        /// One step of the bind; true while it holds the strike, so nothing else is tested. The strike is held where the
        /// blades met and its weapon moved only by the bind, at the blow's own speed: a clean parry's sweep carries it
        /// out off the body, then it backs out; a glance carries it out and on along the blade past its point, then
        /// plays on from there, slowed, still carried off line until its live moments are over.
        /// </summary>
        private bool StepBind(float dt)
        {
            bindTime += dt;
            if (bindWith == null || bindWith.Parry == null || bindWith.Fallen)
            {
                EndBind();
                return false;
            }

            float step = bindSpeed * dt;
            // (Held: the give along the blow first, the sword with it; beaten, the give is the crush.)
            if (bindGiven < bindGiveNeed - 1e-5f)
            {
                float give = Mathf.Min(step * (bindBeaten ? 0.6f : 1f), bindGiveNeed - bindGiven);
                bindGiven += give;
                bindWith.Parry.Sweep(bindGiveWay * give);
                poser.Bind(bindGiveWay * bindGiven);
                if (bindGiven >= bindGiveNeed - 1e-5f && bindBeaten)
                {
                    bindWith.Parry.BladeNow(out var cg, out var ct);
                    var with0 = bindWith;
                    Crush(with0, Vector3.Lerp(cg, ct, SwordParry.HoldAt));
                    spentOn.Add(with0);
                    EndBind();
                    loco.CutShort(RewindMost, ReboundSpeed);
                    return true;
                }

                if (bindTime < MostBind)
                {
                    return true;
                }
            }

            float by = Mathf.Min(step, bindNeed - bindCarried);
            bindCarried += by;
            if (by > 0f)
            {
                bindWith.Parry.Sweep(bindOut * by);
            }

            bindSlid = Mathf.Min(bindSlideNeed, bindSlid + step);
            poser.Bind(bindGiveWay * bindGiven + bindOut * bindCarried + bindAlongWay * bindSlid);
            bool done = (bindCarried >= bindNeed - 1e-4f && bindSlid >= bindSlideNeed - 1e-4f) || bindTime >= MostBind;
            if (!done)
            {
                return true;
            }

            var with = bindWith;
            if (!bindGlance)
            {
                SetAside++;
                EndBind();
                // (Carried off, and jarred back by its weight, half as hard as off a shield: the sweep took some of it.)
                poser.KnockHand(bindOut * Mathf.Min(bindSpeed * 0.1f, 1.5f) - bindGiveWay * (0.5f * Rebound(bindWeight)));
                loco.CutShort(RewindMost, ReboundSpeed);
                return true;
            }

            // (Slid off its point: spent on the one it slid off, it backs out as a stopped blow does. Played on, its
            // swing carried it back across the blade it had slid off. A glance doesn't end the combo: its next blow comes.)
            SlidOff++;
            spentOn.Add(with);
            EndBind();
            loco.CutShort(RewindMost, ReboundSpeed);
            return true;
        }

        /// <summary>The bodies this blow was set aside by: it lands on nothing of theirs any more.</summary>
        private readonly HashSet<ClipFighter> spentOn = new HashSet<ClipFighter>();

        private float bindSlideNeed;
        private Vector3 bindAlongWay;

        private void EndBind()
        {
            if (bindWith != null && bindWith.Parry != null)
            {
                bindWith.Parry.EndSweep();
            }

            poser.LetGo();
            bindWith = null;
        }

        /// <summary>The last contact's weight (m/s: the momentum the struck body took over its mass, as <see cref="DamageRules"/> has it, with the combo's knock).</summary>
        public float LastWeight;

        /// <summary>The last parry: the angle the blades crossed at (degrees, 0 to 90), and where on each it met (0 the hilt's end, 1 the tip).</summary>
        public float LastCross, LastParryAt;
        private float lastParryOn;

        public int Parries, Deflects;

        /// <summary>A parry stops the blow when it is taken within this share of the dummy's blade from its hilt, crossing at this angle or more (degrees); otherwise the blow glances off, left with this much of its momentum.</summary>
        public const float CleanOnBlade = 0.65f, CleanCross = 40f, GlanceLeft = 0.5f;

        /// <summary>The nearest points of two segments (their shares along each, and the points), and how far apart they are.</summary>
        public static float SegmentsNearest(Vector3 p1, Vector3 q1, Vector3 p2, Vector3 q2, out float s, out float t, out Vector3 c1, out Vector3 c2)
        {
            Vector3 d1 = q1 - p1, d2 = q2 - p2, r = p1 - p2;
            float a = Vector3.Dot(d1, d1), e = Vector3.Dot(d2, d2), f = Vector3.Dot(d2, r);
            if (a <= 1e-8f && e <= 1e-8f)
            {
                s = t = 0f;
                c1 = p1;
                c2 = p2;
                return (c1 - c2).magnitude;
            }

            if (a <= 1e-8f)
            {
                s = 0f;
                t = Mathf.Clamp01(f / e);
            }
            else
            {
                float c = Vector3.Dot(d1, r);
                if (e <= 1e-8f)
                {
                    t = 0f;
                    s = Mathf.Clamp01(-c / a);
                }
                else
                {
                    float b = Vector3.Dot(d1, d2), denom = a * e - b * b;
                    s = denom > 1e-8f ? Mathf.Clamp01((b * f - c * e) / denom) : 0f;
                    t = (b * s + f) / e;
                    if (t < 0f)
                    {
                        t = 0f;
                        s = Mathf.Clamp01(-c / a);
                    }
                    else if (t > 1f)
                    {
                        t = 1f;
                        s = Mathf.Clamp01((b - c) / a);
                    }
                }
            }

            c1 = p1 + d1 * s;
            c2 = p2 + d2 * t;
            return (c1 - c2).magnitude;
        }

        /// <summary>How far out along the blade (0 at the hand's end, 1 at the tip) a collider touches it.</summary>
        private static float AlongBlade(Collider collider, Vector3 a, Vector3 b)
        {
            Vector3 ab = b - a;
            float length = Mathf.Max(1e-6f, ab.sqrMagnitude);
            float T(Vector3 p) => Mathf.Clamp01(Vector3.Dot(p - a, ab) / length);
            Vector3 near = collider.ClosestPoint(0.5f * (a + b));
            near = collider.ClosestPoint(a + ab * T(near));
            return T(near);
        }
    }
}
