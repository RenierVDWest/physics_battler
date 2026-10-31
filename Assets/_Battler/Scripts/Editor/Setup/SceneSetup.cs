using System.IO;
using System.Linq;
using PhysicsBattler.Board;
using PhysicsBattler.Combat;
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
            bool changed = sandbox != null && Wire(sandbox);
            changed |= EnsureLineup();
            changed |= MigrateLineup(SetupVersion.Installed);
            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            // The previews were built before the prefabs and weapons were wired: build them again.
            foreach (var spawn in Object.FindObjectsByType<UnitSpawn>())
            {
                spawn.BuildPreview();
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
            EnsureLineup();

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
                    sandbox.board.Build();   // the tiles were built before they had materials
                    changed = true;
                }
            }

            if (changed)
            {
                EditorUtility.SetDirty(sandbox);
            }

            return changed;
        }

        /// <summary>Team B's bodies in the default lineup, columns 3 to 5 (C to E): each faces a different size.</summary>
        private static readonly BodySize[] TeamBOrder = { BodySize.Medium, BodySize.Large, BodySize.Small };

        /// <summary>
        /// A lineup for a scene that has none: three units a side three rows apart (team A small,
        /// medium, large; team B medium, large, small, so each faces a different size), small and
        /// large with clubs, medium with sword and shield. Move, add or change them in the scene;
        /// setup leaves an existing lineup alone.
        /// </summary>
        private static bool EnsureLineup()
        {
            if (Object.FindObjectsByType<UnitSpawn>().Length > 0)
            {
                return false;
            }

            var board = Object.FindAnyObjectByType<BoardView>();
            var grid = board != null ? board.grid : new BoardGrid();
            var club = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(BattlerPaths.Weapon("Club"));
            var sword = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(BattlerPaths.Weapon("Sword"));
            var shield = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(BattlerPaths.Weapon("Shield"));
            var lineup = new GameObject("Lineup").transform;
            var bodies = new[] { BodySize.Small, BodySize.Medium, BodySize.Large };
            for (int team = 0; team < 2; team++)
            {
                for (int k = 0; k < 3; k++)
                {
                    // Team A on row 3, small to large left to right; team B on row 6, large to small, so
                    // each faces a different size.
                    var body = team == 0 ? bodies[k] : TeamBOrder[k];
                    var tile = new Tile(2 + k, team == 0 ? 2 : 5);
                    var go = new GameObject($"{(team == 0 ? "A" : "B")} {body}");
                    go.transform.SetParent(lineup, false);
                    go.transform.position = grid.Center(tile);
                    var spawn = go.AddComponent<UnitSpawn>();
                    spawn.team = team;
                    Arm(spawn, body, club, sword, shield);
                    spawn.Snap();
                }
            }

            return true;
        }

        private static void Arm(UnitSpawn spawn, BodySize body, WeaponDefinition club, WeaponDefinition sword, WeaponDefinition shield)
        {
            spawn.body = body;
            spawn.mainHand = body == BodySize.Medium ? sword : club;
            spawn.offHand = body == BodySize.Medium ? shield : null;
            spawn.gameObject.name = $"{(spawn.team == 0 ? "A" : "B")} {body}";
        }

        /// <summary>
        /// Setup 10's default lineup put each unit straight across from the same pairing every time
        /// (small against large, medium against medium). If the scene still has that lineup
        /// unchanged, team B is rearranged into the new default order.
        /// </summary>
        private static bool MigrateLineup(int from)
        {
            if (from < 1 || from >= 11)
            {
                return false;
            }

            var spawns = Object.FindObjectsByType<UnitSpawn>();
            var board = Object.FindAnyObjectByType<BoardView>();
            var grid = board != null ? board.grid : new BoardGrid();
            var old = new[] { BodySize.Large, BodySize.Medium, BodySize.Small };
            var teamB = new UnitSpawn[3];
            int count = 0;
            foreach (var spawn in spawns)
            {
                if (spawn.team != 1)
                {
                    continue;
                }

                var tile = grid.TileAt(spawn.transform.position);
                int k = tile.column - 2;
                if (tile.row != 5 || k < 0 || k > 2 || spawn.body != old[k] || teamB[k] != null)
                {
                    return false;   // not the old default: leave it as you made it
                }

                teamB[k] = spawn;
                count++;
            }

            if (count != 3)
            {
                return false;
            }

            var club = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(BattlerPaths.Weapon("Club"));
            var sword = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(BattlerPaths.Weapon("Sword"));
            var shield = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(BattlerPaths.Weapon("Shield"));
            for (int k = 0; k < 3; k++)
            {
                Arm(teamB[k], TeamBOrder[k], club, sword, shield);
                EditorUtility.SetDirty(teamB[k]);
            }

            Debug.Log("[Battler Setup] Team B of the default lineup rearranged (medium, large, small), so each unit faces a different size.");
            return true;
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
