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
    /// The three styles differ on purpose (<see cref="CombatStyle.CreateFor"/>): the small unit
    /// weaves and bounces and shuffles a lot, the medium one has the plain defaults, the large one
    /// stands as the captured boxing idle its Fight Rhythm plays.
    /// </summary>
    internal static class DataSetup
    {
        public static void Apply()
        {
            var small = EnsureStyle("Small");
            var medium = EnsureStyle("Medium");
            var large = EnsureStyle("Large");

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
            // Setup 17: the medium and small units stand as their captured idles do (unless their
            // stance was already changed), and every body walks with its captured walk.
            if (from >= 9 && from < 17)
            {
                foreach (string body in new[] { "Small", "Medium", "Large" })
                {
                    var style = AssetDatabase.LoadAssetAtPath<CombatStyle>(BattlerPaths.Style(body));
                    if (style == null)
                    {
                        continue;
                    }

                    if (body == "Medium" && CombatStyle.HasOldMediumStance(style))
                    {
                        CombatStyle.ApplyMediumStance(style);
                        Debug.Log($"[Battler Setup] {BattlerPaths.Style(body)}: the stance from the captured idle.");
                    }
                    else if (body == "Small" && CombatStyle.HasOldSmallStance(style))
                    {
                        CombatStyle.ApplySmallStance(style);
                        Debug.Log($"[Battler Setup] {BattlerPaths.Style(body)}: the stance from the captured idle.");
                    }

                    if (style.walk == null || style.walk.IsEmpty)
                    {
                        style.walk = body == "Small" ? RhythmClips.SmallWalk() : body == "Medium" ? RhythmClips.MediumWalk() : RhythmClips.LargeWalk();
                        Debug.Log($"[Battler Setup] {BattlerPaths.Style(body)}: walks with {style.walk.source}.");
                    }

                    EditorUtility.SetDirty(style);
                }
            }

            // Setup 16: the large unit stands as the captured boxing idle it now plays does (unless
            // its stance was already changed).
            if (from >= 9 && from < 16)
            {
                var large = AssetDatabase.LoadAssetAtPath<CombatStyle>(BattlerPaths.Style("Large"));
                if (large != null && CombatStyle.HasOldLargeStance(large))
                {
                    CombatStyle.ApplyLargeStance(large);
                    EditorUtility.SetDirty(large);
                    Debug.Log($"[Battler Setup] {BattlerPaths.Style("Large")}: the boxing stance from the captured idle.");
                }
            }

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

        /// <summary>A body's combat style asset, made from the ready-made one if missing.</summary>
        private static CombatStyle EnsureStyle(string body)
        {
            string path = BattlerPaths.Style(body);
            var asset = AssetDatabase.LoadAssetAtPath<CombatStyle>(path);
            if (asset != null)
            {
                return asset;
            }

            EditorFolders.EnsureParent(path);
            asset = CombatStyle.CreateFor(body);
            AssetDatabase.CreateAsset(asset, path);
            return asset;
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
