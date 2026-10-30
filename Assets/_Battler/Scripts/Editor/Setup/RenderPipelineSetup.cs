using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PhysicsBattler.Editor.Setup
{
    /// <summary>
    /// Creates the URP asset and its renderer (the same steps as URP's own "Create > Rendering >
    /// URP Asset (with Universal Renderer)") and makes it the pipeline for the project and every
    /// quality level. Adapted from Hexamus's setup, with desktop values.
    /// </summary>
    internal static class RenderPipelineSetup
    {
        private static readonly string DefaultPostProcessDataPath =
            UniversalRenderPipelineAsset.packagePath + "/Runtime/Data/PostProcessData.asset";

        public static void Apply()
        {
            var asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(BattlerPaths.UrpAsset);
            if (asset == null)
            {
                asset = CreatePipelineAsset();
            }

            GraphicsSettings.defaultRenderPipeline = asset;
            ClearQualityLevelOverrides(asset);
        }

        private static void ClearQualityLevelOverrides(RenderPipelineAsset asset)
        {
            int originalLevel = QualitySettings.GetQualityLevel();
            for (int level = 0; level < QualitySettings.names.Length; level++)
            {
                var levelAsset = QualitySettings.GetRenderPipelineAssetAt(level);
                if (levelAsset != null && levelAsset != asset)
                {
                    QualitySettings.SetQualityLevel(level, false);
                    QualitySettings.renderPipeline = null;
                }
            }

            QualitySettings.SetQualityLevel(originalLevel, false);
        }

        private static UniversalRenderPipelineAsset CreatePipelineAsset()
        {
            EditorFolders.Ensure(BattlerPaths.RenderingFolder);

            var rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
            rendererData.postProcessData = AssetDatabase.LoadAssetAtPath<PostProcessData>(DefaultPostProcessDataPath);
            AssetDatabase.CreateAsset(rendererData, BattlerPaths.UrpRenderer);
            // No ResourceReloader call: in Unity 6, URP's shaders and resources live in the graphics
            // settings and are filled in by the pipeline itself.

            var asset = UniversalRenderPipelineAsset.Create(rendererData);
            asset.supportsHDR = true;
            asset.msaaSampleCount = 4;
            asset.renderScale = 1f;
            asset.shadowDistance = 40f;
            asset.shadowCascadeCount = 2;

            AssetDatabase.CreateAsset(asset, BattlerPaths.UrpAsset);
            return asset;
        }
    }
}
