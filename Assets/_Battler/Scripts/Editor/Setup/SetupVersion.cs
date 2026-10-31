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
        /// 2 = Tuning after the first playtest: unit profiles get the raised load-boost cap and the
        ///     longer crouch-then-rise get-up (see <see cref="DataSetup.Migrate"/>).
        /// 3 = After the second playtest: less "puppet on strings" (lighter balance assist, the legs carry
        ///     the weight, less angular damping).
        /// </summary>
        public const int Current = 3;

        /// <summary>The version that last ran here (0 if never).</summary>
        public static int Installed => Read();

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
