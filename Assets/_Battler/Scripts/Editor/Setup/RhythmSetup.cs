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

        /// <summary>
        /// Setup 16: the boxer's base loop is the captured boxing idle (Boxing.fbx), and its hands
        /// go where the capture holds them (unless already changed).
        /// </summary>
        public static void Migrate(int from)
        {
            if (from >= 15 && from < 17)
            {
                MigrateIdle(RhythmStyle.ShieldAndSword, RhythmDefinition.OldMediumMainGuard, RhythmDefinition.MediumMainGuard,
                    RhythmDefinition.OldMediumOffGuard, RhythmDefinition.MediumOffGuard, null, null);
                MigrateIdle(RhythmStyle.Eskrima, RhythmDefinition.OldSmallMainGuard, RhythmDefinition.SmallMainGuard,
                    RhythmDefinition.OldSmallOffGuard, RhythmDefinition.SmallOffGuard, RhythmDefinition.OldSmallMainGuardTurn, RhythmDefinition.SmallMainGuardTurn);
            }

            if (from < 15 || from >= 17)
            {
                return;
            }

            string path = BattlerPaths.Rhythm(RhythmStyle.Boxing.ToString());
            var boxing = AssetDatabase.LoadAssetAtPath<RhythmDefinition>(path);
            if (boxing == null)
            {
                return;
            }

            if (boxing.clip == null || boxing.clip.IsEmpty)
            {
                boxing.clip = RhythmClips.Boxing();
                Debug.Log($"[Battler Setup] {path}: the base loop is now the captured boxing idle ({boxing.clip.source}).");
            }

            if (Vector3.Distance(boxing.offGuard, RhythmDefinition.OldBoxingOffGuard) < 1e-4f)
            {
                boxing.offGuard = RhythmDefinition.BoxingOffGuard;
            }

            if (Vector3.Distance(boxing.mainGuard, RhythmDefinition.OldBoxingMainGuard) < 1e-4f
                && Vector3.Distance(boxing.mainGuardTurn, RhythmDefinition.OldBoxingMainGuardTurn) < 1e-4f)
            {
                boxing.mainGuard = RhythmDefinition.BoxingMainGuard;
                boxing.mainGuardTurn = RhythmDefinition.BoxingMainGuardTurn;
            }

            // (The club rides the captured rear hand: no pump of its own.)
            if (!boxing.clip.IsEmpty && boxing.weaponLoop == WeaponLoop.Pump)
            {
                boxing.weaponLoop = WeaponLoop.None;
            }

            EditorUtility.SetDirty(boxing);
        }

        /// <summary>Setup 17: the medium and small rhythms' base loops are their captured idles, and the hands go where they hold them (unless changed).</summary>
        private static void MigrateIdle(RhythmStyle style, Vector3 oldMain, Vector3 main, Vector3 oldOff, Vector3 off, Vector3? oldTurn, Vector3? turn)
        {
            string path = BattlerPaths.Rhythm(style.ToString());
            var rhythm = AssetDatabase.LoadAssetAtPath<RhythmDefinition>(path);
            if (rhythm == null)
            {
                return;
            }

            var made = RhythmDefinition.Create(style);
            if (rhythm.clip == null || rhythm.clip.IsEmpty)
            {
                rhythm.clip = made.clip;
                Debug.Log($"[Battler Setup] {path}: the base loop is now the captured idle ({rhythm.clip.source}).");
            }

            if (Vector3.Distance(rhythm.mainGuard, oldMain) < 1e-4f)
            {
                rhythm.mainGuard = main;
            }

            if (Vector3.Distance(rhythm.offGuard, oldOff) < 1e-4f)
            {
                rhythm.offGuard = off;
            }

            if (oldTurn.HasValue && Vector3.Distance(rhythm.mainGuardTurn, oldTurn.Value) < 1e-4f)
            {
                rhythm.mainGuardTurn = turn.Value;
            }

            // (The flourishes a touch slower, from the stick held low.)
            foreach (var p in rhythm.postures)
            {
                var fresh = p != null ? made.Find(p.Label) : null;
                if (fresh != null && (p.Label == "AbanicoFlurry" && Mathf.Approximately(p.duration, 0.65f) || p.Label == "RedondaTwirl" && Mathf.Approximately(p.duration, 0.85f)))
                {
                    p.duration = fresh.duration;
                    EditorUtility.SetDirty(p);
                }
            }

            foreach (var p in made.postures)
            {
                UnityEngine.Object.DestroyImmediate(p);
            }

            UnityEngine.Object.DestroyImmediate(made);
            EditorUtility.SetDirty(rhythm);
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
