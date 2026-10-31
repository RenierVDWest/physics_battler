using System;
using System.Collections.Generic;
using PhysicsBattler.Rig;
using UnityEditor;
using UnityEngine;

namespace PhysicsBattler.Editor.Setup
{
    internal sealed class MaterialSet
    {
        private readonly Dictionary<string, Material> byName = new Dictionary<string, Material>();

        public void Add(Material material) => byName[material.name] = material;

        public bool TryGet(string name, out Material material) => byName.TryGetValue(name, out material);

        public Material Get(string name) => byName.TryGetValue(name, out var material) ? material : null;

        public IEnumerable<Material> All => byName.Values;
    }

    /// <summary>
    /// Creates the materials once (after that they are yours to tune): the body and joint materials
    /// the body meshes use, a colour per team (it replaces the body material on spawn), the board
    /// tiles and the ground.
    /// </summary>
    internal static class MaterialSetup
    {
        private const string LitShader = "Universal Render Pipeline/Lit";

        public static MaterialSet Apply()
        {
            EditorFolders.Ensure(BattlerPaths.MaterialsFolder);
            var set = new MaterialSet();
            var rig = RigFileLoader.Load("Medium");
            foreach (var entry in rig.materials ?? Array.Empty<MaterialEntry>())
            {
                set.Add(Ensure($"{BattlerPaths.MaterialsFolder}/{entry.name}.mat", entry.Color, 0.35f, 0f));
            }

            set.Add(Ensure(BattlerPaths.TeamAMaterial, new Color(0.83f, 0.66f, 0.46f), 0.35f, 0f));
            set.Add(Ensure(BattlerPaths.TeamBMaterial, new Color(0.55f, 0.66f, 0.8f), 0.35f, 0f));
            set.Add(Ensure(BattlerPaths.BoardLightMaterial, new Color(0.58f, 0.57f, 0.52f), 0.1f, 0f));
            set.Add(Ensure(BattlerPaths.BoardDarkMaterial, new Color(0.43f, 0.43f, 0.4f), 0.1f, 0f));
            set.Add(Ensure(BattlerPaths.BoardBorderMaterial, new Color(0.26f, 0.25f, 0.24f), 0.1f, 0f));
            set.Add(Ensure(BattlerPaths.GroundMaterial, new Color(0.34f, 0.35f, 0.33f), 0.1f, 0f));
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
        public static RigFile Load(string body)
        {
            string path = BattlerPaths.RigFile(body);
            var text = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            if (text == null)
            {
                throw new InvalidOperationException($"Rig file missing at {path}. "
                                                    + "Rebuild it with SourceArt/Bodies/Scripts/build_bodies.py.");
            }

            return RigFile.Parse(text.text);
        }
    }
}
