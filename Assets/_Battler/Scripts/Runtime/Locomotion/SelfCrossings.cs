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

        // ---- The budget (the shield plan's step 5, "lock it"; 19 November) --------------------------------------------

        /// <summary>
        /// The budget for a fighter on his feet: no single crossing of his own sword, shield and body longer than
        /// <see cref="LongestAllowed"/> seconds, and all of them together under <see cref="ShareAllowed"/> of the time
        /// he was watched on his feet. Falling, down and getting up are counted apart and are not held to it yet
        /// (the plan's step 4 is not built).
        /// </summary>
        public const float LongestAllowed = 0.1f, ShareAllowed = 0.005f;

        /// <summary>What he is doing when he is not on his feet: outside the budget.</summary>
        public static bool OffHisFeet(string doing) => doing == "falling or down" || doing == "getting up";

        /// <summary>The longest single crossing on his feet (seconds), and what it was; the same off his feet.</summary>
        public static float Longest, LongestDown;

        public static string LongestWas = "", LongestDownWas = "";

        /// <summary>A crossing has ended (or reached a twentieth of a second and goes on): how long it has been, and whose.</summary>
        public static void Stretch(string doing, int kind, float seconds, string who)
        {
            string said = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}'s {1} while {2}", who, Names[kind], doing);
            if (OffHisFeet(doing))
            {
                if (seconds > LongestDown)
                {
                    LongestDown = seconds;
                    LongestDownWas = said;
                }
            }
            else if (seconds > Longest)
            {
                Longest = seconds;
                LongestWas = said;
            }
        }

        /// <summary>
        /// The verdict, one line: "CROSSINGS OK" or "CROSSINGS OVER BUDGET", with the figures. On his feet only; what
        /// he does off his feet is told after it and does not change the verdict.
        /// </summary>
        public static string Budget()
        {
            var c = System.Globalization.CultureInfo.InvariantCulture;
            float watched = 0f, crossed = 0f, watchedDown = 0f, crossedDown = 0f;
            foreach (var pair in By)
            {
                float sum = pair.Value[1] + pair.Value[2] + pair.Value[3];
                if (OffHisFeet(pair.Key))
                {
                    watchedDown += pair.Value[0];
                    crossedDown += sum;
                }
                else
                {
                    watched += pair.Value[0];
                    crossed += sum;
                }
            }

            if (watched < 1f)
            {
                return "CROSSINGS not measured (nobody with a shield was watched on his feet)";
            }

            float share = crossed / watched;
            bool ok = Longest <= LongestAllowed + 1e-3f && share <= ShareAllowed;
            return string.Format(c, "CROSSINGS {0}: on his feet {1:0.00} s of {2:0} s ({3:0.00}%; the budget is {4:0.0}%), the longest {5:0.00} s (the budget is {6:0.00} s){7}. Off his feet, not held to it: {8:0.00} s of {9:0.0} s, the longest {10:0.00} s{11}",
                ok ? "OK" : "OVER BUDGET", crossed, watched, 100f * share, 100f * ShareAllowed, Longest, LongestAllowed, Longest > 0f ? " (" + LongestWas + ")" : "",
                crossedDown, watchedDown, LongestDown, LongestDown > 0f ? " (" + LongestDownWas + ")" : "");
        }

        public static void Clear()
        {
            By.Clear();
            Longest = LongestDown = 0f;
            LongestWas = LongestDownWas = "";
            BoardIn.Clear();
            ByAttack.Clear();
            Log.Clear();
            Stretches = 0;
        }

        /// <summary>One line for the lab: since the playtest began, how long his own shield and sword have crossed him.</summary>
        public static string Short()
        {
            var c = System.Globalization.CultureInfo.InvariantCulture;
            float watched = 0f;
            float[] seen = new float[3];
            foreach (var row in By.Values)
            {
                watched += row[0];
                for (int k = 0; k < 3; k++)
                {
                    seen[k] += row[1 + k];
                }
            }

            if (watched < 1f)
            {
                return "nothing measured yet (it reads while a playtest is recorded)";
            }

            return string.Format(c, "over {0:0} fighter-seconds: blade through his own shield {1:0.0} s, sword arm through it {2:0.0} s, shield in his own body {3:0.0} s ({4} stretches)", watched, seen[0], seen[1], seen[2], Stretches);
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

    /// <summary>
    /// The other man in his shield (22 November, the plan "clothes that slide", steps 1 and 2): how deep and how long a
    /// part of another fighter's body, as seen (the physical body), is inside a fighter's shield's board, and how long
    /// another man's blade passes through the board. It changes nothing. A part counts when the board's disc reaches
    /// more than <see cref="Deep"/> into its solid; a stretch is written to the playtest log (<c>shieldin</c>) when it
    /// lasts <see cref="Long"/> seconds or more. The plan's pass mark: no stretch that long.
    /// </summary>
    public static class OthersInShield
    {
        /// <summary>A part this deep in a board counts (metres); a stretch this long is one he would see (seconds).</summary>
        public const float Deep = 0.02f, Long = 0.1f;

        /// <summary>Seconds a shield was watched with another man within reach of it; seconds with a part of him in it (over <see cref="Deep"/>); seconds with his blade through it.</summary>
        public static float Watched, In, BladeThrough;

        /// <summary>The longest stretch with a part in it (seconds), the deepest (metres), stretches of <see cref="Long"/> or more, and all stretches.</summary>
        public static float Longest, Deepest;

        public static int LongStretches, Stretches, BladeStretches;

        public static string LongestWas = "";

        /// <summary>For each part of the other man: the seconds it was in a board.</summary>
        public static readonly Dictionary<string, float> ByPart = new Dictionary<string, float>();

        /// <summary>For what the shield's bearer was doing: the seconds with a part of the other man in his board.</summary>
        public static readonly Dictionary<string, float> ByDoing = new Dictionary<string, float>();

        public static readonly List<string> Log = new List<string>();

        public static void Clear()
        {
            Watched = In = BladeThrough = Longest = Deepest = 0f;
            LongStretches = Stretches = BladeStretches = 0;
            LongestWas = "";
            ByPart.Clear();
            ByDoing.Clear();
            Log.Clear();
        }

        public static void Ended(string said, float seconds, float deepest)
        {
            Stretches++;
            LongStretches += seconds >= Long - 1e-4f ? 1 : 0;
            if (seconds > Longest)
            {
                Longest = seconds;
                LongestWas = said;
            }

            Deepest = System.Math.Max(Deepest, deepest);
            if (Log.Count < 40)
            {
                Log.Add(said);
            }
        }

        public static void Add(Dictionary<string, float> rows, string key, float dt)
        {
            rows.TryGetValue(key, out float was);
            rows[key] = was + dt;
        }

        /// <summary>One line: the verdict against the plan's pass mark, with the figures.</summary>
        public static string Budget()
        {
            var c = System.Globalization.CultureInfo.InvariantCulture;
            if (Watched < 0.5f)
            {
                return "SHIELDIN not measured (no shield had another man near it)";
            }

            return string.Format(c, "SHIELDIN {0}: a part of the other man more than {1:0} cm inside a shield {2:0.00} s of {3:0} s watched near ({4:0.00}%), {5} stretches, {6} of {7:0.0} s or more, the longest {8:0.00} s{9}, the deepest {10:0.0} cm; his blade through a shield {11:0.00} s ({12} stretches)",
                LongStretches == 0 ? "OK" : "OVER", 100f * Deep, In, Watched, 100f * In / Watched, Stretches, LongStretches, Long, Longest, Longest > 0f ? " (" + LongestWas + ")" : "", 100f * Deepest, BladeThrough, BladeStretches);
        }

        /// <summary>One line for the lab.</summary>
        public static string Short()
        {
            var c = System.Globalization.CultureInfo.InvariantCulture;
            if (Watched < 0.5f)
            {
                return "nothing yet (it reads while two men are near and a playtest is recorded)";
            }

            return string.Format(c, "{0:0.00} s of {1:0} s near: {2} times a part of him over {3:0} cm in a shield, {4} for {5:0.0} s or more, the longest {6:0.00} s, the deepest {7:0} cm; a blade through it {8:0.00} s",
                In, Watched, Stretches, 100f * Deep, LongStretches, Long, Longest, 100f * Deepest, BladeThrough);
        }

        public static string Text()
        {
            var c = System.Globalization.CultureInfo.InvariantCulture;
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("    the other man in a shield, as seen: " + Short());
            if (ByPart.Count > 0)
            {
                sb.AppendLine("      by his part: " + string.Join(", ", ByPart.OrderByDescending(p => p.Value).Select(p => string.Format(c, "{0} {1:0.00} s", p.Key, p.Value))));
                sb.AppendLine("      by what the shield's bearer was doing: " + string.Join(", ", ByDoing.OrderByDescending(p => p.Value).Select(p => string.Format(c, "{0} {1:0.00} s", p.Key, p.Value))));
            }

            foreach (string line in Log)
            {
                sb.AppendLine("      " + line);
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// How much what a fighter holds trembles (18 November). Six places are followed each step: his board's middle and
    /// his blade's tip, each as seen (the physical body), as posed, and the seen against the posed. Of each, what is
    /// left when its own smooth motion is taken off (the place less the average of five steps about it: a tremble
    /// of some 12 Hz and over, and the curve of anything that quickens hard, which is why a strike reads high and
    /// why "seen against posed" is the one to read: the pose's own motion is not in it). It changes nothing.
    /// </summary>
    public static class HeldShake
    {
        public const int Signals = 10;
        public static readonly string[] Names = { "board seen", "board posed", "board seen against posed", "blade tip seen", "blade tip posed", "tip seen against posed", "chest seen against posed", "shield hand seen against posed", "sword hand seen against posed", "hips seen against posed" };

        /// <summary>Each large tremble of a fighter standing or moving is written out (headless: FIGHT_SHAKELOG=1).</summary>
        public static bool Trace;

        /// <summary>A tremble over this (metres) is one that shows.</summary>
        public const float Shows = 0.005f;

        /// <summary>For each thing he was doing: the seconds watched; then for each place the sum of the tremble squared times the seconds, and the seconds it showed.</summary>
        public static readonly Dictionary<string, double[]> By = new Dictionary<string, double[]>();

        public static void Watched(string doing, float dt)
        {
            Row(doing)[0] += dt;
        }

        public static void Add(string doing, int signal, float tremble, float dt)
        {
            var row = Row(doing);
            row[1 + 2 * signal] += (double)tremble * tremble * dt;
            row[2 + 2 * signal] += tremble > Shows ? dt : 0f;
        }

        private static double[] Row(string doing)
        {
            if (!By.TryGetValue(doing, out var row))
            {
                By[doing] = row = new double[1 + 2 * Signals];
            }

            return row;
        }

        /// <summary>
        /// The tremble's budget, for a fighter standing or moving (not striking, struck, bashing or off his feet, where
        /// what he holds is meant to move fast): his board as seen may show a tremble (over 5 mm) no more than
        /// <see cref="BoardShowsAllowed"/> of that time. The blade's tip is told beside it and is not held to a budget
        /// yet: why it trembles on his feet is not known.
        /// </summary>
        public const float BoardShowsAllowed = 0.08f;

        public static string Budget()
        {
            var c = System.Globalization.CultureInfo.InvariantCulture;
            double watched = 0, board = 0, tip = 0;
            foreach (var pair in By)
            {
                if (pair.Key == "standing" || pair.Key == "moving")
                {
                    watched += pair.Value[0];
                    board += pair.Value[2];
                    tip += pair.Value[8];
                }
            }

            if (watched < 1.0)
            {
                return "TREMBLE not measured (nobody with a shield was watched standing or moving)";
            }

            bool ok = board / watched <= BoardShowsAllowed;
            return string.Format(c, "TREMBLE {0}: standing or moving ({1:0} s), his board as seen shows a tremble {2:0.0}% of the time (the budget is {3:0}%); his blade's tip {4:0.0}% (no budget yet)",
                ok ? "OK" : "OVER BUDGET", watched, 100.0 * board / watched, 100.0 * BoardShowsAllowed, 100.0 * tip / watched);
        }

        public static void Clear()
        {
            By.Clear();
        }

        /// <summary>One line for the lab: since the playtest began, how much boards and blade tips have trembled as seen.</summary>
        public static string Short()
        {
            var c = System.Globalization.CultureInfo.InvariantCulture;
            var all = new double[1 + 2 * Signals];
            foreach (var row in By.Values)
            {
                for (int i = 0; i < all.Length; i++)
                {
                    all[i] += row[i];
                }
            }

            if (all[0] < 1.0)
            {
                return "nothing measured yet (it reads while a playtest is recorded)";
            }

            return string.Format(c, "over {0:0} fighter-seconds, a shield's board trembles more than 5 mm {1:0}% of the time (its pose alone {2:0}%), a blade's tip {3:0}% ({4:0}%)",
                all[0], 100.0 * all[2] / all[0], 100.0 * all[4] / all[0], 100.0 * all[8] / all[0], 100.0 * all[10] / all[0]);
        }

        public static string Text()
        {
            var c = System.Globalization.CultureInfo.InvariantCulture;
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("    what he holds trembles (mm, the usual size; and the share of the time it is over 5 mm): board seen / posed / seen against posed; blade tip seen / posed / seen against posed");
            var all = new double[1 + 2 * Signals];
            foreach (var row in By.OrderByDescending(r => r.Value[0]))
            {
                sb.AppendLine("      " + Line(row.Key, row.Value, c));
                for (int i = 0; i < all.Length; i++)
                {
                    all[i] += row.Value[i];
                }
            }

            sb.AppendLine("      " + Line("in all", all, c));
            return sb.ToString();
        }

        private static string Line(string name, double[] row, System.Globalization.CultureInfo c)
        {
            double w = System.Math.Max(1e-6, row[0]);
            string One(int s) => string.Format(c, "{0:0.0} ({1:0}%)", 1000.0 * System.Math.Sqrt(row[1 + 2 * s] / w), 100.0 * row[2 + 2 * s] / w);
            return string.Format(c, "{0,-18} {1,6:0.0} s   board {2} / {3} / {4};   tip {5} / {6} / {7};   seen against posed: hips {11}, chest {8}, shield hand {9}, sword hand {10}", name, row[0], One(0), One(1), One(2), One(3), One(4), One(5), One(6), One(7), One(8), One(9));
        }
    }
}
