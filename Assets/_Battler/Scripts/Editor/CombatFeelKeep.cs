using System.Collections.Generic;
using PhysicsBattler.Locomotion;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PhysicsBattler.Editor
{
    /// <summary>
    /// What is tuned on the scene's Combat Feel while the game plays (the sandbox's panel changes it there) is kept
    /// when play stops. Unity puts a scene's objects back as they were, so: as play stops, what the Combat Feel is
    /// tuned to is read off it (<see cref="CombatFeel.Tuned"/>); once play has stopped, the settings that differ are
    /// put on the scene's object (one undo step), and the scene is saved for it if it had no other unsaved changes
    /// when play began (otherwise it is left marked changed, to be saved by hand). The panel's "Keep what I tune"
    /// turns it off.
    /// </summary>
    [InitializeOnLoad]
    internal static class CombatFeelKeep
    {
        private const string TunedKey = "PhysicsBattler.CombatFeel.Tuned", CleanKey = "PhysicsBattler.CombatFeel.SceneWasClean";

        static CombatFeelKeep()
        {
            if (Application.isBatchMode)
            {
                return;
            }

            EditorApplication.playModeStateChanged += Changed;
        }

        private static void Changed(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.ExitingEditMode)
            {
                // (Was the scene saved as play began: then saving it after is saving only what was tuned.)
                var feel = Object.FindAnyObjectByType<CombatFeel>();
                SessionState.SetBool(CleanKey, feel != null && !feel.gameObject.scene.isDirty);
            }
            else if (change == PlayModeStateChange.ExitingPlayMode)
            {
                var feel = Object.FindAnyObjectByType<CombatFeel>();
                SessionState.SetString(TunedKey, feel != null && CombatFeel.KeepTuned ? feel.Tuned() : "");
            }
            else if (change == PlayModeStateChange.EnteredEditMode)
            {
                string tuned = SessionState.GetString(TunedKey, "");
                SessionState.EraseString(TunedKey);
                var feel = Object.FindAnyObjectByType<CombatFeel>();
                if (tuned.Length == 0 || feel == null)
                {
                    return;
                }

                var changed = new List<string>();
                Undo.RecordObject(feel, "Keep what was tuned in play");
                if (feel.Tune(tuned, changed) == 0)
                {
                    return;
                }

                PrefabUtility.RecordPrefabInstancePropertyModifications(feel);
                EditorUtility.SetDirty(feel);
                var scene = feel.gameObject.scene;
                EditorSceneManager.MarkSceneDirty(scene);
                bool saved = SessionState.GetBool(CleanKey, false) && !string.IsNullOrEmpty(scene.path) && EditorSceneManager.SaveScene(scene);
                Debug.Log($"[Combat Feel] kept what was tuned in play, {changed.Count} setting{(changed.Count == 1 ? "" : "s")}: {string.Join("; ", changed)}. "
                    + (saved ? "The scene is saved." : "The scene had other unsaved changes, so it is not saved: save it to keep them.") + " Undo (Ctrl+Z) puts them back.");
            }
        }
    }
}
