using UnityEditor;
using UnityEngine;

namespace PhysicsBattler.Editor.Setup
{
    /// <summary>
    /// Makes sure the physics settings the active ragdolls rely on are in place (the committed
    /// ProjectSettings already have them; this repairs a project where they were changed):
    /// the Body, Weapon, Prop and Ground layers, the Temporal Gauss Seidel solver, more solver
    /// iterations, gentle depenetration, enhanced determinism and a 60 Hz physics step.
    /// </summary>
    internal static class PhysicsSetup
    {
        private const int FirstUserLayer = 8;
        private const int LayerCount = 32;
        private const string LogPrefix = "[Battler Setup]";

        public static void Apply()
        {
            EnsureLayers();

            if (Physics.defaultSolverIterations < 12)
            {
                Physics.defaultSolverIterations = 12;
            }

            if (Physics.defaultSolverVelocityIterations < 4)
            {
                Physics.defaultSolverVelocityIterations = 4;
            }

            Physics.defaultMaxDepenetrationVelocity = Mathf.Min(Physics.defaultMaxDepenetrationVelocity, 4f);
            Physics.defaultMaxAngularSpeed = Mathf.Max(Physics.defaultMaxAngularSpeed, 50f);
            Physics.autoSyncTransforms = false;

            var dynamics = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/DynamicsManager.asset")[0]);
            bool changed = false;
            changed |= SetInt(dynamics, "m_SolverType", 1);                 // Temporal Gauss Seidel
            changed |= SetInt(dynamics, "m_EnableEnhancedDeterminism", 1);
            if (changed)
            {
                dynamics.ApplyModifiedPropertiesWithoutUndo();
            }

            if (Mathf.Abs(Time.fixedDeltaTime - 1f / 60f) > 0.0005f)
            {
                Debug.LogWarning($"{LogPrefix} The physics step is {Time.fixedDeltaTime:0.0000} s. The units were tuned at " +
                                 "1/60 s (Edit > Project Settings > Time > Fixed Timestep = 0.0166667).");
            }
        }

        private static bool SetInt(SerializedObject settings, string property, int value)
        {
            var field = settings.FindProperty(property);
            if (field == null)
            {
                Debug.LogWarning($"{LogPrefix} Physics setting {property} not found; set it by hand if needed.");
                return false;
            }

            if (field.intValue == value)
            {
                return false;
            }

            field.intValue = value;
            return true;
        }

        private static void EnsureLayers()
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tagManager.FindProperty("layers");
            bool added = false;
            foreach (var layerName in new[] { Layers.Body, Layers.Weapon, Layers.Prop, Layers.Ground })
            {
                added |= EnsureLayer(layers, layerName);
            }

            if (added)
            {
                tagManager.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static bool EnsureLayer(SerializedProperty layers, string layerName)
        {
            int free = -1;
            for (int i = FirstUserLayer; i < LayerCount; i++)
            {
                string current = layers.GetArrayElementAtIndex(i).stringValue;
                if (current == layerName)
                {
                    return false;
                }

                if (free < 0 && string.IsNullOrEmpty(current))
                {
                    free = i;
                }
            }

            if (free < 0)
            {
                Debug.LogWarning($"{LogPrefix} No free layer for {layerName}.");
                return false;
            }

            layers.GetArrayElementAtIndex(free).stringValue = layerName;
            return true;
        }
    }
}
