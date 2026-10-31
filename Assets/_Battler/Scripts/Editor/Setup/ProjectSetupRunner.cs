using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace PhysicsBattler.Editor.Setup
{
    /// <summary>
    /// One-time project setup. It runs by itself the first time the project is opened (no render
    /// pipeline, no Sandbox scene, or an older setup version) and can be run again from
    /// Physics Battler > Setup > Run Project Setup. Generated prefabs are rebuilt every run, so tune
    /// units in their profiles (Data/Units) rather than on the prefabs.
    /// </summary>
    [InitializeOnLoad]
    internal static class ProjectSetupRunner
    {
        private const string LogPrefix = "[Battler Setup]";

        static ProjectSetupRunner()
        {
            if (Application.isBatchMode)
            {
                return;
            }

            EditorApplication.delayCall += RunIfNeeded;
        }

        private static bool IsSetupNeeded =>
            GraphicsSettings.defaultRenderPipeline == null
            || !File.Exists(BattlerPaths.SandboxScene)
            || !SetupVersion.IsCurrent;

        private static void RunIfNeeded()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += RunIfNeeded;
                return;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode || !IsSetupNeeded)
            {
                return;
            }

            Run();
        }

        [MenuItem("Physics Battler/Setup/Run Project Setup", priority = 0)]
        public static void Run()
        {
            Debug.Log($"{LogPrefix} Running project setup.");
            try
            {
                RenderPipelineSetup.Apply();
                PhysicsSetup.Apply();
                var materials = MaterialSetup.Apply();
                ModelImportSetup.Apply(materials);
                DataSetup.Apply();
                DataSetup.Migrate(SetupVersion.Installed);
                if (!PrefabSetup.ApplyInScratchScene(materials, out _))
                {
                    AssetDatabase.SaveAssets();
                    Debug.LogWarning($"{LogPrefix} Setup paused before the prefabs and the Sandbox scene, because saving "
                                     + "the open scene was cancelled. Run Physics Battler > Setup > Run Project Setup to finish.");
                    return;
                }

                SceneSetup.Apply();
                AssetDatabase.SaveAssets();
                SetupVersion.MarkCurrent();
            }
            catch (Exception exception)
            {
                Debug.LogError($"{LogPrefix} Setup failed. Please send this error to Claude.\n{exception}");
                return;
            }

            InputCheck.Report();
            Debug.Log($"{LogPrefix} Done. The Sandbox scene is open: press Play, then Space to swing.");
        }

        [MenuItem("Physics Battler/Setup/Rebuild Prefabs", priority = 1)]
        public static void RebuildPrefabs()
        {
            try
            {
                var materials = MaterialSetup.Apply();
                ModelImportSetup.Apply(materials);
                DataSetup.Apply();
                if (!PrefabSetup.ApplyInScratchScene(materials, out string previousScene))
                {
                    Debug.LogWarning($"{LogPrefix} Prefabs not rebuilt: saving the open scene was cancelled.");
                    return;
                }

                AssetDatabase.SaveAssets();
                string reopen = !string.IsNullOrEmpty(previousScene) ? previousScene
                    : File.Exists(BattlerPaths.SandboxScene) ? BattlerPaths.SandboxScene : null;
                if (reopen != null)
                {
                    UnityEditor.SceneManagement.EditorSceneManager.OpenScene(reopen);
                }

                Debug.Log($"{LogPrefix} Prefabs rebuilt.");
            }
            catch (Exception exception)
            {
                Debug.LogError($"{LogPrefix} Rebuilding prefabs failed.\n{exception}");
            }
        }
    }

    /// <summary>The sandbox reads mouse and keys through the Input System when it is active.</summary>
    internal static class InputCheck
    {
        public static void Report()
        {
#if !ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            Debug.LogWarning("[Battler Setup] No input backend is active. Set Edit > Project Settings > Player > " +
                             "Active Input Handling to Both and let Unity restart.");
#endif
        }
    }
}
