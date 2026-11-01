// `fight [dir] [seed]`: a whole 3 v 3 fight on the board with no help (milestone 6's gate), stepped
// as the smoke run is (the stand-in integrator for PhysX), teams starting at opposite ends. Records
// a playtest session in <dir> (analyze it with Tools/Playtest/analyze.py) and prints how it went.
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using PhysicsBattler.Battle;
using PhysicsBattler.Board;
using PhysicsBattler.Combat;
using PhysicsBattler.Diagnostics;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

internal static class FightReport
{
    private const BindingFlags Any = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    /// <summary>Called after every step of a fight (the `arms` runner measures the poses here).</summary>
    public static Action<List<UnitController>, float> OnStep;

    public static int Run(string output, int seed)
    {
        Directory.CreateDirectory(output);
        var rigs = new Dictionary<string, RigFile>();
        foreach (var body in new[] { "Small", "Medium", "Large" })
        {
            rigs[body] = RigFile.Parse(File.ReadAllText(Path.Combine(Application.dataPath, $"_Battler/Data/Rigs/{body.ToLowerInvariant()}_rig.json")));
        }

        string assets = Application.dataPath;
        Application.dataPath = Path.Combine(Path.GetFullPath(output), "Assets");
        var recorder = new GameObject("Recorder").AddComponent<PlaytestRecorder>();
        Call(recorder, "OnEnable");
        var result = Play(rigs, seed, 240f, recorder, out var stats);
        Call(recorder, "OnDisable");
        Application.dataPath = assets;
        Console.WriteLine(result);
        Console.WriteLine(stats);
        Console.WriteLine("fight session: " + recorder.SessionPath);
        return 0;
    }

