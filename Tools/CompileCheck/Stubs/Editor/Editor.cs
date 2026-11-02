// Compile-check stubs for the UnityEditor members the project setup uses.
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityEditor
{
    [AttributeUsage(AttributeTargets.Class)] public sealed class InitializeOnLoadAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class MenuItem : Attribute
    {
        public MenuItem(string itemName) { }
        public MenuItem(string itemName, bool isValidateFunction) { }
        public MenuItem(string itemName, bool isValidateFunction, int priority) { }
        public int priority { get; set; }
    }

    public static class EditorApplication
    {
        public delegate void CallbackFunction();
        public static CallbackFunction delayCall;
        public static bool isCompiling => false;
        public static bool isUpdating => false;
        public static bool isPlaying => false;
        public static bool isPlayingOrWillChangePlaymode => false;
    }

    public static class EditorUtility
    {
        public static void SetDirty(UnityEngine.Object target) { }
        public static bool DisplayDialog(string title, string message, string ok) => true;
        public static void CopySerialized(UnityEngine.Object source, UnityEngine.Object dest) { }
    }

    public static class AssetDatabase
    {
        public static T LoadAssetAtPath<T>(string assetPath) where T : UnityEngine.Object => null;
        public static UnityEngine.Object[] LoadAllAssetsAtPath(string assetPath) => new UnityEngine.Object[1];
        public static bool IsValidFolder(string path) => true;
        public static bool DeleteAsset(string path) => true;
        public static string CreateFolder(string parentFolder, string newFolderName) => "";
        public static void CreateAsset(UnityEngine.Object asset, string path) { }
        public static void SaveAssets() { }
        public static void Refresh() { }
        public static string[] FindAssets(string filter, string[] searchInFolders) => new string[0];
        public static string GUIDToAssetPath(string guid) => "";
        public static string GetAssetPath(UnityEngine.Object assetObject) => "";
        public static void ImportAsset(string path) { }
    }

    public class SerializedProperty
    {
        public int intValue { get; set; }
        public float floatValue { get; set; }
        public bool boolValue { get; set; }
        public string stringValue { get; set; }
        public int arraySize { get; set; }
        public SerializedProperty GetArrayElementAtIndex(int index) => new SerializedProperty();
    }

    public class SerializedObject
    {
        public SerializedObject(UnityEngine.Object obj) { }
        public SerializedProperty FindProperty(string propertyPath) => new SerializedProperty();
        public bool ApplyModifiedPropertiesWithoutUndo() => true;
        public bool ApplyModifiedProperties() => true;
    }

    public static class PrefabUtility
    {
        public static GameObject SaveAsPrefabAsset(GameObject instanceRoot, string assetPath) => instanceRoot;
        public static UnityEngine.Object InstantiatePrefab(UnityEngine.Object assetComponentOrGameObject) => assetComponentOrGameObject;
        public static void UnpackPrefabInstance(GameObject instanceRoot, PrefabUnpackMode unpackMode, InteractionMode action) { }
    }

    public class AssetImporter : UnityEngine.Object
    {
        public struct SourceAssetIdentifier
        {
            public SourceAssetIdentifier(Type type, string name) { this.type = type; this.name = name; }
            public Type type;
            public string name;
        }

        public static AssetImporter GetAtPath(string path) => null;
        public string assetPath => "";
        public Dictionary<SourceAssetIdentifier, UnityEngine.Object> GetExternalObjectMap() => new Dictionary<SourceAssetIdentifier, UnityEngine.Object>();
        public void AddRemap(SourceAssetIdentifier identifier, UnityEngine.Object externalObject) { }
        public bool RemoveRemap(SourceAssetIdentifier identifier) => true;
        public void SaveAndReimport() { }
    }

    public enum ModelImporterAnimationType { None = 0, Legacy = 1, Generic = 2, Human = 3 }
    public enum ModelImporterMaterialImportMode { None = 0, ImportStandard = 1, ImportViaMaterialDescription = 2 }

    public class ModelImporter : AssetImporter
    {
        public bool importCameras { get; set; }
        public bool importLights { get; set; }
        public bool importVisibility { get; set; }
        public bool importBlendShapes { get; set; }
        public bool importAnimation { get; set; }
        public bool isReadable { get; set; }
        public bool useFileScale { get; set; }
        public float globalScale { get; set; } = 1f;
        public ModelImporterAnimationType animationType { get; set; }
        public ModelImporterMaterialImportMode materialImportMode { get; set; }
    }

    public class EditorBuildSettingsScene
    {
        public EditorBuildSettingsScene(string path, bool enabled) { this.path = path; this.enabled = enabled; }
        public string path { get; set; }
        public bool enabled { get; set; }
    }

    public static class EditorBuildSettings
    {
        public static EditorBuildSettingsScene[] scenes { get; set; } = new EditorBuildSettingsScene[0];
    }
}

