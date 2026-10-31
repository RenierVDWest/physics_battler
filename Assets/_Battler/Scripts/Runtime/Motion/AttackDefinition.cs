using UnityEngine;

namespace PhysicsBattler.Motion
{
    /// <summary>
    /// A swing: where it goes on its plane, how hard each phase works, how the body joins in. There
    /// are no phase durations: how long a swing takes comes from the weapon's real weight and reach
    /// and the wielder's strength (<see cref="SwingDynamics"/>). A heavy club pulled back by an orc
    /// that can only just swing it is slow to wind up, has to be brought to a stop behind him before
    /// the strike, and takes a while to stop again after it.
    ///
    /// Phases: wind-up (pull back to the wound-up position and stop there), load (hold until the real
    /// weapon has arrived and settled: the commit point), strike (drive it at the target), follow
    /// (brake it to a stop), recover (bring it back to the carry).
    /// </summary>
    [CreateAssetMenu(menuName = "Physics Battler/Attack Definition", fileName = "AttackDefinition")]
    public class AttackDefinition : ScriptableObject
    {
        [Header("Swing angles (degrees on the swing plane; 0 = contact)")]
        [Tooltip("Where the weapon is pulled back to and stopped before the strike (below -180 is back and up).")]
        public float windUpAngle = -150f;
        [Tooltip("How far past contact the follow-through may take to brake the weapon to a stop. The strike is held to the "
                 + "speed that can be stopped within it: a longer follow-through allows a faster blow.")]
        public float followAngle = 75f;
        [Tooltip("Tilt of the swing plane: 0 vertical, 90 flat. Raised automatically if the weapon would hit the ground.")]
        public float planeRoll = 45f;
        [Tooltip("+1 swings on the right side, -1 on the left.")]
        public int side = 1;
        [Tooltip("Keep the whole weapon this high above the ground during the swing.")]
        public float groundClearance = 0.15f;

        [Header("Effort (share of the wielder's strength each phase uses)")]
        [Range(0.1f, 1f)] public float windUpEffort = 0.55f;
        [Range(0.1f, 1f)] public float strikeEffort = 1f;
        [Range(0.1f, 1f)] public float brakeEffort = 0.9f;
        [Range(0.1f, 1f)] public float recoverEffort = 0.5f;

        [Header("Turning point (the strike starts the moment the pull-back has been stopped)")]
        [Tooltip("The real weapon within this many degrees of the wound-up position…")]
        public float settleAngle = 45f;
        [Tooltip("…and no longer moving back faster than this (degrees/s along the swing)…")]
        public float stopRate = 40f;
        [Tooltip("…for this long (s), and the strike starts.")]
        public float settleTime = 0.03f;
        [Tooltip("Strike anyway after holding this long (s).")]
        public float maxSettleTime = 0.4f;
        [Tooltip("After the follow-through, the recover waits until the tip is slower than this (m/s).")]
        public float settleSpeed = 1.5f;
        [Tooltip("Share of the weapon's speed a solid hit takes out of it.")]
        [Range(0f, 1f)] public float hitAbsorb = 0.5f;

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
        [Tooltip("Weapon arm stiffness during the strike (on top of following the swing's speed).")]
        public float strikeArmBoost = 1.6f;
        [Tooltip("Weapon arm stiffness while recovering.")]
        public float recoverArmStrength = 0.6f;

        [Header("Damage intent per phase")]
        public float windUpIntent = 0.3f;
        public float strikeIntent = 1f;
        public float recoverIntent = 0.3f;
        public float abandonedIntent = 0.5f;

        public static AttackDefinition CreateDefault()
        {
            var definition = CreateInstance<AttackDefinition>();
            definition.name = "AttackDefinition (default)";
            return definition;
        }
    }
}
