using UnityEngine;

namespace PhysicsBattler.Motion
{
    /// <summary>
    /// How a unit stands and moves while it isn't attacking: its stance, its guard, and how much it
    /// sways, bobs, weaves and shuffles. Blows go to where a body part was when they started, so this
    /// movement decides what they strike. Lengths are fractions of the unit's leg length, so one
    /// style fits any body size.
    /// </summary>
    [CreateAssetMenu(menuName = "Physics Battler/Combat Style", fileName = "CombatStyle")]
    public class CombatStyle : ScriptableObject
    {
        [Header("Stance (fractions of leg length; angles in degrees)")]
        [Tooltip("Distance between the feet, side to side.")]
        public float stanceWidth = 0.34f;
        [Tooltip("Distance between the feet, front to back.")]
        public float stanceLength = 0.42f;
        [Tooltip("How far the hips sink below standing straight.")]
        public float crouch = 0.07f;
        public bool leadWithLeftFoot = true;
        public float leadToeOut = 10f;
        public float rearToeOut = 35f;
        [Tooltip("The hips turn away from the lead side (the rear shoulder back)…")]
        public float hipTurn = 22f;
        [Tooltip("…and the chest turns part of the way back toward the target.")]
        public float chestCounterTurn = -10f;
        [Tooltip("Forward lean of the spine and chest.")]
        public float lean = 6f;
        [Tooltip("Share of the weight on the lead foot (0.5 = even). Moves the hips over it.")]
        [Range(0f, 1f)] public float leadWeight = 0.45f;

        [Header("Guard (local rotations of the arms, degrees)")]
        [Tooltip("Upper arms raised forward (negative is forward).")]
        public float guardRaise = -32f;
        [Tooltip("Upper arms brought in toward the body.")]
        public float guardIn = 14f;
        [Tooltip("Elbows bent (negative bends forward).")]
        public float guardElbow = -100f;
        [Tooltip("Lead arm held further forward than the rear arm.")]
        public float leadArmReach = -12f;

        [Header("Sway (the hips drift in a slow loop)")]
        public float swayAmplitude = 0.03f;
        public float swayFrequency = 0.45f;
        [Header("Bob (up and down, like bouncing on the toes)")]
        public float bobAmplitude = 0.012f;
        public float bobFrequency = 1.6f;
        [Header("Weave (the chest and head)")]
        public float weaveDegrees = 4f;
        public float weaveFrequency = 0.35f;

        [Header("Fighting (sandbox gameplay until milestone 6)")]
        [Tooltip("Seconds between blows, at random in this range.")]
        public Vector2 attackInterval = new Vector2(1.6f, 3f);
        [Tooltip("How close the unit closes in on its target: chest to chest, metres at medium size.")]
        public float engageDistance = 1.25f;
        [Tooltip("Walking speed while closing in, metres per second at medium size.")]
        public float walkSpeed = 0.7f;
        [Tooltip("With a shield: the chance it raises the shield across a blow coming at it.")]
        [Range(0f, 1f)] public float blockChance = 0.55f;
        [Tooltip("Seconds from a blow starting to the shield moving.")]
        public float reactionTime = 0.22f;

        [Header("Fight Rhythm (how it moves between blows)")]
        [Tooltip("The fighting style's base loop, postures and mood. Empty: the ready-made one for the unit's body (large: boxing, medium: shield and sword, small: Eskrima).")]
        public RhythmDefinition rhythm;

        [Header("Steps")]
        [Tooltip("A foot steps when it is this far from its spot in the stance (fraction of leg length)…")]
        public float stepThreshold = 0.16f;
        [Tooltip("…or turned this far from it (degrees).")]
        public float stepTurnThreshold = 25f;
        [Tooltip("Seconds per step at medium size (bigger units step slower, smaller faster).")]
        public float stepDuration = 0.26f;
        [Tooltip("Foot lift at the top of a step (fraction of leg length).")]
        public float stepHeight = 0.07f;
        [Tooltip("Seconds between idle shuffle steps (0 = never).")]
        public float shuffleInterval = 2.5f;
        [Tooltip("How far an idle shuffle moves a foot (fraction of leg length).")]
        public float shuffleDistance = 0.1f;

        public static CombatStyle CreateDefault()
        {
            var style = CreateInstance<CombatStyle>();
            style.name = "CombatStyle (default)";
            return style;
        }
    }
}
