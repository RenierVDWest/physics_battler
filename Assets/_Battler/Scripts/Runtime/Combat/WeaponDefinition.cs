using UnityEngine;

namespace PhysicsBattler.Combat
{
    public enum WeaponKind
    {
        Sword,
        Club,
        Shield,

        /// <summary>Milestone 10: the bare off hand's fist (no prefab: the hand itself strikes).</summary>
        Fist,
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

        /// <summary>Milestone 10: the knuckles of a bare fist (the hand's own frame, not a weapon's).</summary>
        Fist,
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
        [Tooltip("How thick the striking part is for hit detection (metres, medium size): the blade's half-width, the club head's radius.")]
        public float sweepRadius = 0.035f;

        [Header("Hits")]
        [Tooltip("Damage at 6 m/s to the chest; scales with the weapon's speed and the part struck.")]
        public float damage = 20f;
        [Tooltip("Slower than this (m/s at the striking part), a touch is not a hit.")]
        public float minHitSpeed = 2f;
        [Tooltip("A sword's edge this fast (m/s) or faster, crossing a limb near a joint and square to the bone, can cut it off there (if the blow is hard enough for that joint). Clubs and shields never sever.")]
        public float severSpeed = 7f;

        [Header("Guard (medium size; the unit's frame: x right, y up, z forward, from the chest)")]
        [Tooltip("Where the grip is held in guard, from the chest null.")]
        public Vector3 guardGrip = new Vector3(0.18f, -0.28f, 0.32f);
        [Tooltip("Which way the weapon points in guard: degrees up from forward, and turned in (+) across the body.")]
        public Vector2 guardAngles = new Vector2(35f, 12f);

        [Tooltip("The arm rebuild: how far the hand may roll about the grip when the wrist can't take the turn asked (degrees each way): a club's roll hardly shows, a sword's edge must land.")]
        public float gripRoll;

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

        /// <summary>The arm rebuild's grip roll by kind: a club 30, a sword 10, a shield none.</summary>
        public static float DefaultGripRoll(WeaponKind kind) => kind == WeaponKind.Club ? 30f : kind == WeaponKind.Shield ? 0f : 10f;

        private static WeaponDefinition bareFist;

        /// <summary>
        /// Milestone 10: the bare off hand as a weapon: its fist, with the blows a bare hand throws
        /// (the jab; each move says which bodies throw it). Made once, in code: it has no prefab.
        /// </summary>
        public static WeaponDefinition BareFist
        {
            get
            {
                if (bareFist == null)
                {
                    bareFist = CreateDefault(WeaponKind.Fist);
                    bareFist.hideFlags = HideFlags.DontSave;
                    var jab = MoveDefinition.Create(MoveStyle.OffJab);
                    jab.hideFlags = HideFlags.DontSave;
                    bareFist.moves = new[] { jab };
                }

                return bareFist;
            }
        }

        public static WeaponDefinition CreateDefault(WeaponKind kind)
        {
            var weapon = CreateInstance<WeaponDefinition>();
            weapon.name = "WD_" + kind;
            weapon.kind = kind;
            weapon.gripRoll = DefaultGripRoll(kind);
            switch (kind)
            {
                case WeaponKind.Club:
                    weapon.damage = 16f;
                    weapon.sweepRadius = 0.06f;
                    weapon.length = 0.83f;
                    weapon.massKg = 2f;
                    weapon.tip = new Vector3(0f, 0f, 0.72f);
                    weapon.edge = weapon.head = new Vector3(0f, 0f, 0.587f);
                    weapon.guardGrip = new Vector3(0.2f, -0.32f, 0.26f);
                    weapon.guardAngles = new Vector2(55f, 8f);
                    break;
                case WeaponKind.Fist:
                    // (A punch: light, but the arm and the body behind it; it lands where the knuckles are.)
                    weapon.damage = 7f;
                    weapon.sweepRadius = 0.045f;
                    weapon.length = 0.1f;
                    weapon.massKg = 0.6f;
                    weapon.minHitSpeed = 1.5f;
                    weapon.tip = weapon.edge = weapon.head = Vector3.zero;
                    break;
                case WeaponKind.Shield:
                    weapon.damage = 0f;
                    weapon.sweepRadius = 0f;
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
