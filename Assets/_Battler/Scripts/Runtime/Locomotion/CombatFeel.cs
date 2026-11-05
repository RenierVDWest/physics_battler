using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// The fight's visual feedback, in one place to tweak live (the scene's "Combat Feel" object): how a blow is
    /// felt on the body it lands on. Pose effects only, no physics. Its values are pushed to the statics the clip
    /// fighters read every frame, in play mode and out of it; with none in the scene (headless) the defaults hold.
    /// More of the fight's feel joins it as it is made.
    /// </summary>
    [ExecuteAlways]
    public sealed class CombatFeel : MonoBehaviour
    {
        [Header("Hit bend: a blow bends the body round where it landed")]
        [Tooltip("How far the bend pushes the struck part, per m/s of the blow's weight (metres at a Medium's size). A Medium's sword weighs about 0.4 m/s, its club 0.65, the Large's club 1.2 to 2.5.")]
        [Range(0f, 1.5f)] public float bendPerWeight = DefaultBendPerWeight;

        [Tooltip("The most a single blow's bend pushes the struck part (metres at a Medium's size).")]
        [Range(0f, 1f)] public float mostBend = DefaultMostBend;

        [Tooltip("How far from where the blow landed the bend reaches, falling off to nothing there (metres at a Medium's size): smaller, the bend is tighter round the blow; larger, the whole body goes with it.")]
        [Range(0.2f, 3f)] public float bendReach = DefaultBendReach;

        [Tooltip("How soon the bend is at its furthest (seconds); it is mostly gone in about five times this.")]
        [Range(0.02f, 0.3f)] public float bendPeak = DefaultBendPeak;

        [Header("Hit lean and head snap (the older whole-body reaction)")]
        [Tooltip("How far a blow leans the upper body as a whole: 1 as before, 0 none.")]
        [Range(0f, 3f)] public float leanScale = 1f;

        [Tooltip("How far a blow to the head snaps it: 1 as before, 0 none.")]
        [Range(0f, 3f)] public float headSnapScale = 1f;

        [Header("Hit stop: both stand still a moment as a blow lands")]
        [Tooltip("The hit stop's length before the blow's weight (seconds).")]
        [Range(0f, 0.2f)] public float hitStopBase = DefaultHitStopBase;

        [Tooltip("How much longer the hit stop is per m/s of the blow's weight (seconds): a Medium's sword weighs about 0.4 m/s, the Large's club 1.2 to 2.5.")]
        [Range(0f, 0.2f)] public float hitStopPerWeight = DefaultHitStopPerWeight;

        [Tooltip("The least and the most a hit stop lasts (seconds; a combo's showstopper holds longer still).")]
        [Range(0f, 0.2f)] public float leastHitStop = DefaultLeastHitStop;

        [Range(0f, 0.4f)] public float mostHitStop = DefaultMostHitStop;

        [Header("Driven back: what the struck part doesn't take moves the whole body")]
        [Tooltip("How hard a blow drives the body back, per m/s of the weight the struck part passes on.")]
        [Range(0f, 8f)] public float driveBack = DefaultDriveBack;

        [Tooltip("The most a blow drives the body back (m/s).")]
        [Range(0f, 4f)] public float mostDrive = DefaultMostDrive;

        [Tooltip("A blow this heavy or more (m/s of weight) throws the body back stumbling at once, its feet catching it, instead of a reaction first.")]
        [Range(0.2f, 4f)] public float stumbleFrom = DefaultStumbleFrom;

        [Tooltip("How much harder a stumble throws the body back than a drive, and the most it does (m/s).")]
        [Range(1f, 3f)] public float stumbleBoost = DefaultStumbleBoost;

        [Range(0f, 5f)] public float mostStumble = DefaultMostStumble;

        [Tooltip("How long a stumbling body takes to find its feet before it can strike again (seconds).")]
        [Range(0f, 2f)] public float stumbleRecovery = DefaultStumbleRecovery;

        public const float DefaultBendPerWeight = 0.35f, DefaultMostBend = 0.4f, DefaultBendReach = 1.2f, DefaultBendPeak = 0.08f;
        public const float DefaultHitStopBase = 0.03f, DefaultHitStopPerWeight = 0.05f, DefaultLeastHitStop = 0.04f, DefaultMostHitStop = 0.16f;
        public const float DefaultDriveBack = 3.5f, DefaultMostDrive = 2.2f, DefaultStumbleFrom = 1.0f, DefaultStumbleBoost = 1.3f, DefaultMostStumble = 2.8f, DefaultStumbleRecovery = 0.7f;

        /// <summary>The values in force (the scene's Combat Feel's, else the defaults).</summary>
        public static float BendPerWeight = DefaultBendPerWeight, MostBend = DefaultMostBend, BendReach = DefaultBendReach, BendPeak = DefaultBendPeak,
            LeanScale = 1f, HeadSnapScale = 1f,
            HitStopBase = DefaultHitStopBase, HitStopPerWeight = DefaultHitStopPerWeight, LeastHitStop = DefaultLeastHitStop, MostHitStop = DefaultMostHitStop,
            DriveBack = DefaultDriveBack, MostDrive = DefaultMostDrive, StumbleFrom = DefaultStumbleFrom, StumbleBoost = DefaultStumbleBoost, MostStumble = DefaultMostStumble, StumbleRecovery = DefaultStumbleRecovery;

        private void OnEnable() => Push();

        private void OnValidate() => Push();

        private void Update() => Push();

        private void OnDisable()
        {
            BendPerWeight = DefaultBendPerWeight;
            MostBend = DefaultMostBend;
            BendReach = DefaultBendReach;
            BendPeak = DefaultBendPeak;
            LeanScale = HeadSnapScale = 1f;
            HitStopBase = DefaultHitStopBase;
            HitStopPerWeight = DefaultHitStopPerWeight;
            LeastHitStop = DefaultLeastHitStop;
            MostHitStop = DefaultMostHitStop;
            DriveBack = DefaultDriveBack;
            MostDrive = DefaultMostDrive;
            StumbleFrom = DefaultStumbleFrom;
            StumbleBoost = DefaultStumbleBoost;
            MostStumble = DefaultMostStumble;
            StumbleRecovery = DefaultStumbleRecovery;
        }

        private void Push()
        {
            BendPerWeight = bendPerWeight;
            MostBend = mostBend;
            BendReach = bendReach;
            BendPeak = Mathf.Max(0.01f, bendPeak);
            LeanScale = leanScale;
            HeadSnapScale = headSnapScale;
            HitStopBase = hitStopBase;
            HitStopPerWeight = hitStopPerWeight;
            LeastHitStop = leastHitStop;
            MostHitStop = Mathf.Max(leastHitStop, mostHitStop);
            DriveBack = driveBack;
            MostDrive = mostDrive;
            StumbleFrom = stumbleFrom;
            StumbleBoost = stumbleBoost;
            MostStumble = mostStumble;
            StumbleRecovery = stumbleRecovery;
        }
    }
}
