// Compile-check stubs for the URP / Core RP members the editor setup uses.
namespace UnityEngine.Rendering
{
    public static class ResourceReloader
    {
        public static bool ReloadAllNullIn(Object container, string basePath) => true;
    }
}

namespace UnityEngine.Rendering.Universal
{
    public abstract class ScriptableRendererData : ScriptableObject { }

    public class PostProcessData : ScriptableObject { }

    public class UniversalRendererData : ScriptableRendererData
    {
        public PostProcessData postProcessData { get; set; }
    }

    public class UniversalRenderPipelineAsset : RenderPipelineAsset
    {
        public static string packagePath => "Packages/com.unity.render-pipelines.universal";
        public static UniversalRenderPipelineAsset Create(ScriptableRendererData rendererData = null) => CreateInstance<UniversalRenderPipelineAsset>();
        public bool supportsHDR { get; set; }
        public int msaaSampleCount { get; set; }
        public float renderScale { get; set; }
        public float shadowDistance { get; set; }
        public int shadowCascadeCount { get; set; }
    }

    public class UniversalAdditionalCameraData : MonoBehaviour { }

    public class UniversalAdditionalLightData : MonoBehaviour { }
}
