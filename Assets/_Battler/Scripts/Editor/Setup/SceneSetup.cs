using System.IO;
using System.Linq;
using PhysicsBattler.Combat;
using PhysicsBattler.Motion;
using PhysicsBattler.Sandbox;
using PhysicsBattler.Units;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PhysicsBattler.Editor.Setup
{
    /// <summary>
    /// Creates the Sandbox scene (orbit camera, sun, ground, and the SandboxController wired to the
    /// prefabs and data), adds it to the build settings and opens it. An existing scene is kept;
    /// only its controller's empty references are filled in.
    /// </summary>
    internal static class SceneSetup
    {
        /// <summary>
        /// Opens (creating it if needed) the Sandbox scene. Runs right after the prefabs are built in a
        /// throwaway scene, which it replaces without asking.
        /// </summary>
        public static void Apply()
        {
            if (!File.Exists(BattlerPaths.SandboxScene))
            {
                CreateScene();
            }

            var scene = EditorSceneManager.OpenScene(BattlerPaths.SandboxScene, OpenSceneMode.Single);
            var controller = Object.FindAnyObjectByType<SandboxController>();
            if (controller != null && Wire(controller))
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            AddToBuildSettings(BattlerPaths.SandboxScene);
        }

        private static void CreateScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.36f, 0.4f, 0.45f);
            camera.fieldOfView = 40f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 200f;
            cameraObject.AddComponent<UniversalAdditionalCameraData>();
            var orbit = cameraObject.AddComponent<OrbitCamera>();
            orbit.Apply();

            var sunObject = new GameObject("Sun");
            sunObject.transform.rotation = Quaternion.Euler(50f, -40f, 0f);
            var sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.96f, 0.9f);
            sun.intensity = 1.3f;
            sun.shadows = LightShadows.Soft;
            sunObject.AddComponent<UniversalAdditionalLightData>();

            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.55f, 0.57f, 0.6f);

            // A thick box rather than a plane: no tunnelling through a zero-thickness floor.
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            ground.layer = Layers.Index(Layers.Ground);
            ground.transform.position = new Vector3(0f, -0.5f, 0f);
            ground.transform.localScale = new Vector3(40f, 1f, 40f);
            var groundMaterial = AssetDatabase.LoadAssetAtPath<Material>(BattlerPaths.GroundMaterial);
            if (groundMaterial != null)
            {
                ground.GetComponent<MeshRenderer>().sharedMaterial = groundMaterial;
            }

            var sandbox = new GameObject("Sandbox").AddComponent<SandboxController>();
            sandbox.orbitCamera = orbit;
            Wire(sandbox);

            EditorFolders.Ensure(BattlerPaths.ScenesFolder);
            EditorSceneManager.SaveScene(scene, BattlerPaths.SandboxScene);
        }

        /// <summary>Fills any empty reference on the controller. Returns true if something changed.</summary>
        private static bool Wire(SandboxController sandbox)
        {
            bool changed = false;
            changed |= Fill(ref sandbox.orcPrefab, LoadComponent<UnitController>(BattlerPaths.OrcPrefab));
            changed |= Fill(ref sandbox.goblinPrefab, LoadComponent<UnitController>(BattlerPaths.GoblinPrefab));
            changed |= Fill(ref sandbox.clubPrefab, LoadComponent<Weapon>(BattlerPaths.ClubPrefab));
            changed |= Fill(ref sandbox.attack, AssetDatabase.LoadAssetAtPath<AttackDefinition>(BattlerPaths.ClubSwing));
            changed |= Fill(ref sandbox.cratePrefab, AssetDatabase.LoadAssetAtPath<GameObject>(BattlerPaths.CratePrefab));
            changed |= Fill(ref sandbox.barrelPrefab, AssetDatabase.LoadAssetAtPath<GameObject>(BattlerPaths.BarrelPrefab));
            if (sandbox.orbitCamera == null)
            {
                sandbox.orbitCamera = Object.FindAnyObjectByType<OrbitCamera>();
                changed |= sandbox.orbitCamera != null;
            }

            if (changed)
            {
                EditorUtility.SetDirty(sandbox);
            }

            return changed;
        }

        private static T LoadComponent<T>(string prefabPath) where T : Component
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            return prefab != null ? prefab.GetComponent<T>() : null;
        }

        private static bool Fill<T>(ref T field, T value) where T : Object
        {
            if (field != null || value == null)
            {
                return false;
            }

            field = value;
            return true;
        }

        private static void AddToBuildSettings(string scenePath)
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.Any(scene => scene.path == scenePath))
            {
                return;
            }

            scenes.Insert(0, new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
