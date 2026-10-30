// Compile-check stubs: application, time, debug, rendering, GUI, scenes, attributes and input.
using System;
using System.Collections.Generic;

namespace UnityEngine
{
    public static class Application
    {
        public static bool isPlaying { get; set; } = true;
        public static bool isBatchMode { get; set; } = true;
        public static bool isEditor => true;
        public static string dataPath { get; set; } = "Assets";
    }

    public static class Time
    {
        public static float time { get; set; }
        public static float deltaTime { get; set; } = 1f / 60f;
        public static float fixedDeltaTime { get; set; } = 1f / 60f;
        public static float timeScale { get; set; } = 1f;
        public static float unscaledDeltaTime { get; set; } = 1f / 60f;
        public static int frameCount { get; set; }
    }

    public static class Debug
    {
        public static void Log(object message) => Console.WriteLine(message);
        public static void Log(object message, Object context) => Console.WriteLine(message);
        public static void LogWarning(object message) => Console.WriteLine("WARNING: " + message);
        public static void LogWarning(object message, Object context) => Console.WriteLine("WARNING: " + message);
        public static void LogError(object message) => Console.WriteLine("ERROR: " + message);
        public static void LogError(object message, Object context) => Console.WriteLine("ERROR: " + message);
        public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0f) { }
        public static void DrawRay(Vector3 start, Vector3 dir, Color color, float duration = 0f) { }
    }

    public static class Gizmos
    {
        public static Color color { get; set; }
        public static void DrawLine(Vector3 from, Vector3 to) { }
        public static void DrawWireSphere(Vector3 center, float radius) { }
        public static void DrawSphere(Vector3 center, float radius) { }
        public static void DrawWireCube(Vector3 center, Vector3 size) { }
    }

    public static class JsonUtility
    {
        /// <summary>Set by the test runner (System.Text.Json isn't part of .NET Standard 2.1).</summary>
        public static Func<string, Type, object> Deserializer;

        public static T FromJson<T>(string json)
        {
            if (Deserializer == null) throw new NotSupportedException("JsonUtility stub has no deserializer.");
            return (T)Deserializer(json, typeof(T));
        }
    }

    public class TextAsset : Object
    {
        public TextAsset() { }
        public TextAsset(string text) { this.text = text; }
        public string text { get; } = "";
    }

    public class Mesh : Object { }

    public class Shader : Object
    {
        public static Shader Find(string name) => null;
    }

    public class Material : Object
    {
        public Material(Shader shader) { }
        public Material(Material source) { }
        public Color color { get; set; }
        public void SetColor(string name, Color value) { }
        public void SetFloat(string name, float value) { }
        public Color GetColor(string name) => default;
    }

    public class Texture : Object { }

    public class Texture2D : Texture
    {
        public static Texture2D whiteTexture => null;
        public static Texture2D blackTexture => null;
        public static Texture2D redTexture => null;
        public static Texture2D grayTexture => null;
    }

    public class MeshFilter : Component
    {
        public Mesh sharedMesh { get; set; }
        public Mesh mesh { get; set; }
    }

    public class Renderer : Component
    {
        public bool enabled { get; set; } = true;
        public Material sharedMaterial { get; set; }
        public Material[] sharedMaterials { get; set; } = new Material[0];
    }

    public class MeshRenderer : Renderer { }

    public enum CameraClearFlags { Skybox = 1, Color = 2, SolidColor = 2, Depth = 3, Nothing = 4 }

    public class Camera : Behaviour
    {
        public static Camera main => null;
        public CameraClearFlags clearFlags { get; set; }
        public Color backgroundColor { get; set; }
        public float fieldOfView { get; set; } = 60f;
        public float nearClipPlane { get; set; } = 0.3f;
        public float farClipPlane { get; set; } = 1000f;
        public Vector3 WorldToScreenPoint(Vector3 position) => position;
        public Ray ScreenPointToRay(Vector3 position) => new Ray(position, Vector3.forward);
    }

    public enum LightType { Spot, Directional, Point }
    public enum LightShadows { None, Hard, Soft }

    public class Light : Behaviour
    {
        public LightType type { get; set; }
        public Color color { get; set; }
        public float intensity { get; set; } = 1f;
        public LightShadows shadows { get; set; }
    }

    public static class RenderSettings
    {
        public static Material skybox { get; set; }
        public static Rendering.AmbientMode ambientMode { get; set; }
        public static Color ambientLight { get; set; }
    }

    public static class Screen
    {
        public static int width { get; set; } = 1920;
        public static int height { get; set; } = 1080;
    }

    public static class QualitySettings
    {
        public static string[] names { get; } = { "Low", "Medium", "High" };
        public static int GetQualityLevel() => 0;
        public static void SetQualityLevel(int index, bool applyExpensiveChanges = true) { }
        public static Rendering.RenderPipelineAsset GetRenderPipelineAssetAt(int index) => null;
        public static Rendering.RenderPipelineAsset renderPipeline { get; set; }
    }

    public sealed class GUIStyle { }

    public sealed class GUILayoutOption { }

    public static class GUI
    {
        public delegate void WindowFunction(int id);
        public static void Label(Rect position, string text) { }
        public static void DrawTexture(Rect position, Texture image) { }
        public static void DragWindow() { }
        public static bool Button(Rect position, string text) => false;
        public static Color color { get; set; }
    }

    public static class GUILayout
    {
        public static Rect Window(int id, Rect screenRect, GUI.WindowFunction func, string text, params GUILayoutOption[] options) => screenRect;
        public static void Label(string text, params GUILayoutOption[] options) { }
        public static bool Button(string text, params GUILayoutOption[] options) => false;
        public static bool Toggle(bool value, string text, params GUILayoutOption[] options) => value;
        public static float HorizontalSlider(float value, float leftValue, float rightValue, params GUILayoutOption[] options) => value;
        public static void BeginHorizontal(params GUILayoutOption[] options) { }
        public static void EndHorizontal() { }
        public static void BeginVertical(params GUILayoutOption[] options) { }
        public static void EndVertical() { }
        public static void Space(float pixels) { }
        public static GUILayoutOption Width(float width) => new GUILayoutOption();
    }

    public static class GUIUtility
    {
        public static int hotControl { get; set; }
    }

    public enum KeyCode { None = 0, Space = 32, Period = 46, G = 103, H = 104, P = 112, R = 114 }

    public static class Input
    {
        public static Vector3 mousePosition => Vector3.zero;
        public static Vector2 mouseScrollDelta => Vector2.zero;
        public static float GetAxisRaw(string axisName) => 0f;
        public static bool GetMouseButtonDown(int button) => false;
        public static bool GetMouseButton(int button) => false;
        public static bool GetKeyDown(KeyCode key) => false;
    }

    public class AsyncOperation : YieldInstruction
    {
        public bool isDone => true;
    }

    // Attributes
    [AttributeUsage(AttributeTargets.Field)] public sealed class SerializeField : Attribute { }
    [AttributeUsage(AttributeTargets.Field)] public sealed class HeaderAttribute : Attribute { public HeaderAttribute(string header) { } }
    [AttributeUsage(AttributeTargets.Field)] public sealed class TooltipAttribute : Attribute { public TooltipAttribute(string tooltip) { } }
    [AttributeUsage(AttributeTargets.Field)] public sealed class RangeAttribute : Attribute { public RangeAttribute(float min, float max) { } }
    [AttributeUsage(AttributeTargets.Field)] public sealed class HideInInspector : Attribute { }
    [AttributeUsage(AttributeTargets.Field)] public sealed class SpaceAttribute : Attribute { public SpaceAttribute(float height = 8f) { } }
    [AttributeUsage(AttributeTargets.Class)] public sealed class DisallowMultipleComponent : Attribute { }
    [AttributeUsage(AttributeTargets.Class)] public sealed class DefaultExecutionOrder : Attribute { public DefaultExecutionOrder(int order) { } }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class RequireComponent : Attribute { public RequireComponent(Type type) { } }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CreateAssetMenuAttribute : Attribute
    {
        public string menuName { get; set; }
        public string fileName { get; set; }
        public int order { get; set; }
    }

    public enum RuntimeInitializeLoadType { AfterSceneLoad, BeforeSceneLoad, AfterAssembliesLoaded, BeforeSplashScreen, SubsystemRegistration }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RuntimeInitializeOnLoadMethodAttribute : Attribute
    {
        public RuntimeInitializeOnLoadMethodAttribute() { }
        public RuntimeInitializeOnLoadMethodAttribute(RuntimeInitializeLoadType loadType) { }
    }
}

