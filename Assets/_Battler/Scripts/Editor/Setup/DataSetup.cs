using System.IO;
using PhysicsBattler.Motion;
using PhysicsBattler.Units;
using UnityEditor;
using UnityEngine;

namespace PhysicsBattler.Editor.Setup
{
    /// <summary>
    /// Creates the tuning assets once: a unit profile and a combat style per body size. After that
    /// they are yours to tune; setup never changes an existing asset.
    ///
    /// The three styles differ on purpose, to see how idle movement plays: the small unit weaves
    /// and bounces and shuffles a lot, the medium one moves like a boxer, the large one barely sways
    /// and stands wide.
    /// </summary>
    internal static class DataSetup
    {
        public static void Apply()
        {
            var small = Ensure<CombatStyle>(BattlerPaths.Style("Small"), s =>
            {
                s.stanceWidth = 0.36f;
                s.stanceLength = 0.46f;
                s.crouch = 0.1f;
                s.lean = 10f;
                s.swayAmplitude = 0.05f;
                s.swayFrequency = 0.7f;
                s.bobAmplitude = 0.025f;
                s.bobFrequency = 2.2f;
                s.weaveDegrees = 7f;
                s.weaveFrequency = 0.6f;
                s.shuffleInterval = 1.4f;
                s.shuffleDistance = 0.12f;
                s.guardRaise = -40f;
                s.guardElbow = -110f;
            });
            var medium = Ensure<CombatStyle>(BattlerPaths.Style("Medium"), _ => { });
            var large = Ensure<CombatStyle>(BattlerPaths.Style("Large"), s =>
            {
                s.stanceWidth = 0.4f;
                s.stanceLength = 0.36f;
                s.crouch = 0.05f;
                s.lean = 4f;
                s.hipTurn = 14f;
                s.swayAmplitude = 0.018f;
                s.swayFrequency = 0.28f;
                s.bobAmplitude = 0.004f;
                s.bobFrequency = 0.9f;
                s.weaveDegrees = 2.5f;
                s.weaveFrequency = 0.22f;
                s.shuffleInterval = 5f;
                s.shuffleDistance = 0.07f;
                s.guardRaise = -22f;
                s.guardIn = 8f;
                s.guardElbow = -80f;
                s.leadArmReach = -6f;
            });

            Ensure<UnitProfile>(BattlerPaths.Profile("Small"), p =>
            {
                p.body = "Small";
                p.style = small;
                p.turnSpeed = 300f;
                p.armPin = 0.75f;
                p.maxHealth = SmallHealth;
                p.maxPoise = SmallPoise;
            });
            Ensure<UnitProfile>(BattlerPaths.Profile("Medium"), p =>
            {
                p.body = "Medium";
                p.style = medium;
            });
            Ensure<UnitProfile>(BattlerPaths.Profile("Large"), p =>
            {
                p.body = "Large";
                p.style = large;
                p.turnSpeed = 140f;
                p.armPin = 0.6f;
                p.torsoPin = 0.8f;
                p.maxHealth = LargeHealth;
                p.maxPoise = LargePoise;
            });
        }

        // Health and poise by size (the medium unit has the UnitProfile defaults, 100 and 45).
        private const float SmallHealth = 60f, SmallPoise = 25f, LargeHealth = 220f, LargePoise = 120f;

        /// <summary>
        /// Setup version 9 is the animation-first rebuild: the old orc, goblin, club, attack and
        /// sandbox assets are gone from the project (they live on the backup/active-ragdoll-v1
        /// branch). Anything left of them locally is removed.
        /// </summary>
        public static void Migrate(int from)
        {
            // Setup 13 (milestone 3) gives units health and poise: profiles made earlier have the
            // medium figures, so the small and large ones get theirs (unless already changed).
            if (from >= 9 && from < 13)
            {
                SetHealth("Small", SmallHealth, SmallPoise);
                SetHealth("Large", LargeHealth, LargePoise);
            }

            if (from >= 1 && from < 9)
            {
                foreach (var path in new[]
                         {
                             BattlerPaths.OldSandboxScene,
                             BattlerPaths.DataFolder + "/Units/UP_Orc.asset", BattlerPaths.DataFolder + "/Units/UP_Goblin.asset",
                             BattlerPaths.DataFolder + "/Attacks", BattlerPaths.DataFolder + "/Weapons",
                             BattlerPaths.PrefabsFolder + "/Units/P_Orc.prefab", BattlerPaths.PrefabsFolder + "/Units/P_Goblin.prefab",
                             BattlerPaths.PrefabsFolder + "/Weapons", BattlerPaths.PrefabsFolder + "/Props",
                             BattlerPaths.Root + "/Art/Orc", BattlerPaths.RigsFolder + "/orc_rig.json",
                         })
                {
                    if (File.Exists(path) || Directory.Exists(path))
                    {
                        AssetDatabase.DeleteAsset(path);
                        Debug.Log($"[Battler Setup] Removed {path} (the first build; it's on the backup/active-ragdoll-v1 branch).");
                    }
                }
            }
        }

        private static void SetHealth(string body, float health, float poise)
        {
            var profile = AssetDatabase.LoadAssetAtPath<UnitProfile>(BattlerPaths.Profile(body));
            var defaults = ScriptableObject.CreateInstance<UnitProfile>();
            if (profile != null && Mathf.Approximately(profile.maxHealth, defaults.maxHealth) && Mathf.Approximately(profile.maxPoise, defaults.maxPoise))
            {
                profile.maxHealth = health;
                profile.maxPoise = poise;
                EditorUtility.SetDirty(profile);
                Debug.Log($"[Battler Setup] {BattlerPaths.Profile(body)}: health {health}, poise {poise}.");
            }

            Object.DestroyImmediate(defaults);
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
