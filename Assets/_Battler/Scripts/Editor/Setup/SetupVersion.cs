using System.IO;

namespace PhysicsBattler.Editor.Setup
{
    /// <summary>
    /// Which version of the project setup has run, in ProjectSettings/BattlerSetupVersion.txt
    /// (commit it). When new setup steps arrive, Current goes up and the setup runs once more.
    /// </summary>
    internal static class SetupVersion
    {
        /// <summary>
        /// 1 = The first prototype: URP, physics settings check, orc materials and models, unit, weapon
        ///     and prop prefabs, data assets and the Sandbox scene.
        /// </summary>
        public const int Current = 1;

        private const string FilePath = "ProjectSettings/BattlerSetupVersion.txt";

        public static bool IsCurrent => Read() >= Current;

        public static void MarkCurrent()
        {
            File.WriteAllText(FilePath, Current + "\n");
        }

        private static int Read()
        {
            if (!File.Exists(FilePath))
            {
                return 0;
            }

            return int.TryParse(File.ReadAllText(FilePath).Trim(), out int version) ? version : 0;
        }
    }
}
