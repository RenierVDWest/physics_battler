using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// Where each strike's weapon is at its contact moment when played as it is on each body, against where
    /// and which way the body stood when it began (metres; x right, y up from the ground, z forward): the
    /// grip and the way the blade points from it (so any weapon's length can be put on it):
    /// measured by the headless runner (`strikes &lt;dir&gt; &lt;body&gt; --measure`) into
    /// Resources/Locomotion/StrikeReach.json, so a strike is aimed from where it really lands, not a guess.
    /// </summary>
    public static class StrikeReach
    {
        [Serializable]
        public class Entry
        {
            public string body;
            public string clip;
            public float x, y, z;
            public float bx, by, bz;
        }

        [Serializable]
        public class File_
        {
            public string about;
            public Entry[] entries;
        }

        public const string ResourcePath = "Locomotion/StrikeReach";
        public const string AssetPath = "_Battler/Resources/Locomotion/StrikeReach.json";

        private static Dictionary<string, (Vector3 grip, Vector3 blade)> table;

        /// <param name="grip">Where the hand holds the weapon at contact.</param>
        /// <param name="blade">The way the blade points from it then (unit).</param>
        public static bool TryGet(string body, string clip, out Vector3 grip, out Vector3 blade)
        {
            if (table == null)
            {
                Load();
            }

            bool found = table.TryGetValue(body + "/" + clip, out var e);
            grip = e.grip;
            blade = e.blade;
            return found;
        }

        public static void Load()
        {
            table = new Dictionary<string, (Vector3, Vector3)>();
            string text = null;
            var asset = Resources.Load<TextAsset>(ResourcePath);
            if (asset != null)
            {
                text = asset.text;
            }
            else
            {
                string path = Path.Combine(Application.dataPath, AssetPath);
                for (var dir = new DirectoryInfo(Directory.GetCurrentDirectory()); !File.Exists(path) && dir != null; dir = dir.Parent)
                {
                    path = Path.Combine(dir.FullName, "Assets", AssetPath);
                }

                if (File.Exists(path))
                {
                    text = File.ReadAllText(path);
                }
            }

            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var file = JsonUtility.FromJson<File_>(text);
            foreach (var e in file.entries ?? Array.Empty<Entry>())
            {
                table[e.body + "/" + e.clip] = (new Vector3(e.x, e.y, e.z), new Vector3(e.bx, e.by, e.bz).normalized);
            }
        }

        /// <summary>Forgets what was loaded (the runner, after measuring).</summary>
        public static void Forget() => table = null;
    }
}
