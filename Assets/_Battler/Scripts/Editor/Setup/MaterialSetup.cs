using System;
using System.Collections.Generic;
using PhysicsBattler.Rig;
using UnityEditor;
using UnityEngine;

namespace PhysicsBattler.Editor.Setup
{
    /// <summary>The project's materials by name (M_Orc_Skin, M_Club_Wood, M_Ground...).</summary>
    internal sealed class MaterialSet
    {
        private readonly Dictionary<string, Material> byName = new Dictionary<string, Material>();

        public void Add(Material material) => byName[material.name] = material;

        public bool TryGet(string name, out Material material) => byName.TryGetValue(name, out material);

        public Material Get(string name) => byName.TryGetValue(name, out var material) ? material : null;

        public IEnumerable<Material> All => byName.Values;
    }

    /// <summary>
    /// Creates a URP Lit material for every colour in the rig file (the orc and club palette) plus the
    /// sandbox's ground and props. Existing materials keep whatever you changed on them.
    /// </summary>
    internal static class MaterialSetup
    {
        private const string LitShader = "Universal Render Pipeline/Lit";

        public static MaterialSet Apply()
        {
            EditorFolders.Ensure(BattlerPaths.MaterialsFolder);
            var set = new MaterialSet();
            var rig = RigFileLoader.LoadOrc();
            foreach (var entry in rig.materials ?? Array.Empty<MaterialEntry>())
            {
                bool metal = entry.name.Contains("Iron");
                set.Add(Ensure($"{BattlerPaths.MaterialsFolder}/{entry.name}.mat", entry.Color, metal ? 0.55f : 0.2f, metal ? 0.8f : 0f));
            }

            set.Add(Ensure(BattlerPaths.GroundMaterial, new Color(0.46f, 0.47f, 0.43f), 0.1f, 0f));
            set.Add(Ensure(BattlerPaths.CrateMaterial, new Color(0.55f, 0.38f, 0.2f), 0.15f, 0f));
            set.Add(Ensure(BattlerPaths.BarrelMaterial, new Color(0.42f, 0.27f, 0.15f), 0.2f, 0f));
            return set;
        }

        private static Material Ensure(string path, Color color, float smoothness, float metallic)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                return material;
            }

            var shader = Shader.Find(LitShader);
            if (shader == null)
            {
                throw new InvalidOperationException($"Shader '{LitShader}' not found. Is the URP package installed?");
            }

            material = new Material(shader) { name = System.IO.Path.GetFileNameWithoutExtension(path) };
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", smoothness);
            material.SetFloat("_Metallic", metallic);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }
    }

    internal static class RigFileLoader
    {
        public static RigFile LoadOrc()
        {
            var text = AssetDatabase.LoadAssetAtPath<TextAsset>(BattlerPaths.OrcRigFile);
            if (text == null)
            {
                throw new InvalidOperationException($"Rig file missing at {BattlerPaths.OrcRigFile}. " +
                                                    "Rebuild it with SourceArt/Orc/Scripts/build_orc.py.");
            }

            return RigFile.Parse(text.text);
        }
    }
}
