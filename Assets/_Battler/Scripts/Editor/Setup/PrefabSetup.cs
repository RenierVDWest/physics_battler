using System.Collections.Generic;
using PhysicsBattler.Combat;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEditor;
using UnityEngine;

namespace PhysicsBattler.Editor.Setup
{
    /// <summary>
    /// Bakes the generated prefabs: the orc and goblin units (built from orc_rig.json by
    /// <see cref="UnitBuilder"/>), the big club, and the sandbox's crate and barrel. Rebuilt every
    /// run, so tune the units in their profiles rather than on these prefabs.
    /// </summary>
    internal static class PrefabSetup
    {
        public static void Apply(MaterialSet materials)
        {
            var rig = RigFileLoader.LoadOrc();
            var orcProfile = AssetDatabase.LoadAssetAtPath<UnitProfile>(BattlerPaths.OrcProfile);
            var goblinProfile = AssetDatabase.LoadAssetAtPath<UnitProfile>(BattlerPaths.GoblinProfile);
            int bodyLayer = Layers.Index(Layers.Body);

            SaveUnit(UnitBuilder.Build(rig, orcProfile, "P_Orc", LoadVisual, bodyLayer), BattlerPaths.OrcPrefab);
            SaveUnit(UnitBuilder.Build(rig, goblinProfile, "P_Goblin", LoadVisual, bodyLayer), BattlerPaths.GoblinPrefab);
            BuildClub(rig);
            BuildCrate(materials);
            BuildBarrel(materials);
        }

        /// <summary>The mesh and (remapped) materials of a model, with the pose of its mesh node.</summary>
        private static UnitBuilder.Visual? LoadVisual(string meshName)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelImportSetup.ModelPath(meshName));
            if (model == null)
            {
                Debug.LogWarning($"[Battler Setup] Model {meshName} not found in {BattlerPaths.OrcArtFolder}.");
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

        private static void SaveUnit(GameObject unit, string path)
        {
            EditorFolders.EnsureParent(path);
            PrefabUtility.SaveAsPrefabAsset(unit, path);
            Object.DestroyImmediate(unit);
        }

        private static void BuildClub(RigFile rig)
        {
            var entry = rig.weapon;
            if (entry == null)
            {
                Debug.LogWarning("[Battler Setup] The rig file has no weapon; skipping the club.");
                return;
            }

            var root = new GameObject("P_Weapon_ClubBig");
            var weapon = root.AddComponent<Weapon>();
            weapon.definition = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(BattlerPaths.ClubDefinition);

            var visual = LoadVisual(entry.mesh);
            if (visual.HasValue)
            {
                var meshObject = new GameObject("Mesh");
                meshObject.transform.SetParent(root.transform, false);
                meshObject.transform.localPosition = visual.Value.offset;
                meshObject.transform.localRotation = visual.Value.rotation;
                meshObject.transform.localScale = visual.Value.scale;
                meshObject.AddComponent<MeshFilter>().sharedMesh = visual.Value.mesh;
                meshObject.AddComponent<MeshRenderer>().sharedMaterials = visual.Value.materials;
            }

            int weaponLayer = Layers.Index(Layers.Weapon);
            var colliders = new List<Collider>();
            if (entry.colliders != null)
            {
                for (int i = 0; i < entry.colliders.Length; i++)
                {
                    var collider = UnitBuilder.AddCollider(root.transform, entry.colliders[i], 1f, weaponLayer, $"Collider_{i}");
                    if (collider != null)
                    {
                        colliders.Add(collider);
                    }
                }
            }

            weapon.colliders = colliders.ToArray();
            weapon.sweetSpot = Marker(root.transform, "SweetSpot", entry.markers?.sweet_spot);
            weapon.tip = Marker(root.transform, "Tip", entry.markers?.tip);
            Marker(root.transform, "PommelEnd", entry.markers?.pommel_end);

            EditorFolders.EnsureParent(BattlerPaths.ClubPrefab);
            PrefabUtility.SaveAsPrefabAsset(root, BattlerPaths.ClubPrefab);
            Object.DestroyImmediate(root);
        }

        private static Transform Marker(Transform parent, string name, float[] position)
        {
            var marker = new GameObject(name).transform;
            marker.SetParent(parent, false);
            marker.localPosition = RigMath.ToVector(position);
            return marker;
        }

        private static void BuildCrate(MaterialSet materials)
        {
            var crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crate.name = "P_Prop_Crate";
            crate.transform.localScale = Vector3.one * 0.6f;
            FinishProp(crate, materials.Get("M_Prop_Crate"), 20f, BattlerPaths.CratePrefab);
        }

        private static void BuildBarrel(MaterialSet materials)
        {
            // Unity's cylinder is 2 m tall with a 0.5 m radius (and a capsule collider): scale to 0.9 x 0.55.
            var barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            barrel.name = "P_Prop_Barrel";
            barrel.transform.localScale = new Vector3(0.55f, 0.45f, 0.55f);
            FinishProp(barrel, materials.Get("M_Prop_Barrel"), 35f, BattlerPaths.BarrelPrefab);
        }

        private static void FinishProp(GameObject prop, Material material, float mass, string path)
        {
            prop.layer = Layers.Index(Layers.Prop);
            if (material != null)
            {
                prop.GetComponent<MeshRenderer>().sharedMaterial = material;
            }

            var body = prop.AddComponent<Rigidbody>();
            body.mass = mass;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            EditorFolders.EnsureParent(path);
            PrefabUtility.SaveAsPrefabAsset(prop, path);
            Object.DestroyImmediate(prop);
        }
    }
}
