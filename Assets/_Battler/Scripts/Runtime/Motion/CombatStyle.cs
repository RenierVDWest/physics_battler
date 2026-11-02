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
        [Tooltip("The arm rebuild: the weapon arm's elbow null in guard (degrees round the arm: 0 hanging relaxed, +90 out to the side, 180 up and over, -90 in across).")]
        public float guardElbowSwivel = 20f;

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
        [Tooltip("Milestone 11: without a shield, the chance it sets its weapon across a blow coming at it (a parry; less sure than a shield).")]
        [Range(0f, 1f)] public float parryChance = 0.4f;

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

        [Header("Reading the opponent (milestone 6)")]
        [Tooltip("How often it reads an opening in its opponent (a posture's window, a tired guard) and strikes into it: aims at the open part, and goes early if its next blow is close.")]
        [Range(0f, 1f)] public float readsOpenings = 0.5f;
        [Tooltip("How early (seconds) it will strike to catch an opening.")]
        public float openingLead = 0.8f;

        [Header("Walking")]
        [Tooltip("A walk cycle from motion capture (Tools/Mocap): its steps and body motion play while the unit walks (empty: the feet just keep up).")]
        public GaitClip walk = new GaitClip();

        public static CombatStyle CreateDefault()
        {
            var style = CreateInstance<CombatStyle>();
            style.name = "CombatStyle (default)";
            style.guardElbowSwivel = DefaultGuardElbowSwivel(null);
            return style;
        }

        /// <summary>The arm rebuild's starting guard elbows: hanging, a little out.</summary>
        public static float DefaultGuardElbowSwivel(string body)
        {
            switch ((body ?? "").ToLowerInvariant())
            {
                case "large": return 20f;
                default: return 25f;
            }
        }

        /// <summary>The ready-made style for a body size ("Small", "Medium", "Large"), as the setup makes it.</summary>
        public static CombatStyle CreateFor(string body)
        {
            var s = CreateDefault();
            s.name = "CS_" + body;
            s.guardElbowSwivel = DefaultGuardElbowSwivel(body);
            switch ((body ?? "").ToLowerInvariant())
            {
                case "small":
                    s.swayFrequency = 0.7f;
                    s.bobFrequency = 2.2f;
                    s.weaveFrequency = 0.6f;
                    s.shuffleInterval = 1.4f;
                    s.shuffleDistance = 0.12f;
                    s.guardRaise = -40f;
                    s.guardElbow = -110f;
                    ApplySmallStance(s);
                    s.walk = RhythmClips.SmallWalk();
                    s.readsOpenings = 0.75f;   // (a stick fighter lives on timing)
                    break;
                case "medium":
                    ApplyMediumStance(s);
                    s.walk = RhythmClips.MediumWalk();
                    s.readsOpenings = 0.55f;
                    break;
                case "large":
                    ApplyLargeStance(s);
                    s.walk = RhythmClips.LargeWalk();
                    s.readsOpenings = 0.35f;   // (a pressure fighter: it makes its own openings)
                    s.shuffleInterval = 5f;
                    s.shuffleDistance = 0.07f;
                    s.guardRaise = -22f;
                    s.guardIn = 8f;
                    s.guardElbow = -80f;
                    s.leadArmReach = -6f;
                    break;
            }

            return s;
        }

        /// <summary>
        /// The large unit's boxing stance, from the captured boxing idle its Fight Rhythm plays
        /// (Boxing.fbx): bladed and long, the hips turned well round, sitting a little low, the
        /// weight about even. Its own idle sway, bob and weave are off: the captured loop moves it.
        /// </summary>
        public static void ApplyLargeStance(CombatStyle s)
        {
            s.stanceWidth = 0.32f;
            s.stanceLength = 0.58f;
            s.crouch = 0.09f;
            s.lean = 3f;
            s.hipTurn = 36f;
            s.chestCounterTurn = -8f;
            s.leadWeight = 0.52f;
            s.swayAmplitude = 0f;
            s.bobAmplitude = 0f;
            s.weaveDegrees = 0f;
        }

        /// <summary>
        /// The medium unit's stance, from its captured idle (Medium_Idle.fbx): side-on, the feet
        /// nearly in line and long (a little wider than the capture's, to stand on), sitting low,
        /// the weight back on the rear foot, the shield arm out toward the opponent.
        /// </summary>
        public static void ApplyMediumStance(CombatStyle s)
        {
            s.stanceWidth = 0.16f;
            s.stanceLength = 0.62f;
            s.crouch = 0.15f;
            s.lean = 1f;
            s.hipTurn = 60f;
            s.chestCounterTurn = 0f;
            s.leadWeight = 0.42f;   // (the capture's 0.28, forward a little for the shield out in front)
            s.swayAmplitude = 0f;
            s.bobAmplitude = 0f;
            s.weaveDegrees = 0f;
        }

        /// <summary>
        /// The small unit's stance, from its captured idle (Small_Idle.fbx): side-on, low, the hands
        /// low, the weight about even.
        /// </summary>
        public static void ApplySmallStance(CombatStyle s)
        {
            s.stanceWidth = 0.25f;
            s.stanceLength = 0.57f;
            s.crouch = 0.14f;
            s.lean = 0f;
            s.hipTurn = 60f;
            s.chestCounterTurn = 0f;
            s.leadWeight = 0.46f;
            s.swayAmplitude = 0f;
            s.bobAmplitude = 0f;
            s.weaveDegrees = 0f;
        }

        /// <summary>The medium unit's stance before its captured idle (the defaults: setup 16 and earlier).</summary>
        public static bool HasOldMediumStance(CombatStyle s)
        {
            return Mathf.Approximately(s.stanceWidth, 0.34f) && Mathf.Approximately(s.stanceLength, 0.42f)
                && Mathf.Approximately(s.crouch, 0.07f) && Mathf.Approximately(s.hipTurn, 22f) && Mathf.Approximately(s.lean, 6f)
                && Mathf.Approximately(s.chestCounterTurn, -10f) && Mathf.Approximately(s.leadWeight, 0.45f)
                && Mathf.Approximately(s.swayAmplitude, 0.03f) && Mathf.Approximately(s.bobAmplitude, 0.012f) && Mathf.Approximately(s.weaveDegrees, 4f);
        }

        /// <summary>The small unit's stance before its captured idle (setup 16 and earlier).</summary>
        public static bool HasOldSmallStance(CombatStyle s)
        {
            return Mathf.Approximately(s.stanceWidth, 0.36f) && Mathf.Approximately(s.stanceLength, 0.46f)
                && Mathf.Approximately(s.crouch, 0.1f) && Mathf.Approximately(s.lean, 10f)
                && Mathf.Approximately(s.hipTurn, 22f) && Mathf.Approximately(s.chestCounterTurn, -10f) && Mathf.Approximately(s.leadWeight, 0.45f)
                && Mathf.Approximately(s.swayAmplitude, 0.05f) && Mathf.Approximately(s.bobAmplitude, 0.025f) && Mathf.Approximately(s.weaveDegrees, 7f);
        }

        /// <summary>The large unit's stance before it took the captured boxing idle's (setup 15 and earlier).</summary>
        public static bool HasOldLargeStance(CombatStyle s)
        {
            return Mathf.Approximately(s.stanceWidth, 0.4f) && Mathf.Approximately(s.stanceLength, 0.36f)
                && Mathf.Approximately(s.crouch, 0.05f) && Mathf.Approximately(s.hipTurn, 14f) && Mathf.Approximately(s.lean, 4f)
                && Mathf.Approximately(s.chestCounterTurn, -10f) && Mathf.Approximately(s.leadWeight, 0.45f)
                && Mathf.Approximately(s.swayAmplitude, 0.018f) && Mathf.Approximately(s.bobAmplitude, 0.004f) && Mathf.Approximately(s.weaveDegrees, 2.5f);
        }
    }
}
