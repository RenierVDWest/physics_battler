using System.Collections.Generic;
using System.Linq;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// What a fighter's own shield and sword cross of the fighter himself, as seen (the physical body), counted by what
    /// he was doing (the plan "A shield that stays outside its bearer"). It changes nothing. Three crossings: his blade
    /// through his own shield's board, his sword arm through it, and the board in his own trunk, head or legs. Each
    /// stretch is written to the playtest log (<c>cross</c>), so a playtest in Unity counts them too.
    /// </summary>
    public static class SelfCrossings
    {
        public const int Blade = 0, SwordArm = 1, Board = 2;
        public static readonly string[] Names = { "blade through his own shield", "sword arm through his own shield", "shield in his own body" };

        /// <summary>Measured at all (headless, or while a playtest is recorded).</summary>
        public static bool Measures;

        /// <summary>For each thing he was doing: the seconds watched, and the seconds of each crossing.</summary>
        public static readonly Dictionary<string, float[]> By = new Dictionary<string, float[]>();

        /// <summary>For the board in his body: the seconds in each part.</summary>
        public static readonly Dictionary<string, float> BoardIn = new Dictionary<string, float>();

        /// <summary>For each attack: the seconds striking with it, and of each crossing while striking with it.</summary>
        public static readonly Dictionary<string, float[]> ByAttack = new Dictionary<string, float[]>();

        public static int Stretches;

        /// <summary>The first stretches, each as it is written to the playtest log.</summary>
        public static readonly List<string> Log = new List<string>();

        public static void Watched(string doing, float dt, string attack)
        {
            Row(By, doing)[0] += dt;
            if (attack != null)
            {
                Row(ByAttack, attack)[0] += dt;
            }
        }

        public static void Crossed(string doing, int kind, float dt, string attack)
        {
            Row(By, doing)[1 + kind] += dt;
            if (attack != null)
            {
                Row(ByAttack, attack)[1 + kind] += dt;
            }
        }

        /// <summary>The pose itself had that crossing too (so it is the pose's, not only the physical body's being off its pose).</summary>
        public static void Posed(string doing, int kind, float dt)
        {
            Row(By, doing)[4 + kind] += dt;
        }

        public static void InPart(string part, float dt)
        {
            BoardIn.TryGetValue(part, out float was);
            BoardIn[part] = was + dt;
        }

        private static float[] Row(Dictionary<string, float[]> rows, string key)
        {
            if (!rows.TryGetValue(key, out var row))
            {
                rows[key] = row = new float[7];
            }

            return row;
        }

        public static void Clear()
        {
            By.Clear();
            BoardIn.Clear();
            ByAttack.Clear();
            Log.Clear();
            Stretches = 0;
        }

        /// <summary>The table, for a report.</summary>
        public static string Text()
        {
            var c = System.Globalization.CultureInfo.InvariantCulture;
            var sb = new System.Text.StringBuilder();
            float all = By.Values.Sum(r => r[0]);
            sb.AppendLine(string.Format(c, "    own crossings, as seen ({0:0} fighter-seconds watched, {1} stretches): seconds of each, and the share of that time", all, Stretches));
            sb.AppendLine("      doing                 watched   blade through shield   sword arm through shield   shield in own body");
            foreach (var row in By.OrderByDescending(r => r.Value[1] + r.Value[2] + r.Value[3]))
            {
                float w = System.Math.Max(1e-3f, row.Value[0]);
                sb.AppendLine(string.Format(c, "      {0,-20} {1,7:0.0} s   {2,6:0.00} s ({3,3:0}%)       {4,6:0.00} s ({5,3:0}%)          {6,6:0.00} s ({7,3:0}%)",
                    row.Key, row.Value[0], row.Value[1], 100f * row.Value[1] / w, row.Value[2], 100f * row.Value[2] / w, row.Value[3], 100f * row.Value[3] / w));
            }

            float[] seen = new float[3], posed = new float[3];
            foreach (var row in By.Values)
            {
                for (int k = 0; k < 3; k++)
                {
                    seen[k] += row[1 + k];
                    posed[k] += row[4 + k];
                }
            }

            sb.AppendLine(string.Format(c, "      of the seconds seen, the pose itself had the crossing too: blade through shield {0:0.00} of {1:0.00} s, sword arm through shield {2:0.00} of {3:0.00} s, shield in own body {4:0.00} of {5:0.00} s",
                posed[0], seen[0], posed[1], seen[1], posed[2], seen[2]));
            if (BoardIn.Count > 0)
            {
                sb.AppendLine("      the shield in his own body, by part: " + string.Join(", ", BoardIn.OrderByDescending(p => p.Value).Select(p => string.Format(c, "{0} {1:0.00} s", p.Key, p.Value))));
            }

            if (ByAttack.Count > 0)
            {
                sb.AppendLine("      striking, by attack (seconds striking: blade through shield / sword arm through shield / shield in body): "
                    + string.Join("; ", ByAttack.OrderByDescending(r => r.Value[1] + r.Value[2]).Select(r => string.Format(c, "{0} {1:0.0} s: {2:0.00} / {3:0.00} / {4:0.00}", r.Key, r.Value[0], r.Value[1], r.Value[2], r.Value[3]))));
            }

            foreach (string line in Log)
            {
                sb.AppendLine("      " + line);
            }

            return sb.ToString();
        }
    }
}
