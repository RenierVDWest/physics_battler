using PhysicsBattler.Motion;
using UnityEngine;

namespace PhysicsBattler.Units
{
    /// <summary>
    /// One kind of unit: which body it is built from, how heavy it is, how it fights while idle
    /// (its <see cref="CombatStyle"/>), and how firmly physics may hold its upper body to the pose.
    /// </summary>
    [CreateAssetMenu(menuName = "Physics Battler/Unit Profile", fileName = "UnitProfile")]
    public class UnitProfile : ScriptableObject
    {
        [Header("Body")]
        [Tooltip("The base body: Small, Medium or Large (Data/Rigs/<body>_rig.json, Art/Bodies/<Body>/).")]
        public string body = "Medium";
        [Tooltip("Size against the body as modelled (1 = as modelled).")]
        public float scale = 1f;
        [Tooltip("Total mass in kg; 0 = the body's own (small 15, medium 80, large 280).")]
        public float totalMassKg;

        [Header("Fighting")]
        public CombatStyle style;
        [Tooltip("How fast the unit turns to face its target (degrees per second).")]
        public float turnSpeed = 220f;

        [Header("Physics authority")]
        [Tooltip("How firmly the torso and head are held to the pose each physics step (0 = loose, 1 = exactly).")]
        [Range(0f, 1f)] public float torsoPin = 0.85f;
        [Tooltip("How firmly the arms are held to the pose. Lower reads as heavier, with more lag and follow-through.")]
        [Range(0f, 1f)] public float armPin = 0.7f;

        [Header("Rigidbodies")]
        public float linearDamping = 0.05f;
        public float angularDamping = 0.1f;
        public int solverIterations = 12;
        public int solverVelocityIterations = 4;
        public float maxAngularVelocity = 30f;
        public float maxDepenetrationVelocity = 3f;
        public float bodyFriction = 0.6f;
        public float footFriction = 1.0f;

        public static UnitProfile CreateDefault()
        {
            var profile = CreateInstance<UnitProfile>();
            profile.name = "UnitProfile (default)";
            return profile;
        }
    }
}
