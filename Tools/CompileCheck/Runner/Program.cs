// Runs the EditMode tests outside Unity, on the stubs (whose vector maths is real).
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using NUnit.Framework;

internal static class Program
{
    private static int Main(string[] args)
    {
        var options = new JsonSerializerOptions { IncludeFields = true };
        UnityEngine.JsonUtility.Deserializer = (json, type) => JsonSerializer.Deserialize(json, type, options);
        UnityEngine.Application.dataPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../Assets"));
        var writeOptions = new JsonSerializerOptions { IncludeFields = true, WriteIndented = true };
        UnityEngine.JsonUtility.Serializer = (obj, pretty) =>
        {
            try { return JsonSerializer.Serialize(obj, obj.GetType(), writeOptions); }
            catch (Exception e) { return "{\"error\": \"" + e.GetType().Name + "\"}"; }
        };

        if (Environment.GetEnvironmentVariable("OLD_WALK") != null)
        {
            PhysicsBattler.Units.UnitController.NewWalk = false;   // (the walk before the locomotion lab)
        }

        // (SOUND_LOG=1: the sounds the run asked for, counted, printed at its end.)
        var heard = new System.Collections.Generic.SortedDictionary<string, int>();
        if (Environment.GetEnvironmentVariable("SOUND_LOG") != null)
        {
            PhysicsBattler.Audio.SoundHooks.Install();
            PhysicsBattler.Audio.Sfx.Requested += (id, volume) => heard[id] = heard.TryGetValue(id, out int n) ? n + 1 : 1;
            AppDomain.CurrentDomain.ProcessExit += (_, _) => Console.WriteLine("Sounds asked for: " + (heard.Count == 0 ? "none" : string.Join(", ", heard.Select(h => $"{h.Key} {h.Value}"))));
        }

#if PHYSX
        if (args.Length >= 1 && args[0] == "physx-hit")
        {
            return HitExperiment.Run(args);
        }

        if (args.Length >= 1 && args[0] == "physx-arm")
        {
            return ArmExperiment.Run(args);
        }

        if (args.Length >= 1 && args[0] == "physx")
        {
            return PhysXReport.Run(args);
        }
#endif

        if (args.Length >= 2 && args[0] == "combos")
        {
            return StrikeComboReport.Run(args[1], args.Length >= 3 ? args[2] : null);
        }

        if (args.Length >= 2 && args[0] == "cores")
        {
            return CoreReport.Run(args[1]);
        }

        if (args.Length >= 1 && args[0] == "parryreach")
        {
            return ParryReachReport.Run(args.Length >= 2 ? args[1] : null);
        }

        if (args.Length >= 2 && args[0] == "footwork")
        {
            return FootworkReport.Run(args[1], args.Length >= 3 ? float.Parse(args[2], System.Globalization.CultureInfo.InvariantCulture) : 30f);
        }

        if (args.Length >= 2 && args[0] == "hitlook")
        {
            return SparReport.RunHitLook(args[1], args.Length >= 3 ? args[2] : "shield");
        }

        if (args.Length >= 2 && args[0] == "spar")
        {
            return SparReport.Run(args[1], args.Length >= 3 ? float.Parse(args[2], System.Globalization.CultureInfo.InvariantCulture) : 40f);
        }

        if (args.Length >= 2 && args[0] == "dummyhits")
        {
            return DummyHitReport.Run(args[1], args.Length >= 3 ? args[2] : null, args.Length >= 4 ? args[3] : null);
        }

        if (args.Length >= 2 && args[0] == "strikes")
        {
            return StrikeAimReport.Run(args[1], args.Length >= 3 ? args[2] : null, args.Length >= 4 ? args[3] : null);
        }

        if (args.Length >= 2 && args[0] == "combatfilm")
        {
            return CombatFilmReport.Run(args[1], args.Length >= 3 ? args[2] : null, args.Length >= 4 ? args[3] : null);
        }

        if (args.Length >= 1 && args[0] == "armgate")
        {
            return ArmGateReport.Run();
        }

        if (args.Length >= 1 && args[0] == "legrange")
        {
            return LegRangeReport.Run();
        }

        if (args.Length >= 1 && args[0] == "armrange")
        {
            return ArmRangeReport.Run();
        }

        if (args.Length >= 2 && args[0] == "fight")
        {
            return FightReport.Run(args[1], args.Length >= 3 ? int.Parse(args[2]) : 7);
        }

        if (args.Length >= 1 && args[0] == "locolab")
        {
            return LocoLabReport.Run(args.Length >= 2 ? args[1] : ".", args.Length >= 3 ? float.Parse(args[2], System.Globalization.CultureInfo.InvariantCulture) : 200f);
        }

        int passed = 0, failed = 0;
        var fixtures = typeof(Program).Assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<TestFixtureAttribute>() != null).OrderBy(t => t.Name);
        foreach (var fixture in fixtures)
        {
            string only = Environment.GetEnvironmentVariable("TEST_ONLY");
            foreach (var method in fixture.GetMethods().Where(m => m.GetCustomAttribute<TestAttribute>() != null))
            {
                if (!string.IsNullOrEmpty(only) && !$"{fixture.Name}.{method.Name}".Contains(only))
                {
                    continue;
                }

                var instance = Activator.CreateInstance(fixture);
                try
                {
                    method.Invoke(instance, null);
                    passed++;
                    Console.WriteLine($"  pass  {fixture.Name}.{method.Name}");
                }
                catch (TargetInvocationException e)
                {
                    failed++;
                    Console.WriteLine($"  FAIL  {fixture.Name}.{method.Name}: {e.InnerException?.Message}");
                }
            }
        }

        Console.WriteLine($"{passed} passed, {failed} failed");
        return failed == 0 ? 0 : 1;
    }
}
