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

        public const float DefaultBendPerWeight = 0.35f, DefaultMostBend = 0.4f, DefaultBendReach = 1.2f, DefaultBendPeak = 0.08f;

        /// <summary>The values in force (the scene's Combat Feel's, else the defaults).</summary>
        public static float BendPerWeight = DefaultBendPerWeight, MostBend = DefaultMostBend, BendReach = DefaultBendReach, BendPeak = DefaultBendPeak,
            LeanScale = 1f, HeadSnapScale = 1f;

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
        }

        private void Push()
        {
            BendPerWeight = bendPerWeight;
            MostBend = mostBend;
            BendReach = bendReach;
            BendPeak = Mathf.Max(0.01f, bendPeak);
            LeanScale = leanScale;
            HeadSnapScale = headSnapScale;
        }
    }
}
