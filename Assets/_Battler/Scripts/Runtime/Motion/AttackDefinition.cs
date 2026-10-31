using UnityEngine;

namespace PhysicsBattler.Motion
{
    /// <summary>
    /// A parametric swing: phase timings, the swing angles on the plane, how much the body joins
    /// in, and how hard the arm pushes. One asset per kind of swing; every swing it produces is
    /// solved fresh for the distance and height of its target (see <see cref="SwingSolver"/>).
    /// </summary>
    [CreateAssetMenu(menuName = "Physics Battler/Attack Definition", fileName = "AttackDefinition")]
    public class AttackDefinition : ScriptableObject
    {
        [Header("Timing (seconds; an attack-speed stat would scale these)")]
        public float windUpTime = 0.8f;
        public float loadTime = 0.15f;
        public float strikeTime = 0.55f;
        public float followTime = 0.3f;
        public float recoverTime = 0.6f;

        [Header("Swing angles (degrees on the swing plane; 0 = contact)")]
        public float windUpAngle = -150f;
        public float loadAngle = -160f;
        public float followAngle = 40f;
        [Tooltip("Tilt of the swing plane: 0 vertical, 90 flat. Raised automatically if the weapon would hit the ground.")]
        public float planeRoll = 45f;
        [Tooltip("+1 swings on the right side, -1 on the left.")]
        public int side = 1;
        [Tooltip("Strike easing power: higher starts slower and arrives faster.")]
        public float strikePower = 1.8f;
        [Tooltip("Keep the whole weapon this high above the ground during the swing.")]
        public float groundClearance = 0.15f;

        [Header("Reach (scaled with the unit)")]
        public float minArm = 0.3f;
        public float maxArm = 0.72f;
        public float maxLean = 0.2f;

        [Header("Body")]
        public float twistDegrees = 35f;
        [Tooltip("Weight shifts onto the back foot while winding up (pelvis back, m at scale 1)…")]
        public float leanBack = 0.1f;
        [Tooltip("…and onto the front foot through the strike.")]
        public float leanForward = 0.15f;
        public float crouch = 0.06f;

        [Header("Muscles")]
        [Tooltip("Weapon arm strength during the strike.")]
        public float strikeArmBoost = 1.6f;
        [Tooltip("Weapon arm strength while recovering: the weapon drags back.")]
        public float recoverArmStrength = 0.6f;

        [Header("Damage intent per phase")]
        public float windUpIntent = 0.3f;
        public float strikeIntent = 1f;
        public float recoverIntent = 0.3f;
        public float abandonedIntent = 0.5f;

        public float TotalTime => windUpTime + loadTime + strikeTime + followTime + recoverTime;

        public static AttackDefinition CreateDefault()
        {
            var definition = CreateInstance<AttackDefinition>();
            definition.name = "AttackDefinition (default)";
            return definition;
        }
    }
}
