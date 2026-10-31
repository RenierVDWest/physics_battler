using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PhysicsBattler.Editor.Setup
{
    /// <summary>
    /// Builds the three unit prefabs (P_Unit_Small/Medium/Large) from their rig files and part
    /// meshes: a null tree with a rigidbody, collider, joint and mesh per body part. Rebuilt every
    /// setup run, so changes to the rig files arrive by themselves; tune the profiles, not the prefabs.
    /// </summary>
    internal static class PrefabSetup
    {
        public static bool ApplyInScratchScene(MaterialSet materials, out string previousScene)
        {
            previousScene = SceneManager.GetActiveScene().path;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return false;
            }

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            WeaponSetup.Apply();   // the weapon prefabs first: built in this scratch scene too
            Apply();
            return true;
        }

        private static void Apply()
        {
            int bodyLayer = Layers.Index(Layers.Body);
            foreach (var body in BattlerPaths.Bodies)
            {
                var rig = RigFileLoader.Load(body);
                var profile = AssetDatabase.LoadAssetAtPath<UnitProfile>(BattlerPaths.Profile(body));
                var unit = UnitBuilder.Build(rig, profile, $"P_Unit_{body}", LoadVisual, bodyLayer);
                string path = BattlerPaths.UnitPrefab(body);
                EditorFolders.EnsureParent(path);
                PrefabUtility.SaveAsPrefabAsset(unit, path);
                Object.DestroyImmediate(unit);
            }
        }

        private static UnitBuilder.Visual? LoadVisual(string meshName)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelImportSetup.ModelPath(meshName));
            if (model == null)
            {
                Debug.LogWarning($"[Battler Setup] Model {meshName} not found at {ModelImportSetup.ModelPath(meshName)}.");
                return null;
            }

            var filter = model.GetComponentInChildren<MeshFilter>(true);
            var renderer = filter != null ? filter.GetComponent<MeshRenderer>() : null;
            if (filter == null || filter.sharedMesh == null)
            {
                return null;
            }

            var visual = UnitBuilder.Visual.Of(filter.sharedMesh, renderer != null ? renderer.sharedMaterials : new Material[0]);
            visual.offset = filter.transform.position;
            visual.rotation = filter.transform.rotation;
            visual.scale = filter.transform.lossyScale;
            return visual;
        }
    }
}
