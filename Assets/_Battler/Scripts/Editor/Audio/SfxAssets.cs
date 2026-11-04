using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using PhysicsBattler.Audio;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PhysicsBattler.Editor.Audio
{
    /// <summary>
    /// Where the sounds live and how they're kept in step: DA_Sfx (the sound list) in Audio/Resources, the clips
    /// in Audio/Sfx named {id}_{take}.mp3 (or .wav/.ogg), linked to their sounds whenever any arrive or go.
    /// </summary>
    internal static class SfxAssets
    {
        public const string AudioFolder = "Assets/_Battler/Audio";
        public const string ClipsFolder = AudioFolder + "/Sfx";
        public const string MusicFolder = AudioFolder + "/Music";
        public const string ResourcesFolder = AudioFolder + "/Resources";
        public const string LibraryPath = ResourcesFolder + "/" + SfxLibrary.ResourcePath + ".asset";

        /// <summary>A folder under Assets, made (with its parents) if it isn't there.</summary>
        public static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            int cut = path.LastIndexOf('/');
            string parent = path.Substring(0, cut);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, path.Substring(cut + 1));
        }

        private static readonly Regex TakeName = new Regex(@"^(?<id>.+)_(?<take>\d{2})$");

        /// <summary>DA_Sfx, made with the default sound list if it isn't there; sounds added since are put in.</summary>
        public static SfxLibrary LoadOrCreate()
        {
            EnsureFolder(ClipsFolder);
            EnsureFolder(ResourcesFolder);
            var library = AssetDatabase.LoadAssetAtPath<SfxLibrary>(LibraryPath);
            if (library == null && File.Exists(LibraryPath))
            {
                // On disk but not imported yet: never overwrite it with the defaults.
                return null;
            }

            if (library == null)
            {
                library = ScriptableObject.CreateInstance<SfxLibrary>();
                library.AddMissing();
                AssetDatabase.CreateAsset(library, LibraryPath);
                Debug.Log($"[Physics Battler] Made the sound list {LibraryPath}. Open Physics Battler > Audio > Sound Effects to make the sounds.");
            }
            else if (library.AddMissing())
            {
                EditorUtility.SetDirty(library);
            }

            // Sounds that have been replaced: out of the list, and their takes deleted.
            foreach (var id in SfxIds.Retired)
            {
                var old = library.Find(id);
                if (old != null)
                {
                    DeleteTakes(old);
                    library.Remove(id);
                    EditorUtility.SetDirty(library);
                    Debug.Log($"[Physics Battler] The sound {id} was replaced: removed from the list and its takes deleted.");
                }
            }

            Relink(library);
            return library;
        }

        /// <summary>The file a take is saved to (with no extension: the generator adds .mp3).</summary>
        public static string TakePath(string id, int take)
        {
            return $"{ClipsFolder}/{id}_{take + 1:00}";
        }

        /// <summary>Puts every clip in Audio/Sfx onto its sound, in take order. True if anything changed.</summary>
        public static bool Relink(SfxLibrary library)
        {
            if (library == null || !AssetDatabase.IsValidFolder(ClipsFolder))
            {
                return false;
            }

            var clips = AssetDatabase.FindAssets("t:AudioClip", new[] { ClipsFolder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(path => (Path: path, Match: TakeName.Match(Path.GetFileNameWithoutExtension(path))))
                .Where(c => c.Match.Success)
                .ToList();

            bool changed = false;
            foreach (var entry in library.Sounds)
            {
                if (entry == null || string.IsNullOrEmpty(entry.Id))
                {
                    continue;
                }

                var found = clips.Where(c => c.Match.Groups["id"].Value == entry.Id)
                    .OrderBy(c => int.Parse(c.Match.Groups["take"].Value))
                    .Select(c => AssetDatabase.LoadAssetAtPath<AudioClip>(c.Path))
                    .Where(c => c != null)
                    .ToList();
                if (!found.SequenceEqual(entry.Clips))
                {
                    entry.Clips = found;
                    changed = true;
                }
            }

            if (changed)
            {
                EditorUtility.SetDirty(library);
            }

            return changed;
        }

        /// <summary>How many takes of a sound are on disk.</summary>
        public static int TakesMade(SfxEntry entry)
        {
            int made = 0;
            for (int take = 0; take < entry.Takes; take++)
            {
                if (Exists(TakePath(entry.Id, take)))
                {
                    made++;
                }
            }

            return made;
        }

        /// <summary>A take's clip, by its number (null if it isn't made yet).</summary>
        public static AudioClip ClipFor(SfxEntry entry, int take)
        {
            string path = TakePath(entry.Id, take);
            foreach (var extension in new[] { ".mp3", ".wav", ".ogg" })
            {
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path + extension);
                if (clip != null)
                {
                    return clip;
                }
            }

            return null;
        }

        public static bool Exists(string pathWithoutExtension)
        {
            return File.Exists(pathWithoutExtension + ".mp3") || File.Exists(pathWithoutExtension + ".wav") || File.Exists(pathWithoutExtension + ".ogg");
        }

        /// <summary>Removes every take of a sound from disk (before making it again).</summary>
        public static void DeleteTakes(SfxEntry entry)
        {
            foreach (var guid in AssetDatabase.FindAssets("t:AudioClip", new[] { ClipsFolder }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var match = TakeName.Match(Path.GetFileNameWithoutExtension(path));
                if (match.Success && match.Groups["id"].Value == entry.Id)
                {
                    AssetDatabase.DeleteAsset(path);
                }
            }
        }
    }

    /// <summary>The sound list made and linked, and a "Sound" object (the mix) in the open scene.</summary>
    internal static class SfxSetup
    {
        [MenuItem("Physics Battler/Audio/Add the Sound object to this scene", priority = 41)]
        public static void Apply()
        {
            var library = SfxAssets.LoadOrCreate();
            AssetDatabase.SaveAssets();
            var settings = Object.FindAnyObjectByType<SoundSettings>();
            if (settings == null)
            {
                settings = new GameObject("Sound").AddComponent<SoundSettings>();
                Undo.RegisterCreatedObjectUndo(settings.gameObject, "Add the Sound object");
                Debug.Log("[Physics Battler] Added the Sound object to the scene: the volumes and switches for every sound. Save the scene to keep it.");
            }

            if (settings.Library == null)
            {
                settings.SetLibrary(library);
                EditorUtility.SetDirty(settings);
            }

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(settings.gameObject.scene);
            Selection.activeObject = settings.gameObject;
        }
    }

    /// <summary>
    /// Imports the sounds in Audio/Sfx: short effects mono and decompressed when loaded (so they
    /// start instantly and their silence can be skipped); the ambience loops stereo and compressed. When clips
    /// arrive or go, DA_Sfx is relinked.
    /// </summary>
    internal sealed class SfxImport : AssetPostprocessor
    {
        private void OnPreprocessAudio()
        {
            if (!assetPath.StartsWith(SfxAssets.ClipsFolder + "/"))
            {
                return;
            }

            var importer = (AudioImporter)assetImporter;
            bool ambience = Path.GetFileName(assetPath).StartsWith("ambience_");
            importer.forceToMono = !ambience;
            importer.loadInBackground = ambience;
            var settings = importer.defaultSampleSettings;
            settings.loadType = ambience ? AudioClipLoadType.CompressedInMemory : AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = ambience ? 0.5f : 0.7f;
            settings.preloadAudioData = true;
            importer.defaultSampleSettings = settings;
        }

        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            bool touched = imported.Concat(deleted).Concat(moved).Concat(movedFrom).Any(p => p.StartsWith(SfxAssets.ClipsFolder + "/"));
            if (!touched)
            {
                return;
            }

            EditorApplication.delayCall -= RelinkSoon;
            EditorApplication.delayCall += RelinkSoon;
        }

        private static void RelinkSoon()
        {
            var library = AssetDatabase.LoadAssetAtPath<SfxLibrary>(SfxAssets.LibraryPath);
            if (library != null && SfxAssets.Relink(library))
            {
                AssetDatabase.SaveAssets();
            }
        }
    }
}
