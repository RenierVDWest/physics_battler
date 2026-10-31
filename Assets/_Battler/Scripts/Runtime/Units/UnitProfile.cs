using UnityEngine;

namespace PhysicsBattler.Units
{
    /// <summary>
    /// Everything that makes one kind of unit feel the way it does: size and mass, how strong its
    /// muscles are, how hard the balance assist holds it up, and how easily it is staggered.
    /// Edit these in Play mode to tune live; changes persist after Play stops.
    /// </summary>
    [CreateAssetMenu(menuName = "Physics Battler/Unit Profile", fileName = "UnitProfile")]
    public class UnitProfile : ScriptableObject
    {
        [Header("Body")]
        [Tooltip("Size relative to the rig file. The goblin is a 0.62 scale orc.")]
        public float scale = 1f;
        [Tooltip("Total mass of the body parts. Each part keeps its share from the rig file.")]
        public float totalMassKg = 120f;
        public int solverIterations = 16;
        public int solverVelocityIterations = 6;
        public float linearDamping = 0.05f;
        public float angularDamping = 0.1f;
        public float maxAngularVelocity = 50f;
        public float maxDepenetrationVelocity = 4f;
        public float footFriction = 0.9f;
        public float bodyFriction = 0.4f;

        [Header("Strength")]
        [Tooltip("Torque (N·m) the unit can put into swinging a weapon at the shoulder, with its body behind it. Sets how fast "
                 + "it can pull a weapon back, stop it, and swing it, and which weapons it can swing at all.")]
        public float strength = 240f;
        [Tooltip("A weapon can be swung if strength is at least this many times the torque it takes to hold it out at arm's length.")]
        public float minimumLiftRatio = 1.2f;

        [Header("Muscles (joint drives, natural frequency in Hz)")]
        [Tooltip("Higher is stiffer. Drives are acceleration-based, so these hold for any unit size.")]
        public float legsHz = 3.8f;
        public float coreHz = 4.4f;
        public float neckHz = 3.2f;
        public float armHz = 3.4f;
        [Tooltip("1 = critically damped. Lower is bouncier, higher is heavier.")]
        public float dampingRatio = 0.75f;
        [Tooltip("Upper bound on how much a heavy subtree (an arm holding a club) multiplies its joint's spring.")]
        public float maxSubtreeBoost = 500f;
        [Tooltip("Share of each joint's gravity load (everything hanging below it) the muscles hold up by themselves, "
                 + "so the springs only deal with motion, not sag. Scaled by muscle strength; stance legs are left out.")]
        [Range(0f, 1.2f)] public float gravityCompensation = 1f;
        [Tooltip("How much the muscles also follow the ghost's speed, not only its pose (fast swings lag far less). 0 = off.")]
        [Range(0f, 1f)] public float velocityFeedForward = 1f;
        [Tooltip("Smallest rotational inertia (kg·m², at scale 1) any part gets, like a motor's armature: keeps light "
                 + "forearms and hands from needing huge drive boosts to move a heavy weapon.")]
        public float minPartInertia = 0.08f;
        [Tooltip("Planted legs hold up the body above them (their share of its weight), so the legs carry the unit "
                 + "rather than the balance assist. 0 = off.")]
        [Range(0f, 1.2f)] public float legSupport = 1f;

        [Header("Hit dips (local muscle softening where a hit lands)")]
        public float dipStrength = 0.2f;
        public float dipHold = 0.15f;
        public float dipRecover = 0.4f;

        [Header("Balance assist (\"hand of god\")")]
        public float kpVertical = 90f;
        public float kdVertical = 16f;
        public float kpHorizontal = 30f;
        public float kdHorizontal = 9f;
        public float kpUpright = 180f;
        public float kdUpright = 24f;
        [Tooltip("The upright torque acts as if turning this share of the whole body about its feet "
                 + "(mass × centre-of-mass height²), split between pelvis and chest. 0 turns only the parts themselves.")]
        public float uprightBodyShare = 0.2f;
        [Tooltip("Share of body weight the assist carries on its own; the legs carry the rest.")]
        public float supportFeedForward = 0.2f;
        [Tooltip("Small upward pull on the head while balanced (the TABS trick). 0 = off.")]
        public float headLift = 0f;
        [Tooltip("How far the body may drift from where the unit thinks it is before the unit follows it.")]
        public float reanchorRadius = 0.15f;
        [Tooltip("Share of the sideways pull while the weight is over the feet; it comes in fully only as the body "
                 + "falls out of its support area. Lower: the legs hold the unit in place, not the assist.")]
        [Range(0f, 1f)] public float horizontalAssistInside = 0.25f;

        [Header("Stability (poise)")]
        public float maxStability = 100f;
        [Tooltip("Divides stability loss. 1 = orc. Higher is harder to stagger.")]
        public float poiseResistance = 1f;
        public float regenDelay = 0.8f;
        public float regenRate = 22f;
        public float flinchThreshold = 4f;
        public float staggerBelow = 60f;
        public float knockdownBelow = 25f;
        public float maxTiltDegrees = 60f;
        [Tooltip("Contact impulse (N s) between bodies below which a shove costs no stability.")]
        public float shoveThreshold = 18f;
        public float shoveStabilityPerImpulse = 0.08f;

        [Header("Stance (feet planted apart, one forward: support in every direction)")]
        [Tooltip("Each foot's distance out to the side of the centre (m at scale 1).")]
        public float stanceWidth = 0.22f;
        [Tooltip("How far the lead foot is in front of the centre, and the other behind it (m at scale 1).")]
        public float stanceLength = 0.17f;
        [Tooltip("Lead with the left foot (right-handed fighters); off leads with the right.")]
        public bool leadWithLeftFoot = true;
        [Tooltip("Toes turned out (degrees); the back foot turns out more.")]
        public float stanceToeOut = 12f;
        [Tooltip("Hips lowered by this much (m at scale 1): knees bent, ready.")]
        public float stanceCrouch = 0.06f;

        [Header("Catch steps")]
        [Tooltip("Step once the capture point is this far outside the area the feet support (m at scale 1).")]
        public float stepThreshold = 0.05f;
        [Tooltip("While steady (not staggered), step only when the weight is this many times the threshold off the feet.")]
        public float steadyStepFactor = 1.5f;
        public float stepDuration = 0.24f;
        public float stepHeight = 0.12f;
        public float maxStepLength = 0.55f;
        public int maxSteps = 3;

        [Header("Knockdown and getting up")]
        public float knockdownMinTime = 1.0f;
        public float restSpeed = 0.35f;
        public float restTime = 0.5f;
        [Tooltip("Crouch then rise. The unit is only steady again once it is really up (pelvis and chest).")]
        public float getUpDuration = 1.8f;
        [Tooltip("Extra time to come fully upright after the get-up pose is reached, before it counts as a failed get-up.")]
        public float getUpGrace = 1.5f;
        [Tooltip("Balance assist at the very start of getting up (it rises to full as the get-up goes on).")]
        [Range(0f, 1f)] public float getUpStartAssist = 0.6f;
        [Tooltip("Up means the pelvis at this share of its rest height or more…")]
        public float standPelvisShare = 0.8f;
        [Tooltip("…and the chest within this many degrees of upright.")]
        public float standMaxTilt = 35f;
        [Range(0f, 1f)] public float knockdownMuscle = 0.3f;

        [Header("Health")]
        public float maxHealth = 100f;

        public static UnitProfile CreateDefault()
        {
            var profile = CreateInstance<UnitProfile>();
            profile.name = "UnitProfile (default)";
            return profile;
        }
    }
}
