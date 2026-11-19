using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PhysicsBattler.Sandbox;
using UnityEngine;

/// <summary>
/// The panel drawn without Unity: `panel [place] [search words]`. The sandbox's panel (and its actions window) is
/// drawn as Unity draws it, once to lay it out and once to paint it, with the stubs writing down every control
/// asked for. Checked: both passes ask for the same controls in the same order, and every row, column and
/// scrolling view that is begun is ended. With search words, what is left of the panel is printed.
/// Place: battle (default), duel, aim, tubes, targets.
/// </summary>
public static class PanelCheck
{
    private const BindingFlags Any = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

    public static int Run(string[] args)
    {
        string place = args.Length > 1 ? args[1] : "battle";
        string words = args.Length > 2 ? string.Join(" ", args.Skip(2)) : "";
        var type = typeof(BattleSandbox);
        var sandbox = new GameObject("Battle").AddComponent<BattleSandbox>();
        void Set(string field, object value)
        {
            var found = type.GetField(field, Any);
            found.SetValue(found.IsStatic ? null : sandbox, value);
        }

        Set("combatFeel", new GameObject("Feel").AddComponent<PhysicsBattler.Locomotion.CombatFeel>());
        Set("uiScale", 1f);
        if (place == "duel" || place == "aim")
        {
            Set("targetTest", true);
            Set("aimTest", true);
            Set("duelLab", place == "duel");
        }
        else if (place == "tubes")
        {
            Set("targetTest", true);
            Set("tubeTest", true);
        }
        else if (place == "targets")
        {
            Set("targetTest", true);
        }

        var latch = type.GetMethod("LatchSearch", Any);
        int bad = 0;
        foreach (string window in new[] { "DrawPanel", "DrawActions" })
        {
            if (window == "DrawActions" && place != "duel")
            {
                continue;
            }

            var draw = type.GetMethod(window, Any);
            List<string> laid = null, painted = null;
            Set("search", words);
            // (Three times: what a row holds is read off in one layout and used from the next.)
            for (int round = 0; round < 3; round++)
            {
                Event.current = new Event { type = EventType.Layout };
                latch.Invoke(sandbox, null);
                GuiTrace.Log = laid = new List<string>();
                draw.Invoke(sandbox, new object[] { 0 });
                Event.current = new Event { type = EventType.Repaint };
                GuiTrace.Log = painted = new List<string>();
                draw.Invoke(sandbox, new object[] { 0 });
                GuiTrace.Log = null;
                bool same = laid.SequenceEqual(painted);
                int rows = 0, columns = 0, scrolls = 0, least = 0;
                foreach (string line in painted)
                {
                    rows += line == "{h" ? 1 : line == "h}" ? -1 : 0;
                    columns += line == "{v" ? 1 : line == "v}" ? -1 : 0;
                    scrolls += line == "{scroll" ? 1 : line == "scroll}" ? -1 : 0;
                    least = Math.Min(least, Math.Min(rows, Math.Min(columns, scrolls)));
                }

                bool balanced = rows == 0 && columns == 0 && scrolls == 0 && least == 0;
                bad += same && balanced ? 0 : 1;
                Console.WriteLine($"{window} ({place}{(words.Length > 0 ? ", searching \"" + words + "\"" : "")}), round {round + 1}: {painted.Count} controls; laid out and painted {(same ? "alike" : "DIFFER")}; begun and ended {(balanced ? "in pairs" : $"NOT in pairs (rows {rows}, columns {columns}, scrolls {scrolls})")}");
            }

            if (words.Length > 0 || Environment.GetEnvironmentVariable("PANEL_ALL") == "1")
            {
                int depth = 0;
                foreach (string line in painted)
                {
                    if (line.EndsWith("}"))
                    {
                        depth--;
                    }

                    if (line != "space" && line != "flex")
                    {
                        Console.WriteLine("  " + new string(' ', 2 * Math.Max(0, depth)) + (line.Length > 110 ? line.Substring(0, 110) + "..." : line));
                    }

                    if (line.StartsWith("{"))
                    {
                        depth++;
                    }
                }
            }
        }

        // (The "look here" names: each has to be drawn somewhere, or its frame shows nothing. Looked for with every
        // section open; a name that belongs to another lab is not a fault here, so the names not drawn are only told.)
        {
            var sandboxType = typeof(PhysicsBattler.Sandbox.BattleSandbox);
            PhysicsBattler.Sandbox.BattleSandbox.OpenAllSections = true;
            PhysicsBattler.Sandbox.BattleSandbox.LookFramed.Clear();
            Set("search", "");
            foreach (string window in new[] { "DrawPanel", "DrawActions" })
            {
                var draw = sandboxType.GetMethod(window, Any);
                for (int round = 0; round < 2; round++)
                {
                    Event.current = new Event { type = EventType.Layout };
                    latch.Invoke(sandbox, null);
                    draw.Invoke(sandbox, new object[] { 0 });
                    Event.current = new Event { type = EventType.Repaint };
                    draw.Invoke(sandbox, new object[] { 0 });
                }
            }

            PhysicsBattler.Sandbox.BattleSandbox.OpenAllSections = false;
            var names = PhysicsBattler.Sandbox.BattleSandbox.LookNames;
            var missing = names.Where(n => !PhysicsBattler.Sandbox.BattleSandbox.LookFramed.Contains(n)).ToList();
            Console.WriteLine($"LOOK HERE ({place}): {names.Count - missing.Count} of {names.Count} names drawn and framed" + (missing.Count > 0 ? "; not drawn in this lab: " + string.Join(", ", missing.Select(n => "\"" + n + "\"")) : ""));
        }

        Console.WriteLine(bad == 0 ? "PANEL OK" : $"PANEL: {bad} FAULTS");
        return bad == 0 ? 0 : 1;
    }
}