    /// <summary>Plays a fight to its end (or the time limit); the result line, and per-unit figures.</summary>
    public static string Play(Dictionary<string, RigFile> rigs, int seed, float limit, PlaytestRecorder recorder, out string stats)
    {
        var grid = new BoardGrid();
        var lineup = new[]
        {
            new[] { ("Small", new Tile(2, 1)), ("Medium", new Tile(3, 1)), ("Large", new Tile(4, 1)) },
            new[] { ("Medium", new Tile(4, 6)), ("Large", new Tile(2, 6)), ("Small", new Tile(3, 6)) },
        };
        if (Environment.GetEnvironmentVariable("FIGHT_MIRROR") != null)
        {
            // (Each team in the other's places, to see whether a side or a slot wins, not the units.)
            var swap = new[] { lineup[1], lineup[0] };
            lineup = new[]
            {
                Array.ConvertAll(swap[0], e => (e.Item1, new Tile(e.Item2.column, 7 - e.Item2.row))),
                Array.ConvertAll(swap[1], e => (e.Item1, new Tile(e.Item2.column, 7 - e.Item2.row))),
            };
        }

        var sword = WeaponDefinition.CreateDefault(WeaponKind.Sword);
        sword.moves = new[] { MoveDefinition.Create(MoveStyle.SwordCut), MoveDefinition.Create(MoveStyle.SwordBackhand), MoveDefinition.Create(MoveStyle.SwordCleave), MoveDefinition.Create(MoveStyle.SwordThrust) };
        var club = WeaponDefinition.CreateDefault(WeaponKind.Club);
        club.moves = new[] { MoveDefinition.Create(MoveStyle.ClubSwing), MoveDefinition.Create(MoveStyle.ClubSmash), MoveDefinition.Create(MoveStyle.ClubRising) };
        var shield = WeaponDefinition.CreateDefault(WeaponKind.Shield);
        var units = new List<UnitController>();
        for (int team = 0; team < 2; team++)
        {
            foreach (var (body, tile) in lineup[team])
            {
                var profile = UnitProfile.CreateDefault();
                profile.name = "UP_" + body;
                profile.body = body;
                profile.style = CombatStyle.CreateFor(body);
                if (body == "Small") { profile.maxHealth = 60f; profile.maxPoise = 25f; }
                if (body == "Large") { profile.maxHealth = 220f; profile.maxPoise = 120f; }
                var root = UnitBuilder.Build(rigs[body], profile, $"{(team == 0 ? "A" : "B")} {body}", _ => null, 8);
                root.transform.position = grid.Center(tile);
                root.transform.rotation = Quaternion.Euler(0f, BoardGrid.TeamYaw(team), 0f);
                var unit = root.GetComponent<UnitController>();
                unit.Team = team;
                unit.Profile = profile;
                Call(unit, "Awake");
                if (body == "Medium") unit.Equip(sword, shield); else unit.Equip(club, null);
                units.Add(unit);
            }
        }

        var loop = new FightLoop(seed) { Grid = grid };
        float dt = Time.fixedDeltaTime;
        int shared = 0, attacks = 0;
        var started = new Dictionary<UnitController, int>();
        var blowsAt = new List<(float t, UnitController by, UnitController at, float duration)>();
        var tempoTime = new Dictionary<string, float>();
        void OnMove(UnitController u, MoveState s)
        {
            started[u] = (started.TryGetValue(u, out int n) ? n : 0) + 1;
            attacks++;
            blowsAt.Add((Time.time, u, s.Target as UnitController, s.Duration));
        }
        UnitController.MoveStarted += OnMove;
        // Milestone 10: blows inside combos, and the time from one link's start to the next.
        int comboBlows = 0, combos = 0;
        var linkGaps = new List<float>();
        var contactGaps = new List<float>();
        var comboEnds = new Dictionary<string, int>();
        void OnCombo(UnitController u, ComboRun c, string what)
        {
            if (what == "started")
            {
                combos++;
                comboBlows++;
            }
            else if (what.StartsWith("link"))
            {
                comboBlows++;
                if (c.LinkStarts.Count >= 2)
                {
                    linkGaps.Add(c.LinkStarts[c.LinkStarts.Count - 1] - c.LinkStarts[c.LinkStarts.Count - 2]);
                }
            }
            else
            {
                // (The strikes' rhythm: from one link's striking moment to the next's.)
                for (int i = 1; i < c.Contacts.Count; i++)
                {
                    contactGaps.Add(c.Contacts[i] - c.Contacts[i - 1]);
                }

                string why = what.Replace("ended: ", "");
                why = why.StartsWith("stopped by") ? (why.EndsWith("'s shield") ? "blocked" : "stopped in a body") : why;
                comboEnds[why] = (comboEnds.TryGetValue(why, out int k) ? k : 0) + 1;
            }
        }
        UnitController.ComboChanged += OnCombo;
        int steps = (int)(limit * 60f) + 600;
        float startTime = Time.time;
        try
        {
            for (int step = 0; step < steps && !loop.Over; step++)
            {
                Time.time += dt;
                if (recorder != null)
                {
                    Call(recorder, "FixedUpdate");
                }

                loop.TimeLimit = limit;
                loop.Step(units, dt, Time.time);
                foreach (var u in units)
                {
                    if (u.IsAlive && u.State == UnitState.Standing && u.Tempo != null && u.TempoOn)
                    {
                        string k = u.Tempo.State.ToString();
                        tempoTime[k] = (tempoTime.TryGetValue(k, out float sec) ? sec : 0f) + dt;
                    }
                }

                foreach (var unit in units)
                {
                    Call(unit, "FixedUpdate");
                    PhysicsBattler.Tests.BalanceTests.Integrate(unit, dt);
                }

                OnStep?.Invoke(units, dt);

                // Two standing bodies overlapping (free roam: the steering should keep them apart).
                int overlapping = FightLoop.Overlapping(units);
                if (overlapping > 0 && shared++ % 120 == 0 && Environment.GetEnvironmentVariable("FIGHT_DEBUG") != null)
                {
                    Console.WriteLine($"  t {Time.time - startTime:0.0}: {overlapping} pairs of bodies overlapping");
                }
            }
        }
        finally
        {
            UnitController.MoveStarted -= OnMove;
            UnitController.ComboChanged -= OnCombo;
        }

        var sb = new System.Text.StringBuilder();
        foreach (var u in units)
        {
            sb.AppendLine($"  {u.name}: {(u.IsAlive ? $"alive, {u.Health:0}/{u.Profile.maxHealth:0} hp" : "dead")}{(u.Loadout?.Main == null ? ", unarmed" : "")}{(u.LegLost ? ", legless" : "")}, "
                          + $"{(started.TryGetValue(u, out int n) ? n : 0)} blows, {u.SeveredLimbs.Count} limbs lost");
        }

        // The tempo's headline (milestone 7a; Tools/Playtest/tempo.py has the rest): bursts, gaps, taking turns.
        var gaps = new List<float>();
        var afterEnd = new List<float>();
        var lastBy = new Dictionary<UnitController, float>();
        var lastEnd = new Dictionary<UnitController, float>();
        var lastInPair = new Dictionary<(UnitController, UnitController), UnitController>();
        int alternate = 0, same = 0;
        foreach (var (t, by, at, duration) in blowsAt)
        {
            if (lastBy.TryGetValue(by, out float before))
            {
                gaps.Add(t - before);
                afterEnd.Add(t - lastEnd[by]);
            }

            lastBy[by] = t;
            lastEnd[by] = t + duration;
            if (at == null)
            {
                continue;
            }

            var key = units.IndexOf(by) < units.IndexOf(at) ? (by, at) : (at, by);
            if (lastInPair.TryGetValue(key, out var previous))
            {
                if (previous == by) same++; else alternate++;
            }

            lastInPair[key] = by;
        }

        gaps.Sort();
        if (gaps.Count > 0)
        {
            int bursts = afterEnd.FindAll(g => g < 0.6f).Count;
            float total = 0f;
            foreach (var v in tempoTime.Values) total += v;
            var shares = new List<string>();
            foreach (var pair in tempoTime) shares.Add($"{pair.Key.ToLowerInvariant()} {100f * pair.Value / Mathf.Max(1e-3f, total):0}%");
            if (shares.Count > 0) sb.AppendLine("  tempo (time on their feet): " + string.Join(", ", shares));
            sb.AppendLine($"  gaps between a unit's own blows: median {gaps[gaps.Count / 2]:0.0} s; in bursts (next within 0.6 s of the last ending): {100f * bursts / afterEnd.Count:0}%; "
                          + $"within pairs the next blow was the other's {100f * alternate / Mathf.Max(1, alternate + same):0}% of the time");
        }

        linkGaps.Sort();
        var ends = new List<string>();
        foreach (var pair in comboEnds) ends.Add($"{pair.Key} {pair.Value}");
        contactGaps.Sort();
        sb.AppendLine($"  combos: {combos}, {100f * comboBlows / Mathf.Max(1, attacks):0}% of blows inside them (target 25-40%); striking moment to the next: median {(contactGaps.Count > 0 ? contactGaps[contactGaps.Count / 2] : 0f):0.00} s "
                      + $"(target 0.4-0.7, {contactGaps.Count}); link start to the next: median {(linkGaps.Count > 0 ? linkGaps[linkGaps.Count / 2] : 0f):0.00} s; ended: {string.Join(", ", ends)}");
        sb.Append($"  steps with standing bodies overlapping: {shared}; times a unit made no headway and went round: {loop.Stalls}");
        stats = sb.ToString();
        string result = loop.Over ? $"fight over: {loop.Result}" : $"not over after {Time.time - startTime:0} s";
        foreach (var u in units)
        {
            Call(u, "OnDestroy");
            UnityEngine.Object.DestroyImmediate(u.gameObject);
        }

        return result + $"; {attacks} blows in all";
    }

    private static void Call(object target, string method)
    {
        target?.GetType().GetMethod(method, Any)?.Invoke(target, null);
    }
}
