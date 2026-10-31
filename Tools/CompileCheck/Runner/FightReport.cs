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
        void OnMove(UnitController u, MoveState s) { started[u] = (started.TryGetValue(u, out int n) ? n : 0) + 1; attacks++; }
        UnitController.MoveStarted += OnMove;
        int steps = (int)(limit * 60f) + 600;
        float startTime = Time.time;
        try
        {
            for (int step = 0; step < steps && !loop.Over; step++)
            {
                Time.time += dt;
                Call(recorder, "FixedUpdate");

                loop.TimeLimit = limit;
                loop.Step(units, dt, Time.time);
                foreach (var unit in units)
                {
                    Call(unit, "FixedUpdate");
                    PhysicsBattler.Tests.BalanceTests.Integrate(unit, dt);
                }

                // Two units standing on one tile (not in passing).
                var on = new Dictionary<long, UnitController>();
                foreach (var u in units)
                {
                    if (u.IsAlive && u.State == UnitState.Standing && loop.TileOf(u, out var t))
                    {
                        long key = ((long)t.column << 16) | (uint)t.row;
                        if (on.TryGetValue(key, out var other))
                        {
                            if (shared++ % 120 == 0 && Environment.GetEnvironmentVariable("FIGHT_DEBUG") != null)
                            {
                                Console.WriteLine($"  t {Time.time - startTime:0.0}: {u.name} and {other.name} both on {t} (roots {u.Root.position} {other.Root.position}; {(u.NeedsSpot ? "needs spot" : "")} {(other.NeedsSpot ? "needs spot" : "")})");
                            }
                        }
                        else
                        {
                            on[key] = u;
                        }
                    }
                }
            }
        }
        finally
        {
            UnitController.MoveStarted -= OnMove;
        }

        var sb = new System.Text.StringBuilder();
        foreach (var u in units)
        {
            sb.AppendLine($"  {u.name}: {(u.IsAlive ? $"alive, {u.Health:0}/{u.Profile.maxHealth:0} hp" : "dead")}{(u.Loadout?.Main == null ? ", unarmed" : "")}{(u.LegLost ? ", legless" : "")}, "
                          + $"{(started.TryGetValue(u, out int n) ? n : 0)} blows, {u.SeveredLimbs.Count} limbs lost");
        }

        sb.Append($"  steps with two standing units on one tile: {shared}");
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
