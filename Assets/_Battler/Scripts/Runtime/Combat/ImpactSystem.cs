using System;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Combat
{
    /// <summary>One resolved hit, for health, effects, sound and the debug log.</summary>
    public struct HitEvent
    {
        public UnitController attacker;
        public UnitController victim;
        public PartId part;
        public Vector3 point;
        public Vector3 direction;
        public float speed;
        public float damage;
        public float impulse;
        public float stabilityLoss;
        public float intent;
        public bool friendly;
        public float time;
    }

    public static class CombatEvents
    {
        public static event Action<HitEvent> HitLanded;

        public static void Raise(in HitEvent hit) => HitLanded?.Invoke(hit);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => HitLanded = null;
    }

    /// <summary>Global combat rules that don't belong to any one unit.</summary>
    public static class CombatRules
    {
        /// <summary>Allies are always hit physically; this decides whether they also take damage.</summary>
        public static bool FriendlyFireDamage = false;
    }

    /// <summary>
    /// Turns contacts into hits. A hit is any weapon contact with another unit's body part that is
    /// fast enough, whoever the swing was aimed at. PhysX supplies the contact; on top of it this
    /// adds a deliberate, tunable shove, because raw contacts between muscle-driven bodies are too
    /// soft and too random to read well.
    /// </summary>
    public static class ImpactSystem
    {
        /// <summary>
        /// A weapon on <paramref name="attacker"/>'s hand touched <paramref name="victimPart"/>.
        /// Returns the hit, or null for a touch that was too slow (or still cooling down).
        /// </summary>
        public static HitEvent? ResolveWeaponContact(UnitController attacker, Weapon weapon, BodyPart victimPart,
            ContactPoint contact)
        {
            var victim = victimPart.Owner;
            if (victim == null || victim == attacker || weapon.Hand == null)
            {
                return null;
            }

            var definition = weapon.Definition;
            if (!attacker.CanHit(victim, definition.hitCooldown))
            {
                return null;
            }

            Vector3 point = contact.point;
            Vector3 relative = weapon.Hand.CachedPointVelocity(point) - victimPart.CachedPointVelocity(point);
            Vector3 normal = contact.normal;
            if (Vector3.Dot(normal, relative) < 0f)
            {
                normal = -normal;   // make it point from the weapon into the victim
            }

            float speed = Mathf.Abs(Vector3.Dot(relative, normal));
            speed = Mathf.Max(speed, relative.magnitude * 0.5f);
            if (speed < definition.minHitSpeed)
            {
                return null;
            }

            attacker.MarkHit(victim);
            bool friendly = attacker.Team == victim.Team;
            float intent = attacker.Planner.Intent;
            float crowded = attacker.Planner.Active ? attacker.Planner.Plan.crowdedFactor : 1f;

            float damage = definition.damage * (speed / Mathf.Max(0.1f, definition.referenceSpeed))
                           * PartInfo.DamageMultiplier(victimPart.id) * intent * crowded;
            if (friendly && !CombatRules.FriendlyFireDamage)
            {
                damage = 0f;
            }

            float rawImpulse = definition.gameplayMassKg * speed;
            Vector3 direction = (relative.normalized * 0.6f + normal * 0.4f).normalized + Vector3.up * definition.upLift;
            direction.Normalize();
            float shove = rawImpulse * definition.knockback;

            float pelvisShare = Mathf.Clamp01(definition.pelvisShare);
            victimPart.body.AddForceAtPosition(direction * (shove * (1f - pelvisShare)), point, ForceMode.Impulse);
            victim.Rig.Part(PartId.Pelvis).body.AddForce(direction * (shove * pelvisShare), ForceMode.Impulse);

            var hit = new HitEvent
            {
                attacker = attacker,
                victim = victim,
                part = victimPart.id,
                point = point,
                direction = direction,
                speed = speed,
                damage = damage,
                impulse = rawImpulse,
                stabilityLoss = rawImpulse * definition.stabilityScale * Mathf.Lerp(0.5f, 1f, intent),
                intent = intent,
                friendly = friendly,
                time = Time.time,
            };

            victim.ReceiveHit(hit);
            attacker.OnWeaponLanded(hit, victim);
            CombatEvents.Raise(hit);
            return hit;
        }

        /// <summary>A body part was shoved by something with a rigidbody (another unit, a prop).</summary>
        public static void ResolveShove(UnitController unit, BodyPart part, Collision collision)
        {
            float impulse = collision.impulse.magnitude;
            var profile = unit.Profile;
            if (impulse <= profile.shoveThreshold)
            {
                return;
            }

            unit.State.LoseStability((impulse - profile.shoveThreshold) * profile.shoveStabilityPerImpulse);
            if (impulse > profile.shoveThreshold * 3f)
            {
                unit.Muscles.Dip(part.id, 0.5f);
            }
        }
    }
}
