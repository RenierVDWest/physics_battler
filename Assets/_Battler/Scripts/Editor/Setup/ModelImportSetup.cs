using System.IO;
using UnityEditor;
using UnityEngine;

namespace PhysicsBattler.Editor.Setup
{
    /// <summary>
    /// Import settings for the body part FBX files (from SourceArt/Bodies/Scripts/build_bodies.py):
    /// static meshes only, scale 1, and their materials replaced by the project's, matched by name.
    /// </summary>
    internal static class ModelImportSetup
    {
        public static void Apply(MaterialSet materials)
        {
            if (!AssetDatabase.IsValidFolder(BattlerPaths.BodiesArtFolder))
            {
                throw new System.InvalidOperationException($"Art folder missing: {BattlerPaths.BodiesArtFolder}");
            }

            foreach (var guid in AssetDatabase.FindAssets("t:Model", new[] { BattlerPaths.BodiesArtFolder }))
            {
                Configure(AssetDatabase.GUIDToAssetPath(guid), materials);
            }
        }

        public static void Configure(string path, MaterialSet materials)
        {
            if (!(AssetImporter.GetAtPath(path) is ModelImporter importer))
            {
                return;
            }

            bool changed = false;
            changed |= Set(importer.importCameras, false, v => importer.importCameras = v);
            changed |= Set(importer.importLights, false, v => importer.importLights = v);
            changed |= Set(importer.importVisibility, false, v => importer.importVisibility = v);
            changed |= Set(importer.importBlendShapes, false, v => importer.importBlendShapes = v);
            changed |= Set(importer.importAnimation, false, v => importer.importAnimation = v);
            changed |= Set(importer.isReadable, false, v => importer.isReadable = v);
            changed |= Set(importer.useFileScale, true, v => importer.useFileScale = v);
            if (!Mathf.Approximately(importer.globalScale, 1f))
            {
                importer.globalScale = 1f;
                changed = true;
            }

            if (importer.animationType != ModelImporterAnimationType.None)
            {
                importer.animationType = ModelImporterAnimationType.None;
                changed = true;
            }

            if (importer.materialImportMode != ModelImporterMaterialImportMode.ImportViaMaterialDescription)
            {
                importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
                changed = true;
            }

            var remaps = importer.GetExternalObjectMap();
            foreach (var material in materials.All)
            {
                var id = new AssetImporter.SourceAssetIdentifier(typeof(Material), material.name);
                if (!remaps.TryGetValue(id, out var current) || current != material)
                {
                    importer.AddRemap(id, material);
                    changed = true;
                }
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }
        }

        private static bool Set(bool current, bool wanted, System.Action<bool> apply)
        {
            if (current == wanted)
            {
                return false;
            }

            apply(wanted);
            return true;
        }

        /// <summary>SM_Medium_Chest lives in Art/Bodies/Medium/.</summary>
        public static string ModelPath(string meshName)
        {
            var parts = meshName.Split('_');
            string body = parts.Length > 1 ? parts[1] : "";
            return $"{BattlerPaths.BodiesArtFolder}/{body}/{meshName}.fbx";
        }

        public static bool Exists(string meshName) => File.Exists(ModelPath(meshName));
    }
}
