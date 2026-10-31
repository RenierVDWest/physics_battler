using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Combat
{
    /// <summary>How far physics may take a struck unit (there are no hit animations: every reaction is the body recoiling).</summary>
    public enum Reaction
    {
        /// <summary>The struck part (and, less, its neighbours) goes loose and takes the impulse; no interrupt.</summary>
        Recoil,

        /// <summary>A hard blow: recoil, and the unit's own blow is cancelled.</summary>
        Flinch,

        /// <summary>Poise broken: the upper body goes loose for longer, the unit is knocked back and its blow cancelled.</summary>
        Stagger,

        /// <summary>Health gone: the whole body goes ragdoll (getting up comes in milestone 4).</summary>
        Killed,
    }

    /// <summary>What one weapon contact did.</summary>
    public struct HitResult
    {
        public float Damage;
        public float PoiseDamage;

        /// <summary>The whole impulse (N·s, world): part of it goes into the struck part, the rest moves the whole unit.</summary>
        public Vector3 Impulse;

        /// <summary>Share of the impulse the struck part takes; the rest knocks the unit back.</summary>
        public float PartShare;

        public Reaction Reaction;

        /// <summary>
        /// The blow ends here (a body too heavy to cleave through, or a shield); otherwise it carries
        /// on with <see cref="MomentumLeft"/> of its speed, and the struck body is shoved out of its way.
        /// </summary>
        public bool StopsSwing;
        public float MomentumLeft;

        public bool Blocked;
    }

    /// <summary>
    /// The damage rules: a contact's weapon, speed, striking part, body part struck and the two
    /// units' sizes become damage, poise damage, an impulse and a reaction. Pure maths, tested on its
    /// own. Weight and size matter on their own: the same blow moves a light unit much further
    /// than a heavy one.
    /// </summary>
    public static class DamageRules
    {
        /// <summary>Weapon speed (m/s) at which damage is the weapon's own figure (a full swing through contact).</summary>
        public const float ReferenceSpeed = 15f;

        /// <summary>
        /// A swing carries on through a body if it keeps at least this share of its speed: what is
        /// left is 1 - struck mass / (struck mass + attacker mass). So a large unit cleaves through
        /// small and medium ones, a medium through a small one; equal weights stop each other.
        /// </summary>
        public const float CleaveShare = 0.55f;

        /// <summary>Poise lost per N·s of impulse: a full club swing from a large unit breaks a medium one's poise, a sword cut makes it flinch at most.</summary>
        public const float PoiseScale = 0.5f;

        /// <summary>Share of the attacker's mass in the weapon arm (upper arm, forearm, hand).</summary>
        public const float ArmMassShare = 0.058f;

        /// <summary>A blow this share of a unit's poise or more makes it flinch (its own blow is cancelled).</summary>
        public const float FlinchShare = 0.35f;

        /// <summary>The weapon's mass at the wielder's size (mass goes with volume), plus half the arm swinging it.</summary>
        public static float EffectiveMass(WeaponDefinition weapon, float weaponScale, float attackerMass)
        {
            float weaponMass = (weapon != null ? weapon.massKg : 1f) * weaponScale * weaponScale * weaponScale;
            return weaponMass + 0.5f * ArmMassShare * attackerMass;
        }

        /// <summary>How much of the momentum goes into the body, by what strikes: a club's weight lands, an edge or point cuts in.</summary>
        public static float Transfer(StrikeWith with)
        {
            switch (with)
            {
                case StrikeWith.Head: return 0.9f;
                case StrikeWith.Tip: return 0.45f;
                default: return 0.55f;
            }
        }

        /// <summary>How much a blow to this part hurts, against the chest.</summary>
        public static float PartMultiplier(PartId part)
        {
            switch (part)
            {
                case PartId.Head: return 1.6f;
                case PartId.Chest: return 1f;
                case PartId.Spine: return 0.95f;
                case PartId.Pelvis: return 0.85f;
                case PartId.UpperArmL:
                case PartId.UpperArmR: return 0.6f;
                case PartId.ForearmL:
                case PartId.ForearmR: return 0.5f;
                case PartId.HandL:
                case PartId.HandR: return 0.4f;
                case PartId.ThighL:
                case PartId.ThighR: return 0.7f;
                case PartId.ShinL:
                case PartId.ShinR: return 0.6f;
                default: return 0.4f;
            }
        }

        /// <summary>
        /// Share of a blow's impulse the struck part takes itself (it recoils), the rest moving the
        /// whole unit (knock-back). Limbs swing away and take most of it; the legs carry the body and
        /// pass it on.
        /// </summary>
        public static float PartShare(PartId part)
        {
            switch (part)
            {
                case PartId.Head: return 0.6f;
                case PartId.Chest:
                case PartId.Spine: return 0.45f;
                case PartId.Pelvis: return 0.2f;
                case PartId.UpperArmL:
                case PartId.UpperArmR:
                case PartId.ForearmL:
                case PartId.ForearmR:
                case PartId.HandL:
                case PartId.HandR: return 0.8f;
                default: return 0.15f;   // thighs, shins, feet: kinematic while standing
            }
        }

        /// <summary>
        /// Resolves one contact. <paramref name="momentumFactor"/> is what is left of the swing after
        /// bowling through earlier bodies (1 for the first).
        /// </summary>
        public static HitResult Resolve(WeaponDefinition weapon, StrikeWith with, float weaponScale, float speed, Vector3 direction,
            float attackerMass, float struckMass, PartId part, bool shield, float momentumFactor,
            float health, float poise, float maxPoise)
        {
            var r = new HitResult();
            float mass = EffectiveMass(weapon, weaponScale, attackerMass);
            float momentum = mass * speed * Mathf.Clamp01(momentumFactor);
            Vector3 dir = direction.sqrMagnitude > 1e-8f ? direction.normalized : Vector3.forward;
            // A little lift, so struck bodies pop rather than slide.
            dir = (dir + Vector3.up * 0.15f).normalized;

            float speedFactor = speed / ReferenceSpeed;
            float baseDamage = weapon != null ? weapon.damage : 10f;
            if (shield)
            {
                r.Blocked = true;
                r.Damage = 0f;
                r.PoiseDamage = 0.6f * PoiseScale * momentum;   // a block still costs poise
                r.Impulse = dir * (0.8f * momentum);
                r.PartShare = 0.7f;               // the shield arm takes most of it
                r.StopsSwing = true;
            }
            else
            {
                r.Damage = baseDamage * speedFactor * Mathf.Clamp01(momentumFactor) * PartMultiplier(part);
                r.Impulse = dir * (momentum * Transfer(with));
                r.PoiseDamage = PoiseScale * r.Impulse.magnitude;
                r.PartShare = PartShare(part);
                // A swing heavy enough for the body it meets carries on through it (the body is shoved
                // out of the arc, never passed through); otherwise it stops where it met it.
                float keep = 1f - struckMass / Mathf.Max(1f, struckMass + attackerMass);
                r.StopsSwing = keep < CleaveShare;
                r.MomentumLeft = r.StopsSwing ? 0f : keep * Mathf.Clamp01(momentumFactor);
            }

            if (health - r.Damage <= 0f)
            {
                r.Reaction = Reaction.Killed;
            }
            else if (poise - r.PoiseDamage <= 0f)
            {
                r.Reaction = Reaction.Stagger;
            }
            else if (r.PoiseDamage >= FlinchShare * maxPoise)
            {
                r.Reaction = Reaction.Flinch;
            }
            else
            {
                r.Reaction = Reaction.Recoil;
            }

            return r;
        }

        /// <summary>The speed (m/s) a knock-back gives the whole unit: what the part didn't take, over the unit's mass.</summary>
        public static float KnockbackSpeed(HitResult hit, float unitMass)
        {
            return hit.Impulse.magnitude * (1f - hit.PartShare) / Mathf.Max(1f, unitMass);
        }

        // ---- Milestone 5: severing ----------------------------------------------------------------

        /// <summary>
        /// The damage a blow must do (at medium size; times the struck unit's size: bigger bones are
        /// thicker) to cut through the joint above a part. A wrist goes easiest, a hip hardest.
        /// </summary>
        public static float SeverDamage(PartId below)
        {
            switch (below)
            {
                case PartId.HandL:
                case PartId.HandR:
                case PartId.FootL:
                case PartId.FootR: return 3.5f;
                case PartId.ForearmL:
                case PartId.ForearmR: return 4.5f;
                case PartId.ShinL:
                case PartId.ShinR: return 6f;
                case PartId.UpperArmL:
                case PartId.UpperArmR: return 6.5f;
                case PartId.Head: return 9f;
                case PartId.ThighL:
                case PartId.ThighR: return 8f;
                default: return float.MaxValue;
            }
        }

        /// <summary>The wound of losing the limb below a joint, as a share of the unit's health.</summary>
        public static float SeverLoss(PartId below)
        {
            switch (below)
            {
                case PartId.HandL:
                case PartId.HandR:
                case PartId.FootL:
                case PartId.FootR: return 0.06f;
                case PartId.ForearmL:
                case PartId.ForearmR:
                case PartId.ShinL:
                case PartId.ShinR: return 0.1f;
                default: return 0.15f;   // a whole arm or leg
            }
        }

        /// <summary>How fast the wound bleeds, as a share of the unit's health a second: a lost leg kills in about 20 seconds, a hand in about a minute.</summary>
        public static float BleedRate(PartId below)
        {
            switch (below)
            {
                case PartId.HandL:
                case PartId.HandR:
                case PartId.FootL:
                case PartId.FootR: return 0.015f;
                case PartId.ForearmL:
                case PartId.ForearmR:
                case PartId.ShinL:
                case PartId.ShinR: return 0.03f;
                default: return 0.045f;
            }
        }

        public static bool IsTorso(PartId part) => part == PartId.Chest || part == PartId.Spine || part == PartId.Pelvis;
    }
}