namespace UnityEngine.Rendering
{
    public enum AmbientMode { Skybox = 0, Trilight = 1, Flat = 3, Custom = 4 }

    public abstract class RenderPipelineAsset : ScriptableObject { }

    public static class GraphicsSettings
    {
        public static RenderPipelineAsset defaultRenderPipeline { get; set; }
        public static RenderPipelineAsset currentRenderPipeline => defaultRenderPipeline;
    }
}

namespace UnityEngine.SceneManagement
{
    public struct Scene
    {
        public string name => "";
        public string path => "";
        public bool IsValid() => true;
    }

    public enum LocalPhysicsMode { None = 0, Physics2D = 1, Physics3D = 2 }

    public struct CreateSceneParameters
    {
        public CreateSceneParameters(LocalPhysicsMode physicsMode) { localPhysicsMode = physicsMode; }
        public LocalPhysicsMode localPhysicsMode { get; set; }
    }

    public static class SceneManager
    {
        public static Scene CreateScene(string sceneName) => default;
        public static Scene CreateScene(string sceneName, CreateSceneParameters parameters) => default;
        public static void MoveGameObjectToScene(GameObject go, Scene scene) { }
        public static AsyncOperation UnloadSceneAsync(Scene scene) => new AsyncOperation();
        public static Scene GetActiveScene() => default;
    }
}
