using UnityEngine;

namespace PhysicsBattler.Combat
{
    public enum WeaponKind
    {
        Sword,
        Club,
        Shield,
    }

    /// <summary>Which part of the weapon a move strikes with.</summary>
    public enum StrikeWith
    {
        /// <summary>The middle of a blade's edge: cuts.</summary>
        Edge,

        /// <summary>The point: thrusts.</summary>
        Tip,

        /// <summary>The heavy end of a club.</summary>
        Head,
    }

    /// <summary>
    /// A weapon: its shape (from weapons.json, built for the medium body; units scale it with their
    /// size), how it is held in guard, and the moves it can make. Setup creates one per weapon and
    /// never overwrites your changes.
    ///
    /// Weapon axes (Unity): the grip, where the hand's weapon slot sits, is at the origin. A sword or
    /// club points along +Z; a sword's edges face +Y and -Y. A shield's face looks along +Z.
    /// </summary>
    [CreateAssetMenu(menuName = "Physics Battler/Weapon", fileName = "WD_")]
    public class WeaponDefinition : ScriptableObject
    {
        public WeaponKind kind;

        [Tooltip("The prefab with the weapon's mesh and colliders (built by setup).")]
        public GameObject prefab;

        [Header("Shape (medium size, metres, weapon axes)")]
        public float length = 0.9f;
        public float massKg = 1.3f;
        [Tooltip("The point, for thrusts.")]
        public Vector3 tip = new Vector3(0f, 0f, 0.86f);
        [Tooltip("Where a cut strikes: the middle of the edge.")]
        public Vector3 edge = new Vector3(0f, 0f, 0.57f);
        [Tooltip("Where a club strikes.")]
        public Vector3 head = new Vector3(0f, 0f, 0.57f);
        [Tooltip("Shields: the middle of the face.")]
        public Vector3 faceCenter = new Vector3(0f, 0f, 0.1f);
        public float radius;

        [Header("Guard (medium size; the unit's frame: x right, y up, z forward, from the chest)")]
        [Tooltip("Where the grip is held in guard, from the chest null.")]
        public Vector3 guardGrip = new Vector3(0.18f, -0.28f, 0.32f);
        [Tooltip("Which way the weapon points in guard: degrees up from forward, and turned in (+) across the body.")]
        public Vector2 guardAngles = new Vector2(35f, 12f);

        [Header("Moves")]
        public MoveDefinition[] moves = new MoveDefinition[0];

        public bool IsShield => kind == WeaponKind.Shield;

        /// <summary>The striking point a move uses, in weapon axes (medium size).</summary>
        public Vector3 StrikePoint(StrikeWith with)
        {
            switch (with)
            {
                case StrikeWith.Tip: return tip;
                case StrikeWith.Head: return head;
                default: return edge;
            }
        }

        public static WeaponDefinition CreateDefault(WeaponKind kind)
        {
            var weapon = CreateInstance<WeaponDefinition>();
            weapon.name = "WD_" + kind;
            weapon.kind = kind;
            switch (kind)
            {
                case WeaponKind.Club:
                    weapon.length = 0.83f;
                    weapon.massKg = 2f;
                    weapon.tip = new Vector3(0f, 0f, 0.72f);
                    weapon.edge = weapon.head = new Vector3(0f, 0f, 0.587f);
                    weapon.guardGrip = new Vector3(0.2f, -0.32f, 0.26f);
                    weapon.guardAngles = new Vector2(55f, 8f);
                    break;
                case WeaponKind.Shield:
                    weapon.length = 0.58f;
                    weapon.massKg = 4f;
                    weapon.radius = 0.29f;
                    weapon.faceCenter = new Vector3(0f, 0f, 0.1f);
                    // For a shield the guard is where the middle of its face is held, and which way the face looks.
                    weapon.guardGrip = new Vector3(-0.12f, -0.1f, 0.36f);
                    weapon.guardAngles = new Vector2(8f, 18f);
                    break;
            }

            return weapon;
        }
    }
}
