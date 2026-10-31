using System.IO;
using System.Linq;
using PhysicsBattler.Board;
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
    /// The Battle scene: camera, sun, ground, the board and the battle sandbox. Created once; after
    /// that only missing references are filled in, so your changes to the scene are kept.
    /// </summary>
    internal static class SceneSetup
    {
        public static void Apply()
        {
            if (!File.Exists(BattlerPaths.BattleScene))
            {
                CreateScene();
            }

            var scene = EditorSceneManager.OpenScene(BattlerPaths.BattleScene, OpenSceneMode.Single);
            var sandbox = Object.FindAnyObjectByType<BattleSandbox>();
            if (sandbox != null && Wire(sandbox))
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            SetBuildScenes(BattlerPaths.BattleScene);
        }

        private static void CreateScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.25f, 0.27f, 0.3f);
            camera.fieldOfView = 35f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 200f;
            cameraObject.AddComponent<UniversalAdditionalCameraData>();
            var orbit = cameraObject.AddComponent<OrbitCamera>();
            orbit.target = new Vector3(0f, 0.8f, 0f);
            orbit.distance = 20f;
            orbit.yaw = 20f;
            orbit.pitch = 38f;
            orbit.maxDistance = 45f;
            orbit.Apply();

            var sunObject = new GameObject("Sun");
            sunObject.transform.rotation = Quaternion.Euler(52f, -35f, 0f);
            var sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.96f, 0.9f);
            sun.intensity = 1.25f;
            sun.shadows = LightShadows.Soft;
            sunObject.AddComponent<UniversalAdditionalLightData>();

            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.52f, 0.54f, 0.58f);

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

            var board = new GameObject("Board").AddComponent<BoardView>();
            var sandbox = new GameObject("Battle").AddComponent<BattleSandbox>();
            sandbox.orbitCamera = orbit;
            sandbox.board = board;
            Wire(sandbox);

            EditorFolders.Ensure(BattlerPaths.ScenesFolder);
            EditorSceneManager.SaveScene(scene, BattlerPaths.BattleScene);
        }

        private static bool Wire(BattleSandbox sandbox)
        {
            bool changed = false;
            changed |= Fill(ref sandbox.smallPrefab, LoadComponent<UnitController>(BattlerPaths.UnitPrefab("Small")));
            changed |= Fill(ref sandbox.mediumPrefab, LoadComponent<UnitController>(BattlerPaths.UnitPrefab("Medium")));
            changed |= Fill(ref sandbox.largePrefab, LoadComponent<UnitController>(BattlerPaths.UnitPrefab("Large")));
            changed |= Fill(ref sandbox.teamA, AssetDatabase.LoadAssetAtPath<Material>(BattlerPaths.TeamAMaterial));
            changed |= Fill(ref sandbox.teamB, AssetDatabase.LoadAssetAtPath<Material>(BattlerPaths.TeamBMaterial));
            if (sandbox.orbitCamera == null)
            {
                sandbox.orbitCamera = Object.FindAnyObjectByType<OrbitCamera>();
                changed |= sandbox.orbitCamera != null;
            }

            if (sandbox.board == null)
            {
                sandbox.board = Object.FindAnyObjectByType<BoardView>();
                changed |= sandbox.board != null;
            }

            if (sandbox.board != null)
            {
                bool boardChanged = false;
                boardChanged |= Fill(ref sandbox.board.lightTiles, AssetDatabase.LoadAssetAtPath<Material>(BattlerPaths.BoardLightMaterial));
                boardChanged |= Fill(ref sandbox.board.darkTiles, AssetDatabase.LoadAssetAtPath<Material>(BattlerPaths.BoardDarkMaterial));
                boardChanged |= Fill(ref sandbox.board.border, AssetDatabase.LoadAssetAtPath<Material>(BattlerPaths.BoardBorderMaterial));
                if (boardChanged)
                {
                    EditorUtility.SetDirty(sandbox.board);
                    changed = true;
                }
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

        /// <summary>The Battle scene first in the build; the old Sandbox scene out.</summary>
        private static void SetBuildScenes(string scenePath)
        {
            var scenes = EditorBuildSettings.scenes.Where(s => s.path != BattlerPaths.OldSandboxScene).ToList();
            if (!scenes.Any(scene => scene.path == scenePath))
            {
                scenes.Insert(0, new EditorBuildSettingsScene(scenePath, true));
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
