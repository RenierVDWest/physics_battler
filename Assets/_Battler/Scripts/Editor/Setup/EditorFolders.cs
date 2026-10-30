using System.IO;
using UnityEditor;

namespace PhysicsBattler.Editor.Setup
{
    /// <summary>Creates folders (and missing parents) through the AssetDatabase.</summary>
    internal static class EditorFolders
    {
        public static void Ensure(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(parent))
            {
                Ensure(parent);
            }

            AssetDatabase.CreateFolder(parent, Path.GetFileName(folderPath));
        }

        public static void EnsureParent(string assetPath)
        {
            Ensure(Path.GetDirectoryName(assetPath)?.Replace('\\', '/'));
        }
    }
}
