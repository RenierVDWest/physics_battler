using PhysicsBattler.Combat;
using PhysicsBattler.Motion;
using PhysicsBattler.Units;
using UnityEditor;
using UnityEngine;

namespace PhysicsBattler.Editor.Setup
{
    /// <summary>
    /// Creates the tuning assets once: the orc and goblin profiles, the big club and its low swing.
    /// After that they are yours to tune; the setup never overwrites them.
    /// </summary>
    internal static class DataSetup
    {
        public const float GoblinScale = 0.62f;

        public static void Apply()
        {
            Ensure<UnitProfile>(BattlerPaths.OrcProfile, _ => { });
            Ensure<UnitProfile>(BattlerPaths.GoblinProfile, goblin =>
            {
                goblin.scale = GoblinScale;
                goblin.totalMassKg = 120f * GoblinScale * GoblinScale * GoblinScale;
                goblin.poiseResistance = 0.45f;
                goblin.maxHealth = 45f;
            });

            var rig = RigFileLoader.LoadOrc();
            Ensure<WeaponDefinition>(BattlerPaths.ClubDefinition, club =>
            {
                if (rig.weapon != null)
                {
                    club.physicsMassKg = rig.weapon.physicsMassKg > 0f ? rig.weapon.physicsMassKg : club.physicsMassKg;
                    club.gameplayMassKg = rig.weapon.gameplayMassKg > 0f ? rig.weapon.gameplayMassKg : club.gameplayMassKg;
                }
            });
            Ensure<AttackDefinition>(BattlerPaths.ClubSwing, _ => { });
        }

        private static T Ensure<T>(string path, System.Action<T> initialize) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            EditorFolders.EnsureParent(path);
            asset = ScriptableObject.CreateInstance<T>();
            initialize(asset);
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }
}
