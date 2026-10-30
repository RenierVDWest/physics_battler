using UnityEngine;

namespace PhysicsBattler.Combat
{
    /// <summary>
    /// How a weapon hits. It has two masses on purpose: PhysX simulates the light physics mass, so
    /// the arm's joint chain stays stable, while the impact system uses the heavy gameplay mass to
    /// decide how hard a hit lands.
    /// </summary>
    [CreateAssetMenu(menuName = "Physics Battler/Weapon Definition", fileName = "WeaponDefinition")]
    public class WeaponDefinition : ScriptableObject
    {
        [Header("Mass")]
        [Tooltip("What PhysX simulates, added to the hand's rigidbody.")]
        public float physicsMassKg = 6f;
        [Tooltip("What hits feel like: drives knockback and stability loss.")]
        public float gameplayMassKg = 40f;

        [Header("Hits")]
        public float damage = 30f;
        [Tooltip("Impact speed (m/s) at which a hit does its full damage. Damage scales linearly with speed.")]
        public float referenceSpeed = 9f;
        [Tooltip("Slower contacts are touches, not hits.")]
        public float minHitSpeed = 2.5f;
        [Tooltip("Scales the extra shove added on top of the physical contact.")]
        public float knockback = 0.45f;
        [Tooltip("Stability lost per N s of gameplay impulse.")]
        public float stabilityScale = 0.15f;
        [Tooltip("Upward share of the shove's direction, so knockbacks read from a high camera.")]
        public float upLift = 0.18f;
        [Tooltip("Share of the shove applied to the victim's pelvis rather than the struck part.")]
        [Range(0f, 1f)] public float pelvisShare = 0.4f;
        [Tooltip("The same weapon can hit the same unit once per this many seconds.")]
        public float hitCooldown = 0.3f;

        public static WeaponDefinition CreateDefault()
        {
            var definition = CreateInstance<WeaponDefinition>();
            definition.name = "WeaponDefinition (default)";
            return definition;
        }
    }
}
