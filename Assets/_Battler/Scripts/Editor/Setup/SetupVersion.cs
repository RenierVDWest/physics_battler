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
        /// 4 = After the fourth playtest: a fighting stance, balance measured against the area the feet
        ///     support (catch-step threshold 0.16 → 0.05 m outside it), more weight shift in the swing,
        ///     load-boost cap 300 → 500.
        /// 5 = Strength and weight: units get a strength stat (the goblin's scaled down), swings are paced
        ///     by it and the weapon's weight, and the follow-through budget goes from 40° to 75°.
        /// 6 = The big club becomes two-handed (carried low at the side), the wind-up goes back and up
        ///     (-150° → -200°), and the strike starts at the turning point instead of waiting to settle.
        /// 7 = The two-handed carry moves out to the side, clear of the hips and back leg.
        /// 8 = One-handed again, with the club at half size (2 kg); wind-up back to -150°. Pictures of
        ///     the game view in the playtest log.
        /// 9 = The animation-first rebuild: three body sizes on an 8 by 8 board, the Battle scene; the
        ///     orc, goblin, club and Sandbox scene removed.
        /// 10 = Milestone 2: the sword, club and shield (models, weapon and move assets, prefabs), aimed
        ///     blows; the lineup laid out in the Battle scene as unit markers.
        /// 11 = Blows keep clear of their wielder's body; the default lineup's team B is rearranged so
        ///     each unit faces a different size.
        /// 12 = The small body rebuilt 1.4 times larger: the unit prefabs are rebuilt from the new rig.
        /// </summary>
        public const int Current = 12;

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
