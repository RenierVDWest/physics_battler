using System;
using PhysicsBattler.Combat;
using PhysicsBattler.Tests;
using PhysicsBattler.Units;

/// <summary>
/// Milestone 10: every body's strings thrown at a standing opponent of the same body (the plan's
/// combos runner, before dodges exist): how many links each threw, the time from one link's start
/// to the next, and why it ended. A link's own body clearance is the planner's (a link that would
/// pass through its own body is never thrown).
/// </summary>
public static class ComboReport
{
    public static int Run()
    {
        foreach (var body in new[] { "Small", "Medium", "Large" })
        {
            var probe = RhythmTests.Build(body, UnityEngine.Vector3.zero, 0f, 0);
            var strings = ComboDefinition.For(probe.Loadout.Main, probe.Body.SizeFactor);
            RhythmTests.Destroy(probe);
            foreach (var combo in strings)
            {
                var played = ComboTests.Play(body, combo.name, out var unit, out var enemy);
                try
                {
                    var gaps = new System.Collections.Generic.List<string>();
                    for (int i = 1; i < played.starts.Count; i++)
                    {
                        gaps.Add($"{played.starts[i].time - played.starts[i - 1].time:0.00}");
                    }

                    var moves = new System.Collections.Generic.List<string>();
                    foreach (var s in played.starts)
                    {
                        moves.Add(s.state.Move.name.Replace("MD_", "") + "@" + s.state.Part);
                    }

                    Console.WriteLine($"{body,-6} {combo.name,-22} {played.starts.Count}/{combo.links.Length} links: {string.Join(" > ", moves)}; starts {string.Join(", ", gaps)} s apart; "
                                      + $"{played.changes[played.changes.Count - 1]}; hand-over pop {played.pop * 100f:0.0} cm");
                }
                finally
                {
                    RhythmTests.Destroy(unit, enemy);
                }
            }
        }

        return 0;
    }
}
