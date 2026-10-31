// A dry run of the battle sandbox outside Unity: three units a side built from the body rig files,
// stepped by hand on the board, with a tiny integrator standing in for PhysX (kinematic parts arrive
// where they were moved; dynamic parts move by the velocity the authority layer gave them, plus
// gravity). It exercises the pose pipeline, the authority layer and the recorder end to end, so
// Tools/Playtest/analyze.py can be tested on real output.
//
//   dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll smoke <output folder>
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using PhysicsBattler.Board;
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
        var tiles = new[] { new[] { new Tile(2, 2), new Tile(4, 2), new Tile(6, 2) }, new[] { new Tile(5, 5), new Tile(3, 5), new Tile(1, 5) } };
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
                    style = CombatStyle.CreateDefault();
                    style.name = "CS_" + body;
                    if (body == "Small") { style.swayAmplitude = 0.05f; style.bobAmplitude = 0.025f; style.shuffleInterval = 1.4f; }
                    if (body == "Large") { style.swayAmplitude = 0.018f; style.stanceWidth = 0.4f; style.shuffleInterval = 5f; }
                    styles[body] = style;
                }

                profile.style = style;
                var unit = Spawn(rigs[body], profile, $"{(team == 0 ? "A" : "B")} {body}", grid.Center(tiles[team][k]), BoardGrid.TeamYaw(team), team);
                units.Add(unit);
                k++;
            }
        }

        float dt = Time.fixedDeltaTime;
        bool away = false;
        for (int step = 0; step < 60 * 12; step++)
        {
            Time.time += dt;
            Call(recorder, "FixedUpdate");
            if (step == 60 * 4) away = true;
            if (step == 60 * 7) away = false;
            foreach (var unit in units)
            {
                var enemy = Nearest(units, unit);
                Vector3 point = enemy.Root.position;
                unit.FacePoint(away ? unit.Root.position * 2f - point : point);
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
    private static void Integrate(UnitController unit, float dt)
    {
        foreach (var id in PartInfo.All)
        {
            var body = unit.Rig.Part(id).body;
            if (body.isKinematic)
            {
                continue;
            }

            body.linearVelocity += Physics.gravity * dt;
            body.position += body.linearVelocity * dt;
            Vector3 w = body.angularVelocity;
            float angle = w.magnitude * dt * Mathf.Rad2Deg;
            if (angle > 1e-5f)
            {
                body.rotation = Quaternion.AngleAxis(angle, w.normalized) * body.rotation;
            }
        }
    }

    private static void Call(object target, string method)
    {
        target.GetType().GetMethod(method, Any)?.Invoke(target, null);
    }
}
