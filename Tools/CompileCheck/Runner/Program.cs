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

        if (args.Length >= 2 && args[0] == "smoke")
        {
            return Smoke.Run(args[1]);
        }

        if (args.Length >= 6 && args[0] == "aimdebug")
        {
            return AimDebug.Run(args[1], args[2], args[3], args[4], float.Parse(args[5], System.Globalization.CultureInfo.InvariantCulture));
        }

        if (args.Length >= 1 && args[0] == "swings")
        {
            return SwingReport.Run(args.Length >= 2 ? args[1] : null);
        }

        if (args.Length >= 1 && args[0] == "balance")
        {
            return BalanceReport.Run(args.Length >= 2 ? args[1] : null);
        }

        if (args.Length >= 1 && args[0] == "aim")
        {
            return AimMatrix.Run(args.Length >= 2 ? float.Parse(args[1], System.Globalization.CultureInfo.InvariantCulture) : 1f);
        }

        int passed = 0, failed = 0;
        var fixtures = typeof(PhysicsBattler.Tests.SupportPolygonTests).Assembly.GetTypes()
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
