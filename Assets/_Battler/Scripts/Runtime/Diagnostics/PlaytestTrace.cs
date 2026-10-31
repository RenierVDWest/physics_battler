using System;
using PhysicsBattler.Units;

namespace PhysicsBattler.Diagnostics
{
    /// <summary>
    /// Where the gameplay code reports why it made a decision (a catch step, a knockdown, a rejected
    /// hit), for the <see cref="PlaytestRecorder"/> to write into the playtest log. Costs nothing while
    /// nothing records: check <see cref="Enabled"/> before building the message.
    /// </summary>
    public static class PlaytestTrace
    {
        public static event Action<UnitController, string, string> Noted;

        public static bool Enabled => Noted != null;

        /// <param name="unit">The unit it is about (null for the scene).</param>
        /// <param name="category">One short lower-case word: attack, hit, step, state, getup, input…</param>
        public static void Note(UnitController unit, string category, string text)
        {
            Noted?.Invoke(unit, category, text);
        }
    }
}
