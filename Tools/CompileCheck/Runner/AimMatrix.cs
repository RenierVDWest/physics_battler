using PhysicsBattler.Motion;
// Every blow, every body part, every size pair: does the striking part reach the aim point?
//
//   dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll aim [distance in tiles, default 1]
//
// Prints, per attacker and target size, per move and part: the planner's verdict (out of lunge
// range, or its dry-run error) and the error measured when the blow is actually played.
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using PhysicsBattler.Combat;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

internal static class AimMatrix
{
    private static readonly List<string> Self = new List<string>();

    public static int Run(float tiles)
    {
        var sword = WeaponDefinition.CreateDefault(WeaponKind.Sword);
        sword.moves = new[] { MoveDefinition.Create(MoveStyle.SwordCut), MoveDefinition.Create(MoveStyle.SwordBackhand), MoveDefinition.Create(MoveStyle.SwordCleave), MoveDefinition.Create(MoveStyle.SwordThrust) };
        var club = WeaponDefinition.CreateDefault(WeaponKind.Club);
        club.moves = new[] { MoveDefinition.Create(MoveStyle.ClubSwing), MoveDefinition.Create(MoveStyle.ClubSmash), MoveDefinition.Create(MoveStyle.ClubRising) };
        var shield = WeaponDefinition.CreateDefault(WeaponKind.Shield);
        int worst = 0;
        float distance = 1.6f * tiles;
        foreach (var a in new[] { "Small", "Medium", "Large" })
        {
            foreach (var b in new[] { "Small", "Medium", "Large" })
            {
                foreach (var weapon in new[] { sword, club })
                {
                    Console.WriteLine($"\n== {a} with {weapon.kind} -> {b} at {distance:0.0} m");
                    foreach (var move in weapon.moves)
                    {
                        var line = $"  {move.name,-18}";
                        foreach (var part in AimPartInfo.All)
                        {
                            if (!move.targets.Contains(part))
                            {
                                line += $" {part}:   .  ";
                                continue;
                            }

                            var r = Try(a, b, weapon, weapon == sword ? shield : null, move, part, distance);
                            line += $" {part}:{r}";
                            if (r.Contains("!"))
                            {
                                worst++;
                            }
                        }

                        Console.WriteLine(line);
                    }
                }
            }
        }

        Console.WriteLine($"\nblows planned but missing by more than 3 cm: {worst}");
        Console.WriteLine($"blows turned down for passing through their own body (S: cm deep): {Self.Count}");
        foreach (var line in Self)
        {
            Console.WriteLine("  " + line);
        }
        return 0;
    }

    private const BindingFlags Any = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    public static UnitController Build(string body, Vector3 at, float yaw, int team)
    {
        string path = Path.Combine(Application.dataPath, $"_Battler/Data/Rigs/{body.ToLowerInvariant()}_rig.json");
        var rig = RigFile.Parse(File.ReadAllText(path));
        var profile = UnitProfile.CreateDefault();
        profile.body = body;
        if (Environment.GetEnvironmentVariable("AIM_STYLES") != null)
        {
            profile.style = CombatStyle.CreateFor(body);   // (the bodies' own stances, as the setup makes them)
        }

        var root = UnitBuilder.Build(rig, profile, body, _ => null, 0);
        root.transform.position = at;
        root.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        var unit = root.GetComponent<UnitController>();
        unit.Profile = profile;
        unit.Team = team;
        typeof(UnitController).GetMethod("Awake", Any).Invoke(unit, null);
        unit.DealsHits = false;   // the whole blow is measured; a hit would stop it at the body
        return unit;
    }

    /// <summary>"out" (beyond the lunge), "~2.4" (planner says it misses by 2.4 cm), or "1.2" (played: 1.2 cm off; "!" over 3).</summary>
    public static string Try(string a, string b, WeaponDefinition main, WeaponDefinition off, MoveDefinition move, AimPart part, float distance)
    {
        var attacker = Build(a, Vector3.zero, 0f, 0);
        var target = Build(b, new Vector3(0f, 0f, distance), 180f, 1);
        try
        {
            attacker.Equip(main, off);
            for (int i = 0; i < 20; i++)
            {
                attacker.PreviewPose(1f / 60f);
                target.PreviewPose(1f / 60f);
            }

            var state = MovePlanner.Prepare(attacker, target, move, part);
            if (state == null)
            {
                return "  out";
            }

            float planned = attacker.TryOut(state);
            if (planned > MovePlanner.Tolerance)
            {
                return $"~{planned * 100,4:0.0}";
            }

            float clear = attacker.Clearance(state, MovePlanner.ClearanceSamples, out var near, out float when, out bool blade);
            if (clear < -MovePlanner.OwnBodyClearance * attacker.Body.SizeFactor)
            {
                Self.Add($"{a}->{b} {move.name} {part}: {(blade ? "weapon" : "arm")} {clear * 100:0} cm into own {near} at t={when:0.00}");
                return $"S{-clear * 100,4:0}";
            }

            float played = -1f;
            void OnContact(UnitController u, MoveState s, ContactReport r)
            {
                if (u == attacker) played = r.AnimError;
            }

            UnitController.MoveContact += OnContact;
            attacker.StartMove(state);
            for (int i = 0; i < 300 && attacker.IsAttacking; i++)
            {
                attacker.PreviewPose(1f / 60f);
            }

            UnitController.MoveContact -= OnContact;
            return $" {played * 100,4:0.0}{(played > MovePlanner.Tolerance || played < 0f ? "!" : " ")}";
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(attacker.gameObject);
            UnityEngine.Object.DestroyImmediate(target.gameObject);
        }
    }
}
