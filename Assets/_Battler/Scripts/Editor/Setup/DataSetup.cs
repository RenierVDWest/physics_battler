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

        /// <summary>Strength goes with muscle mass: the orc's, times the goblin's scale cubed.</summary>
        public static readonly float GoblinStrength = 240f * GoblinScale * GoblinScale * GoblinScale;

        public static void Apply()
        {
            Ensure<UnitProfile>(BattlerPaths.OrcProfile, _ => { });
            Ensure<UnitProfile>(BattlerPaths.GoblinProfile, goblin =>
            {
                goblin.scale = GoblinScale;
                goblin.totalMassKg = 120f * GoblinScale * GoblinScale * GoblinScale;
                goblin.poiseResistance = 0.45f;
                goblin.strength = GoblinStrength;
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

                club.twoHanded = true;
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

            if (from >= 1 && from < 4)
            {
                MigrateTo4();
            }

            if (from >= 1 && from < 5)
            {
                MigrateTo5();
            }

            if (from >= 1 && from < 6)
            {
                MigrateTo6();
            }

            if (from >= 1 && from < 7)
            {
                MigrateTo7();
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

        private static void MigrateTo4()
        {
            // The balance error is now how far the capture point is outside the area the feet support
            // (it was the distance to the midpoint between the feet), so the step threshold shrinks.
            foreach (var path in new[] { BattlerPaths.OrcProfile, BattlerPaths.GoblinProfile })
            {
                var profile = AssetDatabase.LoadAssetAtPath<UnitProfile>(path);
                if (profile == null)
                {
                    continue;
                }

                bool changed = false;
                changed |= Replace(ref profile.stepThreshold, 0.16f, 0.05f, profile.name, "stepThreshold");
                changed |= Replace(ref profile.maxSubtreeBoost, 300f, 500f, profile.name, "maxSubtreeBoost");
                if (changed)
                {
                    EditorUtility.SetDirty(profile);
                }
            }

            var swing = AssetDatabase.LoadAssetAtPath<AttackDefinition>(BattlerPaths.ClubSwing);
            if (swing != null)
            {
                bool changed = false;
                changed |= Replace(ref swing.leanBack, 0.07f, 0.1f, swing.name, "leanBack");
                changed |= Replace(ref swing.leanForward, 0.1f, 0.15f, swing.name, "leanForward");
                if (changed)
                {
                    EditorUtility.SetDirty(swing);
                }
            }
        }

        private static void MigrateTo5()
        {
            // Strength is new: the orc keeps the default, the goblin gets its share.
            var goblin = AssetDatabase.LoadAssetAtPath<UnitProfile>(BattlerPaths.GoblinProfile);
            if (goblin != null && Replace(ref goblin.strength, 240f, GoblinStrength, goblin.name, "strength"))
            {
                EditorUtility.SetDirty(goblin);
            }

            // Phase durations are gone (the weight and strength set the pace); the follow-through is the
            // braking room the strike's speed is held to.
            var swing = AssetDatabase.LoadAssetAtPath<AttackDefinition>(BattlerPaths.ClubSwing);
            if (swing != null && Replace(ref swing.followAngle, 40f, 75f, swing.name, "followAngle"))
            {
                EditorUtility.SetDirty(swing);
            }
        }

        private static void MigrateTo6()
        {
            var club = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(BattlerPaths.ClubDefinition);
            if (club != null && !club.twoHanded)
            {
                club.twoHanded = true;
                EditorUtility.SetDirty(club);
                Debug.Log($"[Battler Setup] {club.name}.twoHanded: false → true.");
            }

            var swing = AssetDatabase.LoadAssetAtPath<AttackDefinition>(BattlerPaths.ClubSwing);
            if (swing != null)
            {
                bool changed = false;
                changed |= Replace(ref swing.windUpAngle, -150f, -200f, swing.name, "windUpAngle");
                changed |= Replace(ref swing.settleAngle, 20f, 45f, swing.name, "settleAngle");
                changed |= Replace(ref swing.settleTime, 0.12f, 0.03f, swing.name, "settleTime");
                changed |= Replace(ref swing.maxSettleTime, 1.5f, 0.4f, swing.name, "maxSettleTime");
                if (changed)
                {
                    EditorUtility.SetDirty(swing);
                }
            }
        }

        private static void MigrateTo7()
        {
            // The first two-handed carry passed through the hip, and the club exploded out of the body
            // at spawn and got trapped behind the orc's back.
            var club = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(BattlerPaths.ClubDefinition);
            if (club == null)
            {
                return;
            }

            bool changed = false;
            changed |= Replace(ref club.carryGrip, new Vector3(0.2f, 1.0f, 0.28f), new Vector3(0.26f, 1.02f, 0.34f), club.name, "carryGrip");
            changed |= Replace(ref club.carryDirection, new Vector3(0.3f, -0.12f, -1f), new Vector3(0.5f, -0.15f, -0.85f), club.name, "carryDirection");
            if (changed)
            {
                EditorUtility.SetDirty(club);
            }
        }

        private static bool Replace(ref Vector3 field, Vector3 oldDefault, Vector3 newValue, string asset, string name)
        {
            if ((field - oldDefault).sqrMagnitude > 1e-8f)
            {
                Debug.Log($"[Battler Setup] {asset}.{name} is {field} (tuned by you), left as it is; the new default is {newValue}.");
                return false;
            }

            field = newValue;
            Debug.Log($"[Battler Setup] {asset}.{name}: {oldDefault} → {newValue}.");
            return true;
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
