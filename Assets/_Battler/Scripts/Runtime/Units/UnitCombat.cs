using System;
using System.Collections.Generic;
using PhysicsBattler.Authority;
using PhysicsBattler.Combat;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Units
{
    /// <summary>One weapon contact: who struck whom, where, how hard, and what it did.</summary>
    public sealed class HitReport
    {
        public UnitController Attacker;
        public UnitController Struck;
        public PartId Part;
        public bool Shield;
        public MoveState Move;
        public Vector3 Point;
        public float Speed;
        public HitResult Result;

        /// <summary>The part the blow was aimed at; the target but another part; an ally of the attacker; or someone else in the way.</summary>
        public string Kind;
    }

    /// <summary>
    /// The combat half of a unit (milestone 3): its health and poise, the weapon sweep that finds
    /// what a blow strikes, and the physical reactions to being struck. There are no hit
    /// animations: a struck part goes loose and takes the impulse, its neighbours less, and the
    /// pins ramp back; what the part doesn't take moves the whole unit (the feet step after it).
    /// </summary>
    public partial class UnitController
    {
        public static event Action<HitReport> Hit;
        public static event Action<UnitController, HitReport> Died;

        public float Health { get; private set; }
        public float Poise { get; private set; }
        public bool IsAlive { get; private set; } = true;

        /// <summary>Off, the unit's blows pass through bodies (the aim tests measure the whole blow).</summary>
        public bool DealsHits { get; set; } = true;

        /// <summary>The unit's own sliding from a knock-back (m/s, world, horizontal).</summary>
        public Vector3 Knockback { get; private set; }

        /// <summary>Seconds of hit stop left: the pose and the blow stand still.</summary>
        public float HitStop { get; private set; }

        /// <summary>How far the shield is raised across an incoming blow (0 to 1).</summary>
        public float BlockBlend { get; private set; }

        public Reaction LastReaction { get; private set; } = Reaction.Recoil;

        // Recovering regions: physics owns them after a hit, the pin ramps back up.
        private readonly float[] recoverFrom = new float[5];
        private readonly float[] recoverTime = new float[5];
        private readonly float[] recoverLeft = new float[5];

        private readonly HashSet<UnitController> struckThisBlow = new HashSet<UnitController>();
        private float momentumLeft = 1f;
        private Vector3 lastGrip, lastTip, lastStrike;
        private bool hasLastSweep;
        private Vector3 pendingRebound;
        private System.Random combatRandom;
        private MoveState cutShort;

        /// <summary>Seconds a cut-short blow takes to leave the pose.</summary>
        private const float CutShortFade = 0.3f;

        /// <summary>The fastest a struck part is sent on its own (m/s, medium size).</summary>
        private const float MaxPartSpeed = 5f;

        /// <summary>The fastest a knock-back slides the unit (m/s, medium size).</summary>
        private const float MaxKnockback = 2.5f;

        /// <summary>Speeds go with the square root of size (a bigger body's parts swing slower for the same look).</summary>
        private float SizeSpeed() => Mathf.Sqrt(Mathf.Max(0.3f, Body.SizeFactor));

        // An incoming blow the shield is raised against.
        private MoveState incoming;
        private float blockFrom, blockUntil;

        private void InitializeCombat(int seed)
        {
            Health = profile.maxHealth;
            Poise = profile.maxPoise;
            IsAlive = true;
            combatRandom = new System.Random(seed + 17);
        }

        // ---- Each step --------------------------------------------------------------------------

        /// <summary>Hit stop: the time the pose runs on (0 while stopped).</summary>
        private float CombatTime(float dt)
        {
            if (HitStop > 0f)
            {
                HitStop = Mathf.Max(0f, HitStop - dt);
                return 0f;
            }

            return dt;
        }

        /// <summary>Knock-back slides the unit (the feet step after it); poise comes back; the shield goes up or down.</summary>
        private void CombatBeforePose(float dt)
        {
            if (Knockback.sqrMagnitude > 1e-6f)
            {
                transform.position += Knockback * dt;
                Knockback *= Mathf.Exp(-dt / KnockbackDecay);
            }
            else
            {
                Knockback = Vector3.zero;
            }

            Poise = Mathf.Min(profile.maxPoise, Poise + profile.poiseRegen * dt);
            for (int r = 0; r < 5; r++)
            {
                recoverLeft[r] = Mathf.Max(0f, recoverLeft[r] - dt);
            }

            // The shield: up from the reaction time, down after the blow is past.
            float target = incoming != null && LocalTime >= blockFrom && LocalTime <= blockUntil ? 1f : 0f;
            BlockBlend = Mathf.MoveTowards(BlockBlend, target, dt / 0.15f);
            if (incoming != null && LocalTime > blockUntil && BlockBlend <= 0f)
            {
                incoming = null;
            }

            if (incoming != null)
            {
                // In front of the point the blow is aimed at, facing back along it.
                Vector3 toward = -(incoming.Frame * Vector3.forward);
                Pose.BlockCenter = incoming.Aim + toward * (0.3f * Body.SizeFactor);
                Pose.BlockNormal = toward;
            }

            Pose.BlockBlend = BlockBlend;
        }

        /// <summary>Seconds a knock-back takes to die away (it slows exponentially).</summary>
        private const float KnockbackDecay = 0.18f;

        /// <summary>How firmly each region is held this step: the profile's pins, a hit's ramp, the blow's weapon arm.</summary>
        private void SetAuthority(MoveState move)
        {
            if (!IsAlive)
            {
                for (int r = 0; r < 5; r++)
                {
                    Authority.Set((BodyRegion)r, AuthorityLevel.Ragdoll, 0f);
                }

                return;
            }

            Authority.Set(BodyRegion.Legs, AuthorityLevel.Kinematic, 1f);
            float blend = move != null ? move.Blend : 0f;
            float armR = Mathf.Lerp(profile.armPin, move != null ? move.Move.weaponArmPin : profile.armPin, blend);
            float armL = BlockBlend > 0f ? Mathf.Lerp(profile.armPin, 0.95f, BlockBlend) : profile.armPin;
            SetRegion(BodyRegion.Torso, profile.torsoPin, false);
            SetRegion(BodyRegion.Head, profile.torsoPin, false);
            SetRegion(BodyRegion.ArmL, armL, false);
            bool kinematicArm = move != null && profile.weaponArmKinematicInBlows && blend > 0.01f;
            SetRegion(BodyRegion.ArmR, armR, kinematicArm);
        }

        private void SetRegion(BodyRegion region, float pin, bool kinematic)
        {
            int r = (int)region;
            if (recoverLeft[r] > 0f)
            {
                // Recovering from a hit: physics has it, the pin eases back up.
                float t = 1f - recoverLeft[r] / Mathf.Max(1e-3f, recoverTime[r]);
                float ease = t * t * (3f - 2f * t);
                Authority.Set(region, AuthorityLevel.Pinned, Mathf.Lerp(recoverFrom[r] * pin, pin, ease));
                return;
            }

            Authority.Set(region, kinematic ? AuthorityLevel.Kinematic : AuthorityLevel.Pinned, pin);
        }

        /// <summary>After the authority layer: a rebound held over from a stopped swing goes into the (now dynamic) weapon hand.</summary>
        private void CombatAfterAuthority()
        {
            if (pendingRebound.sqrMagnitude > 0f)
            {
                var hand = Rig.Part(PartId.HandR).body;
                if (!hand.isKinematic)
                {
                    hand.AddForce(pendingRebound, ForceMode.Impulse);
                }

                pendingRebound = Vector3.zero;
            }
        }

        // ---- The sweep ---------------------------------------------------------------------------

        private void BeginSweep()
        {
            struckThisBlow.Clear();
            momentumLeft = 1f;
            hasLastSweep = false;
        }

        /// <summary>
        /// The weapon as PhysX has it now, swept from where it was last step: everything it passes
        /// through during the blow's active window is struck (any unit: friendly fire counts), in
        /// the order the blade reaches them. Each unit once per blow. A heavy body or a shield stops
        /// the swing; a light one is bowled through and the rest of the swing goes on with less.
        /// </summary>
        private void Sweep(MoveState move, float dt)
        {
            var weapon = move.Weapon;
            if (MainWeapon == null || weapon == null || dt <= 0f)
            {
                return;
            }

            float from = weapon.kind == WeaponKind.Club ? 0.35f : 0.1f;
            Vector3 grip = MainWeapon.PhysicalPoint(weapon.tip * from);
            Vector3 tip = MainWeapon.PhysicalPoint(weapon.tip);
            Vector3 strike = MainWeapon.PhysicalPoint(weapon.StrikePoint(move.Move.strikeWith));
            bool had = hasLastSweep;
            Vector3 lastG = lastGrip, lastT = lastTip, lastS = lastStrike;
            lastGrip = grip;
            lastTip = tip;
            lastStrike = strike;
            hasLastSweep = true;
            if (!had || !DealsHits || !move.Active || momentumLeft <= 0f)
            {
                return;
            }

            float speed = Vector3.Distance(strike, lastS) / dt;
            if (speed < weapon.minHitSpeed)
            {
                return;
            }

            Vector3 direction = strike - lastS;
            float radius = Mathf.Max(0.01f, weapon.sweepRadius * Loadout.Scale);
            float travel = Mathf.Max(Vector3.Distance(grip, lastG), Vector3.Distance(tip, lastT));
            int samples = Mathf.Clamp(Mathf.CeilToInt(travel / (1.5f * radius)), 1, 10);
            var found = new List<(UnitController unit, BodyPart part, Weapon shield, Collider collider, float order)>();
            for (int i = 1; i <= samples; i++)
            {
                float s = i / (float)samples;
                Vector3 a = Vector3.Lerp(lastG, grip, s);
                Vector3 b = Vector3.Lerp(lastT, tip, s);
                Vector3 mid = 0.5f * (a + b);
                found.Clear();
                foreach (var collider in Physics.OverlapCapsule(a, b, radius, ~0, QueryTriggerInteraction.Collide))
                {
                    var shield = collider.GetComponentInParent<Weapon>();
                    if (shield != null && shield.definition != null && shield.definition.IsShield && shield.Owner != null && shield.Owner != this)
                    {
                        found.Add((shield.Owner, null, shield, collider, AlongBlade(collider, a, b) - 0.05f));
                        continue;
                    }

                    if (shield != null)
                    {
                        continue;   // a weapon, not a body (clashes come in milestone 7)
                    }

                    var part = collider.GetComponentInParent<BodyPart>();
                    if (part == null || part.Owner == null || part.Owner == this || part.partCollider != collider)
                    {
                        continue;
                    }

                    found.Add((part.Owner, part, null, collider, AlongBlade(collider, a, b)));
                }

                // In this stretch, nearest the hand first: the blade turns about the hand, so a body
                // nearer the hand is in the way of everything further out (a shield before the arm behind it).
                found.Sort((x, y) => x.order.CompareTo(y.order));
                foreach (var f in found)
                {
                    if (!f.unit.IsAlive || struckThisBlow.Contains(f.unit))
                    {
                        continue;
                    }

                    struckThisBlow.Add(f.unit);
                    var report = Strike(move, f.unit, f.part, f.shield, f.collider.ClosestPoint(mid), speed, direction);
                    if (report.Result.StopsSwing)
                    {
                        momentumLeft = 0f;
                        StopSwing(move, report);
                        return;
                    }

                    momentumLeft = report.Result.MomentumLeft;
                }
            }
        }

        /// <summary>How far out along the blade (0 at a, the hand's end, to 1 at b, the tip) a collider touches it.</summary>
        private static float AlongBlade(Collider collider, Vector3 a, Vector3 b)
        {
            Vector3 ab = b - a;
            float length = Mathf.Max(1e-6f, ab.sqrMagnitude);
            float T(Vector3 p) => Mathf.Clamp01(Vector3.Dot(p - a, ab) / length);
            Vector3 near = collider.ClosestPoint(0.5f * (a + b));
            near = collider.ClosestPoint(a + ab * T(near));
            return T(near);
        }

        private HitReport Strike(MoveState move, UnitController struck, BodyPart part, Weapon shield, Vector3 point, float speed, Vector3 direction)
        {
            var partId = shield != null ? PartId.HandL : part.id;
            var result = DamageRules.Resolve(move.Weapon, move.Move.strikeWith, Loadout.Scale, speed, direction,
                Body.Mass, struck.Body.Mass, partId, shield != null, momentumLeft, struck.Health, struck.Poise, struck.profile.maxPoise);
            string kind = shield != null ? "blocked by the shield"
                : ReferenceEquals(struck, move.Target) && partId == AimPartInfo.BodyPartOf(move.Part) ? "the part aimed at"
                : ReferenceEquals(struck, move.Target) ? "the target, another part"
                : struck.Team == Team ? "an ally (friendly fire)" : "someone in the way";
            var report = new HitReport
            {
                Attacker = this, Struck = struck, Part = partId, Shield = shield != null, Move = move, Point = point, Speed = speed,
                Result = result, Kind = kind,
            };
            struck.TakeHit(report);

            // Hit stop: the attacker and the struck stand still a moment, longer for a heavier blow.
            float stop = Mathf.Clamp(move.Move.hitStop * (0.6f + result.Impulse.magnitude / 40f), 0.04f, 0.1f);
            HitStop = Mathf.Max(HitStop, stop);
            struck.HitStop = Mathf.Max(struck.HitStop, stop);
            Hit?.Invoke(report);
            return report;
        }

        /// <summary>The swing met something it can't go through: the blow ends and the weapon arm recoils.</summary>
        private void StopSwing(MoveState move, HitReport report)
        {
            CancelMove(move, "stopped by " + report.Struck.name + (report.Shield ? "'s shield" : ""));
            StartRecovery(BodyRegion.ArmR, 0.05f, 0.45f);
            pendingRebound = -report.Result.Impulse * 0.35f;
        }

        // ---- Being struck ------------------------------------------------------------------------

        private void TakeHit(HitReport hit)
        {
            var r = hit.Result;
            Health = Mathf.Max(0f, Health - r.Damage);
            Poise -= r.PoiseDamage;
            LastReaction = r.Reaction;

            // The struck part goes loose and takes its share; the regions joined to it follow, less.
            var region = AuthorityController.RegionOf(hit.Part);
            float hold = r.Reaction == Reaction.Stagger ? 0.9f : r.Reaction == Reaction.Flinch ? 0.55f : 0.35f;
            StartRecovery(region, 0.03f, hold);
            foreach (var neighbour in Neighbours(region))
            {
                StartRecovery(neighbour, r.Reaction == Reaction.Stagger ? 0.15f : 0.4f, 0.7f * hold);
            }

            var body = Rig.Part(hit.Part).body;
            float partShare = r.PartShare;
            bool partTakesIt = region != BodyRegion.Legs && body != null;
            if (partTakesIt && r.Impulse.sqrMagnitude > 1e-8f)
            {
                // A part is joined to the rest of the body: it can't fly off on its own. Past a
                // few metres a second the rest of the blow moves the whole unit.
                float most = body.mass * MaxPartSpeed * SizeSpeed();
                partShare = Mathf.Min(partShare, most / r.Impulse.magnitude);
                r.PartShare = partShare;
                hit.Result = r;

                // The region was just made dynamic for the next step; if it is kinematic this step
                // (a weapon arm mid-blow), the impulse is applied as a velocity change instead.
                if (body.isKinematic)
                {
                    body.isKinematic = false;
                }

                body.AddForceAtPosition(r.Impulse * partShare, hit.Point, ForceMode.Impulse);
            }
            else
            {
                partShare = 0f;   // the legs carry the body: the whole blow moves the unit
            }

            Vector3 push = r.Impulse * (1f - partShare) / Mathf.Max(1f, Body.Mass);
            push.y = 0f;
            if (r.Reaction == Reaction.Stagger)
            {
                push *= 1.5f;
            }

            // Being knocked over or launched comes in milestone 4: until then the feet keep up.
            Knockback = Vector3.ClampMagnitude(Knockback + push, MaxKnockback * SizeSpeed());

            if (r.Reaction == Reaction.Killed)
            {
                Die(hit);
                return;
            }

            if (r.Reaction == Reaction.Stagger)
            {
                Poise = profile.maxPoise;   // back on its feet, poise restored
            }

            if ((r.Reaction == Reaction.Stagger || r.Reaction == Reaction.Flinch) && CurrentMove != null)
            {
                CancelMove(CurrentMove, r.Reaction == Reaction.Stagger ? "staggered" : "flinched");
            }
        }

        private void StartRecovery(BodyRegion region, float fromShare, float seconds)
        {
            if (region == BodyRegion.Legs)
            {
                return;
            }

            int r = (int)region;
            // A new hit while still recovering keeps the looser of the two.
            if (recoverLeft[r] > 0f)
            {
                float t = 1f - recoverLeft[r] / Mathf.Max(1e-3f, recoverTime[r]);
                fromShare = Mathf.Min(fromShare, Mathf.Lerp(recoverFrom[r], 1f, t));
            }

            recoverFrom[r] = fromShare;
            recoverTime[r] = seconds;
            recoverLeft[r] = seconds;
        }

        private static IEnumerable<BodyRegion> Neighbours(BodyRegion region)
        {
            switch (region)
            {
                case BodyRegion.Torso:
                    yield return BodyRegion.Head;
                    yield return BodyRegion.ArmL;
                    yield return BodyRegion.ArmR;
                    break;
                case BodyRegion.Head:
                case BodyRegion.ArmL:
                case BodyRegion.ArmR:
                case BodyRegion.Legs:
                    yield return BodyRegion.Torso;
                    break;
            }
        }

        private void Die(HitReport hit)
        {
            IsAlive = false;
            Health = 0f;
            if (CurrentMove != null)
            {
                CancelMove(CurrentMove, "killed");
            }

            incoming = null;
            BlockBlend = 0f;
            Pose.BlockBlend = 0f;
            Knockback = Vector3.zero;
            // Every part to physics, with the blow's whole impulse in the part struck.
            SetAuthority(null);
            foreach (var part in Rig.Parts)
            {
                if (part.body != null)
                {
                    part.body.isKinematic = false;
                }
            }

            // The struck part already took its share (TakeHit); what it didn't take moves the whole
            // body, spread by mass, so it falls away from the blow rather than flying apart.
            Vector3 rest = hit.Result.Impulse * (1f - hit.Result.PartShare);
            float mass = Mathf.Max(1f, Body.Mass);
            Vector3 velocity = Vector3.ClampMagnitude(rest / mass, 1.5f * MaxKnockback * SizeSpeed());
            foreach (var part in Rig.Parts)
            {
                if (part.body != null)
                {
                    part.body.AddForce(velocity, ForceMode.VelocityChange);
                }
            }

            Died?.Invoke(this, hit);
        }

        /// <summary>Ends a blow early (stopped, flinched, staggered, killed): back to the guard, the feet settle by themselves.</summary>
        private void CancelMove(MoveState move, string why)
        {
            if (CurrentMove != move)
            {
                return;
            }

            move.Cancelled = why;
            CurrentMove = null;
            Stance.Hold = false;
            // The pose lets go of the blow over a moment from where it stopped, rather than snapping
            // back to guard; feet out in the lunge step back home.
            cutShort = move;
            if (move.SteppedOut && !move.SteppedBack)
            {
                move.SteppedBack = true;
                int lead = Stance.Lead;
                Stance.StepTo(lead, Stance.HomeWorld(lead, transform.position, facingYaw), facingYaw + Stance.HomeYawLocal(lead), 0.28f);
            }

            if (move.RearOut && !move.RearBack)
            {
                move.RearBack = true;
                int rear = 1 - Stance.Lead;
                Stance.StepTo(rear, Stance.HomeWorld(rear, transform.position, facingYaw), facingYaw + Stance.HomeYawLocal(rear), 0.32f);
            }

            MoveEnded?.Invoke(this, move);
        }

        /// <summary>Drops a fading blow at once (the unit was moved).</summary>
        private void DropCutShort()
        {
            if (cutShort != null && Pose.Move == cutShort)
            {
                Pose.Move = null;
            }

            cutShort = null;
        }

        /// <summary>A cut-short blow fading out of the pose (gone once a new blow starts).</summary>
        private void FadeCutShort(float dt)
        {
            if (cutShort == null)
            {
                return;
            }

            if (Pose.Move != cutShort)
            {
                cutShort = null;
                return;
            }

            cutShort.Fade -= dt / CutShortFade;
            if (cutShort.Fade <= 0f)
            {
                Pose.Move = null;
                cutShort = null;
            }
        }

        // ---- Shield blocks -----------------------------------------------------------------------

        /// <summary>A blow is coming at this unit: with a shield, it may raise it across the blow after its reaction time.</summary>
        private void OnIncoming(UnitController attacker, MoveState move)
        {
            if (!IsAlive || Loadout == null || !Loadout.HasShield || IsAttacking)
            {
                return;
            }

            if (combatRandom.NextDouble() >= Style.blockChance)
            {
                return;
            }

            incoming = move;
            blockFrom = LocalTime + Style.reactionTime;
            blockUntil = LocalTime + move.Duration * Mathf.Min(1f, move.Move.activeWindow.y + 0.1f);
        }
    }
}
