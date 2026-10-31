using PhysicsBattler.Combat;
using PhysicsBattler.Motion;
using PhysicsBattler.Units;
using UnityEditor;
using UnityEngine;

namespace PhysicsBattler.Editor.Setup
{
    /// <summary>
    /// Creates the tuning assets once: the orc and goblin profiles, the big club and its low swing.
    /// After that they are yours to tune. The only changes the setup makes to existing assets are
    /// the migrations in <see cref="Migrate"/>, each run once, and only to values still at the old default.
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

        /// <summary>Updates existing tuning assets from setup version <paramref name="from"/> to the current one.</summary>
        public static void Migrate(int from)
        {
            if (from >= 1 && from < 3)
            {
                MigrateTo3();
            }

            if (from >= 1 && from < 2)
            {
                // After the first playtest (the club too heavy for the arm, get-ups declared done while
                // the orc hung face down). New fields arrive with their defaults by themselves.
                foreach (var path in new[] { BattlerPaths.OrcProfile, BattlerPaths.GoblinProfile })
                {
                    var profile = AssetDatabase.LoadAssetAtPath<UnitProfile>(path);
                    if (profile == null)
                    {
                        continue;
                    }

                    bool changed = false;
                    changed |= Replace(ref profile.maxSubtreeBoost, 25f, 300f, profile.name, "maxSubtreeBoost");
                    changed |= Replace(ref profile.getUpDuration, 1.3f, 1.8f, profile.name, "getUpDuration");
                    if (changed)
                    {
                        EditorUtility.SetDirty(profile);
                    }
                }
            }
        }

        private static void MigrateTo3()
        {
            // After the second playtest: the units hung from the balance assist like puppets. The legs
            // now carry the weight (UnitProfile.legSupport), so the assist can let go a lot.
            foreach (var path in new[] { BattlerPaths.OrcProfile, BattlerPaths.GoblinProfile })
            {
                var profile = AssetDatabase.LoadAssetAtPath<UnitProfile>(path);
                if (profile == null)
                {
                    continue;
                }

                bool changed = false;
                changed |= Replace(ref profile.supportFeedForward, 0.55f, 0.2f, profile.name, "supportFeedForward");
                changed |= Replace(ref profile.kpVertical, 160f, 90f, profile.name, "kpVertical");
                changed |= Replace(ref profile.kdVertical, 24f, 16f, profile.name, "kdVertical");
                changed |= Replace(ref profile.angularDamping, 0.6f, 0.1f, profile.name, "angularDamping");
                changed |= Replace(ref profile.uprightBodyShare, 0.3f, 0.2f, profile.name, "uprightBodyShare");
                if (changed)
                {
                    EditorUtility.SetDirty(profile);
                }
            }
        }

        private static bool Replace(ref float field, float oldDefault, float newValue, string asset, string name)
        {
            if (!Mathf.Approximately(field, oldDefault))
            {
                Debug.Log($"[Battler Setup] {asset}.{name} is {field} (tuned by you), left as it is; the new default is {newValue}.");
                return false;
            }

            field = newValue;
            Debug.Log($"[Battler Setup] {asset}.{name}: {oldDefault} → {newValue}.");
            return true;
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
