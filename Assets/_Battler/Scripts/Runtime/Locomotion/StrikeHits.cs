using System.Collections.Generic;
using PhysicsBattler.Combat;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// A walker's strikes meeting a body (the combat plan's step 4, at the lab's dummy): about each
    /// strike's contact the weapon is swept from where it was last step to where it is now, and the first
    /// part or shield of the dummy it meets is struck, by the fight's own damage rules
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

        /// <summary>The weapon's length (metres).</summary>
        public float Length;

        /// <summary>The striking point's path about contact (world, earliest first; <see cref="StrikeHits.ContactPoint"/> is at contact, <see cref="Aim"/>), the tip's and the grip's.</summary>
        public Vector3[] Path, TipPath, GripPath;

        /// <summary>How far apart the path's points are (seconds as they pass).</summary>
        public float PathSeconds;

        /// <summary>Seconds (as they pass) until the path reaches a point along it (an index, fractional).</summary>
        public float SecondsTo(float along) => Mathf.Max(0f, ToContact + (along - StrikeHits.ContactPoint) * PathSeconds);

        public bool Thrust;
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
        private readonly List<(Collider collider, PartId part, bool shield, float order)> found = new List<(Collider, PartId, bool, float)>();

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
            if (clip == null || clip.ContactFrame < 0 || loco.ShotTime > clip.ContactFrame / clip.Rate + 0.1f || !Blade(clip, out _, out _, out blade))
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
                return false;
            }

            var clip = loco.Shot;
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

        /// <summary>How many points of the blow's path are read (from 0.28 s before its contact to the end of its live moments), which is at contact, how far apart (seconds of the clip).</summary>
        public const int PathPoints = 13, ContactPoint = 7;
        public const float PathStep = 0.04f;

        /// <summary>How far before contact the blow's line is read from (seconds of the clip).</summary>
        public const float ReadOver = 0.06f;

        private readonly Vector3[] readPose = new Vector3[LocoClip.BoneCount];

        /// <summary>A clip's striking point at a time, against its root then (metres at this body's size).</summary>
        private Vector3 StrikeAt(LocoClip clip, float t, float alongBlade = -1f)
        {
            clip.Pose(t, readPose, loco.Leg);
            float along = alongBlade >= 0f ? alongBlade : IsThrust(clip) ? 1f : poser.SweetSpot;
            return readPose[(int)LocoBone.RightHand] + StrikeCombos.Blade(readPose) * (poser.WeaponLength * along);
        }

        /// <summary>The parts of the dummy this blow has struck already (a blade that cuts through goes on to the next), whether it was stopped, and whether its blade has been deflected off the dummy's.</summary>
        private readonly HashSet<PartId> struckParts = new HashSet<PartId>();
        private bool stopped, deflected;

        /// <summary>When each part (and the shield, and the sword) was last touched outside a blow (seconds).</summary>
        private readonly Dictionary<int, float> touched = new Dictionary<int, float>();
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
        public void Step(float dt, LabDummy dummy)
        {
            clock += dt;
            if (HitStop > 0f)
            {
                HitStop = Mathf.Max(0f, HitStop - dt);
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
                stopped = deflected = false;
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
            float shotWas = lastShotTime;
            lastGrip = grip;
            lastTip = tip;
            lastStrike = strike;
            lastShotTime = loco.ShotTime;
            had = true;
            if (!wasHad || dummy == null)
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
            for (int i = 1; i <= samples; i++)
            {
                float s = i / (float)samples;
                Vector3 a = Vector3.Lerp(lastG, grip, s), b = Vector3.Lerp(lastT, tip, s);
                found.Clear();
                // (The dummy's own sword, parrying: blade against blade, two segments' nearest points.)
                float parryAlong = -1f;
                Vector3 parryAt = Vector3.zero;
                if (dummy.Stance == LabDummy.Stances.Sword && dummy.Parry != null && !deflected)
                {
                    dummy.Parry.BladeNow(out var dg, out var dt2);
                    float defRadius = 0.02f * Scale;
                    if (SegmentsNearest(a, b, dg, dt2, out float sa, out float sd, out Vector3 pa, out Vector3 pd) <= radius + defRadius)
                    {
                        parryAlong = sa;
                        parryAt = 0.5f * (pa + pd);
                        lastParryOn = sd;
                    }
                }

                foreach (var collider in Physics.OverlapCapsule(a, b, radius, ~0, QueryTriggerInteraction.Collide))
                {
                    if (dummy.ShieldColliders.Contains(collider))
                    {
                        found.Add((collider, PartId.HandL, true, AlongBlade(collider, a, b) - 0.05f));
                    }
                    else if (dummy.PartOf(collider, out var id) && !(live && struckParts.Contains(id)))
                    {
                        found.Add((collider, id, false, AlongBlade(collider, a, b)));
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
                    || found[0].part == PartId.HandR || found[0].part == PartId.ForearmR);
                if (!live)
                {
                    // Outside a blow: a touch, a push on what it touches.
                    Vector3 velocity = direction / Mathf.Max(1e-4f, dt);
                    if (parryFirst)
                    {
                        Touch(dummy, -2, PartId.HandR, false, true, parryAt, velocity);
                    }
                    else
                    {
                        var ft = found[0];
                        Touch(dummy, ft.shield ? -1 : (int)ft.part, ft.part, ft.shield, false, ft.collider.ClosestPoint(0.5f * (a + b)), velocity);
                    }

                    return;
                }

                if (parryFirst)
                {
                    float metAt = Mathf.Lerp(shotWas, loco.ShotTime, (i - 1) / (float)samples);
                    Strike(clip, dummy, PartId.HandR, true, parryAt, speed, direction, metAt, parryAlong, b - a);
                    return;
                }

                var f = found[0];
                // (The fist that holds the shield is behind it: met with the shield, the shield takes the blow.)
                if (!f.shield && (f.part == PartId.HandL || f.part == PartId.ForearmL))
                {
                    int s0 = found.FindIndex(x => x.shield);
                    if (s0 >= 0)
                    {
                        f = found[s0];
                    }
                }

                // (Where in this step it met it: a stopped blow is put back there.)
                float met = Mathf.Lerp(shotWas, loco.ShotTime, (i - 1) / (float)samples);
                Strike(clip, dummy, f.part, f.shield, f.collider.ClosestPoint(0.5f * (a + b)), speed, direction, met);
                return;
            }
        }

        /// <summary>A touch outside a blow: what the blade touches is pushed by the weapon's weight at its speed.</summary>
        private void Touch(LabDummy dummy, int key, PartId part, bool shield, bool sword, Vector3 point, Vector3 velocity)
        {
            // (Only a weapon moving into it pushes: one resting against it, swaying with its holder, is felt but not knocked.)
            if (velocity.magnitude < TouchSpeed * Mathf.Sqrt(Scale) || (touched.TryGetValue(key, out float last) && clock - last < TouchEvery))
            {
                return;
            }

            touched[key] = clock;
            Touches++;
            if (shield || sword)
            {
                Sounded?.Invoke(shield ? Met.TouchShield : Met.TouchBlade, point, velocity.magnitude, Club, false);
            }

            dummy.Touch(part, shield, sword, point, Vector3.ClampMagnitude(velocity, 8f) * (WeaponMass * Scale));
        }

        private void Strike(LocoClip clip, LabDummy dummy, PartId part, bool shield, Vector3 point, float speed, Vector3 direction, float met,
            float parryAlong = -1f, Vector3 bladeWay = default)
        {
            bool parried = parryAlong >= 0f;
            if (!parried && !shield)
            {
                struckParts.Add(part);
            }

            var r = DamageRules.Resolve(Weapon, With(clip), Scale, speed, direction, Mass, dummy.Mass, part, shield, momentumLeft,
                1e6f, dummy.Poise, dummy.MaxPoise);
            bool glanced = false;
            if (parried)
            {
                dummy.Parry.BladeNow(out var dg, out var dtip);
                LastCross = Vector3.Angle(bladeWay, dtip - dg);
                LastCross = Mathf.Min(LastCross, 180f - LastCross);
                LastParryAt = parryAlong;
                // (A clean parry stops the blow: taken on the strong of the blade, near square. Taken out on the
                // weak of it, or at a slant, the blow glances off and goes on, slowed.)
                glanced = lastParryOn > CleanOnBlade || LastCross < CleanCross;
                if (glanced)
                {
                    deflected = true;
                    Deflects++;
                    // (Turned off the dummy's blade: away from the arm that holds it.)
                    struckParts.Add(PartId.HandR);
                    struckParts.Add(PartId.ForearmR);
                    struckParts.Add(PartId.UpperArmR);
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
            // (A slanted shield turns part of the blow aside; one taken square on takes all of it.)
            if (shield && !parried)
            {
                knock *= 1f - dummy.ShieldGlance;
            }
            dummy.TakeHit(part, shield && !parried, (glanced ? r.Impulse * GlanceLeft : r.Impulse) * knock, r.PartShare, r.PoiseDamage * knock, r.Reaction, point, parried);

            // Hit stop: both stand still a moment, longer for a heavier blow (as the fight has it).
            HitStop = Mathf.Clamp(0.06f * (0.6f + r.Impulse.magnitude / 40f), 0.04f, 0.1f);
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
                poser.KnockHand(-direction.normalized * Mathf.Min(speed * 0.12f, 1.5f));
                what = $"glanced off the sword ({dummy.Parry.CurrentName}, crossed at {LastCross:0} degrees, on its blade {lastParryOn:0.00} from the hilt): on, slowed";
                HitStop = Mathf.Min(HitStop, 0.04f);
            }
            else if ((r.StopsSwing && !(comboing && !shield)) || parried)
            {
                stopped = true;
                // (The blades or the shield meet: the attacker's hand thrown back along the blow, its arm taking it.)
                poser.KnockHand(-direction.normalized * Mathf.Min(speed * 0.25f, 3f));
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
                what = parried ? $"parried by the sword ({dummy.Parry.CurrentName}, crossed at {LastCross:0} degrees, on its blade {lastParryOn:0.00} from the hilt, on the attacker's {parryAlong:0.00})"
                    : shield ? (dummy.ShieldGlance > 0.25f ? "blocked by the shield, turned aside" : "blocked by the shield, square on") : "stopped in the " + Name(part);
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

            Last = $"{clip.Name} at {speed:0.0} m/s: {what} ({r.Reaction.ToString().ToLowerInvariant()}, impulse {r.Impulse.magnitude:0} N·s, hit stop {HitStop:0.00} s{(finishing ? ", the showstopper" : "")})";
            Landed?.Invoke(Last);
        }

        private static string Name(PartId part)
        {
            string n = part.ToString();
            return n.EndsWith("L") ? "left " + n.Substring(0, n.Length - 1).ToLowerInvariant()
                : n.EndsWith("R") ? "right " + n.Substring(0, n.Length - 1).ToLowerInvariant()
                : n.ToLowerInvariant();
        }

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
