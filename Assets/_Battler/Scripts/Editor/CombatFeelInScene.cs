using PhysicsBattler.Locomotion;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PhysicsBattler.Editor
{
    /// <summary>
    /// Keeps a "Combat Feel" object in any scene with the locomotion lab in it (the fight's visual feedback to tweak,
    /// <see cref="CombatFeel"/>): added when such a scene is opened without one, the scene marked changed so it is
    /// saved with it. Also on the menu: Physics Battler > Add Combat Feel.
    /// </summary>
    [InitializeOnLoad]
    internal static class CombatFeelInScene
    {
        static CombatFeelInScene()
        {
            if (Application.isBatchMode)
            {
                return;
            }

            EditorSceneManager.sceneOpened += (scene, mode) => EditorApplication.delayCall += Ensure;
            EditorApplication.delayCall += Ensure;
        }

        [MenuItem("Physics Battler/Add Combat Feel")]
        private static void AddFromMenu()
        {
            if (!Add(true))
            {
                Selection.activeObject = Object.FindAnyObjectByType<CombatFeel>();
            }
        }

        private static void Ensure()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || Object.FindAnyObjectByType<LocomotionLab>() == null)
            {
                return;
            }

            Add(false);
        }

        /// <summary>Adds one if the open scene has none; true if it did.</summary>
        private static bool Add(bool select)
        {
            if (Object.FindAnyObjectByType<CombatFeel>() != null)
            {
                return false;
            }

            var made = new GameObject("Combat Feel");
            made.AddComponent<CombatFeel>();
            Undo.RegisterCreatedObjectUndo(made, "Add Combat Feel");
            EditorSceneManager.MarkSceneDirty(made.scene);
            if (select)
            {
                Selection.activeObject = made;
            }

            Debug.Log("[Combat Feel] added to the scene: the fight's visual feedback (the hit bend and the rest) is tweaked there; save the scene to keep it.");
            return true;
        }
    }
}
