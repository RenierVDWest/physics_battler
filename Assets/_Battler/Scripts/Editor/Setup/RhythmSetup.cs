using System;
using PhysicsBattler.Motion;
using UnityEditor;
using UnityEngine;

namespace PhysicsBattler.Editor.Setup
{
    /// <summary>
    /// The Fight Rhythms: a rhythm asset per ready-made style (Data/Rhythms/RD_*) and a posture asset
    /// per posture (Data/Rhythms/Postures/PD_*), and each body's combat style pointed at its style
    /// (large: boxing, medium: shield and sword, small: Eskrima) if it has none. Created once, then
    /// yours to tune.
    /// </summary>
    internal static class RhythmSetup
    {
        public static void Apply()
        {
            foreach (RhythmStyle style in Enum.GetValues(typeof(RhythmStyle)))
            {
                var rhythm = EnsureRhythm(style);
                string body = style == RhythmStyle.Boxing ? "Large" : style == RhythmStyle.Eskrima ? "Small" : "Medium";
                var combat = AssetDatabase.LoadAssetAtPath<CombatStyle>(BattlerPaths.Style(body));
                if (combat != null && combat.rhythm == null)
                {
                    combat.rhythm = rhythm;
                    EditorUtility.SetDirty(combat);
                    Debug.Log($"[Battler Setup] {BattlerPaths.Style(body)}: Fight Rhythm {rhythm.name}.");
                }
            }
        }

        private static RhythmDefinition EnsureRhythm(RhythmStyle style)
        {
            string path = BattlerPaths.Rhythm(style.ToString());
            var rhythm = AssetDatabase.LoadAssetAtPath<RhythmDefinition>(path);
            if (rhythm != null)
            {
                return rhythm;
            }

            EditorFolders.EnsureParent(path);
            rhythm = RhythmDefinition.Create(style);
            for (int i = 0; i < rhythm.postures.Length; i++)
            {
                var made = rhythm.postures[i];
                string posturePath = BattlerPaths.Posture(made.Label);
                var existing = AssetDatabase.LoadAssetAtPath<PostureDefinition>(posturePath);
                if (existing != null)
                {
                    rhythm.postures[i] = existing;
                    UnityEngine.Object.DestroyImmediate(made);
                    continue;
                }

                EditorFolders.EnsureParent(posturePath);
                AssetDatabase.CreateAsset(made, posturePath);
            }

            AssetDatabase.CreateAsset(rhythm, path);
            return rhythm;
        }
    }
}
