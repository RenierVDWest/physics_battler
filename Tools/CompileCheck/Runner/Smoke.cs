// A dry run of the battle sandbox outside Unity: three units a side built from the body rig files,
// armed as the default lineup (small and large with clubs, medium with sword and shield), facing
// each other across adjacent rows and fighting through the sandbox's own Brawl, stepped by hand
// with a tiny integrator standing in for PhysX (kinematic parts arrive where they were moved;
// dynamic parts move by the velocity the authority layer gave them, plus gravity). It exercises the
// pose pipeline, the moves, the authority layer and the recorder end to end, so
// Tools/Playtest/analyze.py can be tested on real output.
//
//   dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll smoke <output folder>
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using PhysicsBattler.Board;
using PhysicsBattler.Combat;
using PhysicsBattler.Sandbox;
using PhysicsBattler.Diagnostics;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

internal static class Smoke
{
    private const BindingFlags Any = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    public static int Run(string output)
    {
        string assets = Application.dataPath;
        Directory.CreateDirectory(output);
        var rigs = new Dictionary<string, RigFile>();
        foreach (var body in new[] { "Small", "Medium", "Large" })
        {
            rigs[body] = RigFile.Parse(File.ReadAllText(Path.Combine(assets, $"_Battler/Data/Rigs/{body.ToLowerInvariant()}_rig.json")));
        }

        Application.dataPath = Path.Combine(Path.GetFullPath(output), "Assets");
        var recorderObject = new GameObject("Recorder");
        var recorder = recorderObject.AddComponent<PlaytestRecorder>();
        Call(recorder, "OnEnable");

        var grid = new BoardGrid();
        var units = new List<UnitController>();
        // As the default lineup: team A small, medium, large; team B medium, large, small (two rows apart).
        var tiles = new[] { new[] { new Tile(2, 2), new Tile(3, 2), new Tile(4, 2) }, new[] { new Tile(4, 4), new Tile(2, 4), new Tile(3, 4) } };
        var sword = WeaponDefinition.CreateDefault(WeaponKind.Sword);
        sword.moves = new[] { MoveDefinition.Create(MoveStyle.SwordCut), MoveDefinition.Create(MoveStyle.SwordBackhand), MoveDefinition.Create(MoveStyle.SwordCleave), MoveDefinition.Create(MoveStyle.SwordThrust) };
        var club = WeaponDefinition.CreateDefault(WeaponKind.Club);
        club.moves = new[] { MoveDefinition.Create(MoveStyle.ClubSwing), MoveDefinition.Create(MoveStyle.ClubSmash), MoveDefinition.Create(MoveStyle.ClubRising), MoveDefinition.Create(MoveStyle.ClubBackhand), MoveDefinition.Create(MoveStyle.ClubFan), MoveDefinition.Create(MoveStyle.ClubFanBackhand) };
        var shield = WeaponDefinition.CreateDefault(WeaponKind.Shield);
        var styles = new Dictionary<string, CombatStyle>();
        for (int team = 0; team < 2; team++)
        {
            int k = 0;
            foreach (var body in new[] { "Small", "Medium", "Large" })
            {
                var profile = UnitProfile.CreateDefault();
                profile.name = "UP_" + body;
                profile.body = body;
                if (!styles.TryGetValue(body, out var style))
                {
                    style = CombatStyle.CreateFor(body);   // (as the setup makes them: the captured idles and walks)
                    styles[body] = style;
                }

                profile.style = style;
                var unit = Spawn(rigs[body], profile, $"{(team == 0 ? "A" : "B")} {body}", grid.Center(tiles[team][k]), BoardGrid.TeamYaw(team), team);
                if (body == "Medium")
                {
                    unit.Equip(sword, shield);
                }
                else
                {
                    unit.Equip(club, null);
                }

                units.Add(unit);
                k++;
            }
        }

        float dt = Time.fixedDeltaTime;
        var brawl = new Brawl(7) { Grid = grid };
        for (int step = 0; step < 60 * 14; step++)
        {
            Time.time += dt;
            Call(recorder, "FixedUpdate");
            brawl.Fight = step < 60 * 10;
            brawl.TurnedAway = step >= 60 * 11 && step < 60 * 12;
            brawl.Step(units, dt, Time.time);
            foreach (var unit in units)
            {
                Call(unit, "FixedUpdate");
                Integrate(unit, dt);
            }

            if (step == 60 * 6)
            {
                recorder.Mark("smoke test mark");
                Application.RaiseLog("A warning from the smoke test", LogType.Warning);
            }

            if (step == 60 * 9)
            {
                Time.timeScale = 0.5f;
                Call(recorder, "Update");
            }
        }

        Call(recorder, "OnDisable");
        Console.WriteLine("smoke session: " + recorder.SessionPath);
        return 0;
    }

    private static UnitController Nearest(List<UnitController> units, UnitController unit)
    {
        UnitController best = null;
        float d = float.MaxValue;
        foreach (var other in units)
        {
            if (other.Team == unit.Team) continue;
            float dd = (other.Root.position - unit.Root.position).sqrMagnitude;
            if (dd < d) { d = dd; best = other; }
        }

        return best;
    }

    private static UnitController Spawn(RigFile rig, UnitProfile profile, string name, Vector3 position, float yaw, int team)
    {
        var root = UnitBuilder.Build(rig, profile, name, _ => null, 8);
        root.name = name;
        root.transform.position = position;
        root.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        var unit = root.GetComponent<UnitController>();
        unit.Team = team;
        unit.Profile = profile;
        Call(unit, "Awake");
        return unit;
    }

    /// <summary>Stands in for PhysX's step: dynamic parts move by their velocity (gravity added first).</summary>
    // Gravity, the ground and friction (a knocked-down body comes to rest on it), as the tests do.
    private static void Integrate(UnitController unit, float dt) => PhysicsBattler.Tests.BalanceTests.Integrate(unit, dt);

    private static void Call(object target, string method)
    {
        target.GetType().GetMethod(method, Any)?.Invoke(target, null);
    }
}
