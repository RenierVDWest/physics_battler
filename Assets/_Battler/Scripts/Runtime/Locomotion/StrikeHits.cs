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
        private bool had, struck;
        private Vector3 lastGrip, lastTip, lastStrike;
        private float momentumLeft = 1f, lastShotTime;
        private readonly List<(Collider collider, PartId part, bool shield, float order)> found = new List<(Collider, PartId, bool, float)>();

        public StrikeHits(Locomotor loco, LocoBody poser, AnimRig anim)
        {
            this.loco = loco;
            this.poser = poser;
            this.anim = anim;
        }

        private static bool IsThrust(LocoClip clip) => clip.Kind != null && clip.Kind.StartsWith("thrust");

        private StrikeWith With(LocoClip clip) =>
            IsThrust(clip) ? StrikeWith.Tip : Weapon != null && Weapon.kind == WeaponKind.Club ? StrikeWith.Head : StrikeWith.Edge;

        /// <summary>The weapon now: the swept stretch (from near the hand to the tip) and the striking point.</summary>
        private bool Blade(LocoClip clip, out Vector3 grip, out Vector3 tip, out Vector3 strike)
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

        /// <summary>One step, after the striker is posed (and while the hit stop lasts, only its clock).</summary>
        public void Step(float dt, LabDummy dummy)
        {
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
            if (clip == null || clip.ContactFrame < 0)
            {
                blow = null;
                had = false;
                return;
            }

            if (clip != blow)
            {
                blow = clip;
                had = false;
                struck = false;
                momentumLeft = 1f;
            }

            if (!Blade(clip, out Vector3 grip, out Vector3 tip, out Vector3 strike))
            {
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
            float contact = clip.ContactFrame / clip.Rate, t = loco.ShotTime;
            bool live = t >= contact - LiveBefore && t <= contact + LiveAfter && loco.ShotSpeed > 0f;
            if (!wasHad || !live || struck || dummy == null)
            {
                return;
            }

            // (Capped: a jump in one step is not a blow's speed.)
            float speed = Mathf.Min(Vector3.Distance(strike, lastS) / dt, 16f);
            if (speed < (Weapon != null ? Weapon.minHitSpeed : 2f))
            {
                return;
            }

            Vector3 direction = strike - lastS;
            float radius = Mathf.Max(0.01f, (Weapon != null ? Weapon.sweepRadius : 0.035f) * Scale);
            float travel = Mathf.Max(Vector3.Distance(grip, lastG), Vector3.Distance(tip, lastT));
            int samples = Mathf.Clamp(Mathf.CeilToInt(travel / (1.5f * radius)), 1, 10);
            for (int i = 1; i <= samples; i++)
            {
                float s = i / (float)samples;
                Vector3 a = Vector3.Lerp(lastG, grip, s), b = Vector3.Lerp(lastT, tip, s);
                found.Clear();
                foreach (var collider in Physics.OverlapCapsule(a, b, radius, ~0, QueryTriggerInteraction.Collide))
                {
                    if (dummy.ShieldColliders.Contains(collider))
                    {
                        found.Add((collider, PartId.HandL, true, AlongBlade(collider, a, b) - 0.05f));
                    }
                    else if (dummy.PartOf(collider, out var id))
                    {
                        found.Add((collider, id, false, AlongBlade(collider, a, b)));
                    }
                }

                if (found.Count == 0)
                {
                    continue;
                }

                // (Nearest the hand first: the blade turns about the hand, so what is nearer it is in the way.)
                found.Sort((x, y) => x.order.CompareTo(y.order));
                var f = found[0];
                // (Where in this step it met it: a stopped blow is put back there.)
                float met = Mathf.Lerp(shotWas, loco.ShotTime, (i - 1) / (float)samples);
                Strike(clip, dummy, f.part, f.shield, f.collider.ClosestPoint(0.5f * (a + b)), speed, direction, met);
                return;
            }
        }

        private void Strike(LocoClip clip, LabDummy dummy, PartId part, bool shield, Vector3 point, float speed, Vector3 direction, float met)
        {
            struck = true;
            var r = DamageRules.Resolve(Weapon, With(clip), Scale, speed, direction, Mass, dummy.Mass, part, shield, momentumLeft,
                1e6f, dummy.Poise, dummy.MaxPoise);
            dummy.TakeHit(part, shield, r.Impulse, r.PartShare, r.PoiseDamage, r.Reaction);
            // Hit stop: both stand still a moment, longer for a heavier blow (as the fight has it).
            HitStop = Mathf.Clamp(0.06f * (0.6f + r.Impulse.magnitude / 40f), 0.04f, 0.1f);
            string what;
            if (r.StopsSwing)
            {
                // Back to where the weapon met it, shown through the hit stop; then it backs out a little.
                loco.WindBack(met);
                poser.Pose(1e-4f);
                loco.CutShort(RewindMost, ReboundSpeed);
                what = shield ? "blocked by the shield" : "stopped in the " + Name(part);
                if (shield)
                {
                    Blocks++;
                }
                else
                {
                    Hits++;
                }
            }
            else
            {
                momentumLeft = r.MomentumLeft;
                loco.ShotSpeed = Mathf.Max(0.3f, r.MomentumLeft);
                what = "cut through the " + Name(part);
                Hits++;
                Cleaves++;
            }

            Last = $"{clip.Name} at {speed:0.0} m/s: {what} ({r.Reaction.ToString().ToLowerInvariant()}, impulse {r.Impulse.magnitude:0} N·s, hit stop {HitStop:0.00} s)";
            Landed?.Invoke(Last);
        }

        private static string Name(PartId part)
        {
            string n = part.ToString();
            return n.EndsWith("L") ? "left " + n.Substring(0, n.Length - 1).ToLowerInvariant()
                : n.EndsWith("R") ? "right " + n.Substring(0, n.Length - 1).ToLowerInvariant()
                : n.ToLowerInvariant();
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