namespace UnityEditor.SceneManagement
{
    public enum OpenSceneMode { Single, Additive, AdditiveWithoutLoading }
    public enum NewSceneSetup { EmptyScene, DefaultGameObjects }
    public enum NewSceneMode { Single, Additive }

    public static class EditorSceneManager
    {
        public static bool SaveCurrentModifiedScenesIfUserWantsTo() => true;
        public static Scene OpenScene(string scenePath, OpenSceneMode mode = OpenSceneMode.Single) => default;
        public static Scene NewScene(NewSceneSetup setup, NewSceneMode mode = NewSceneMode.Single) => default;
        public static bool MarkSceneDirty(Scene scene) => true;
        public static bool SaveScene(Scene scene, string dstScenePath = "", bool saveAsCopy = false) => true;
    }
}

namespace UnityEditor
{
    // Editor windows and Scene view drawing, as far as the project uses them.
    public class EditorWindow : UnityEngine.ScriptableObject
    {
        public static T GetWindow<T>(string title) where T : EditorWindow => UnityEngine.ScriptableObject.CreateInstance<T>();
        public void Show() { }
        public void Repaint() { }
    }

    public enum MessageType { None, Info, Warning, Error }

    public static class EditorStyles
    {
        public static UnityEngine.GUIStyle boldLabel => new UnityEngine.GUIStyle();
    }

    public static class EditorGUI
    {
        public static void BeginChangeCheck() { }
        public static bool EndChangeCheck() => false;
    }

    public static class EditorGUILayout
    {
        public static void LabelField(string label, UnityEngine.GUIStyle style, params UnityEngine.GUILayoutOption[] options) { }
        public static void LabelField(string label, params UnityEngine.GUILayoutOption[] options) { }
        public static System.Enum EnumPopup(string label, System.Enum selected, params UnityEngine.GUILayoutOption[] options) => selected;
        public static UnityEngine.Object ObjectField(string label, UnityEngine.Object obj, System.Type objType, bool allowSceneObjects, params UnityEngine.GUILayoutOption[] options) => obj;
        public static int Popup(string label, int selectedIndex, string[] displayedOptions, params UnityEngine.GUILayoutOption[] options) => selectedIndex;
        public static float Slider(string label, float value, float leftValue, float rightValue, params UnityEngine.GUILayoutOption[] options) => value;
        public static bool Toggle(string label, bool value, params UnityEngine.GUILayoutOption[] options) => value;
        public static void HelpBox(string message, MessageType type) { }
        public static UnityEngine.Vector3 Vector3Field(string label, UnityEngine.Vector3 value, params UnityEngine.GUILayoutOption[] options) => value;
        public static void Space() { }

        public sealed class HorizontalScope : System.IDisposable
        {
            public HorizontalScope(params UnityEngine.GUILayoutOption[] options) { }
            public void Dispose() { }
        }
    }

    public class SceneView : EditorWindow
    {
        public static event System.Action<SceneView> duringSceneGui;
        public static SceneView lastActiveSceneView => null;
        public static void RepaintAll() { duringSceneGui?.Invoke(null); }
        public void LookAt(UnityEngine.Vector3 point, UnityEngine.Quaternion direction, float newSize) { }
    }

    public static class Handles
    {
        public static UnityEngine.Color color { get; set; }
        public static void DrawLine(UnityEngine.Vector3 p1, UnityEngine.Vector3 p2, float thickness) { }
        public static void SphereHandleCap(int controlID, UnityEngine.Vector3 position, UnityEngine.Quaternion rotation, float size, UnityEngine.EventType eventType) { }
        public static void Label(UnityEngine.Vector3 position, string text) { }
        public static UnityEngine.Vector3 PositionHandle(UnityEngine.Vector3 position, UnityEngine.Quaternion rotation) => position;
        public static void DrawWireDisc(UnityEngine.Vector3 center, UnityEngine.Vector3 normal, float radius) { }
        public static void DrawDottedLine(UnityEngine.Vector3 p1, UnityEngine.Vector3 p2, float screenSpaceSize) { }
    }

    public static class Undo
    {
        public static void RecordObject(UnityEngine.Object objectToUndo, string name) { }
    }

    public enum PrefabUnpackMode { OutermostRoot, Completely }
    public enum InteractionMode { AutomatedAction, UserAction }
}
