// `balance [dir]`: knocks each body down (face down and face up) and stumbles it, printing how the
// get-up and the stumble go, and (with a folder) writes every step's animated pose to CSV for
// Tools/Playtest/getups.py to draw.
using System;
using System.Globalization;
using System.IO;
using System.Text;
using PhysicsBattler.Combat;
using PhysicsBattler.Rig;
using PhysicsBattler.Tests;
using PhysicsBattler.Units;
using UnityEngine;

internal static class BalanceReport
{
    public static int Run(string dir)
    {
        if (dir != null) Directory.CreateDirectory(dir);
        var ci = CultureInfo.InvariantCulture;
        int skip = int.TryParse(Environment.GetEnvironmentVariable("BALANCE_SKIP"), out int n0) ? n0 : 0;
        for (int i = 0; i < skip; i++) BalanceTests.Destroy(BalanceTests.Build("Small", Vector3.zero, 0f, 0));
        string only = Environment.GetEnvironmentVariable("BALANCE_ONLY");
        foreach (string body in new[] { "Small", "Medium", "Large" })
        {
            foreach (bool faceDown in new[] { true, false })
            {
                if (only != null && !$"{body}_{faceDown}".Contains(only)) continue;
                var unit = BalanceTests.Build(body, Vector3.zero, 30f, 0);
                // (The medium with its shield, as it fights: milestone 9c's shield get-ups.)
                unit.Equip(WeaponDefinition.CreateDefault(WeaponKind.Sword), body == "Medium" ? WeaponDefinition.CreateDefault(WeaponKind.Shield) : null);
                BalanceTests.Simulate(new[] { unit }, 10);
                unit.Knock(unit.Root.forward * (faceDown ? 2.6f : -2.6f) * Mathf.Sqrt(unit.Body.SizeFactor));
                BalanceTests.Lay(unit, faceDown);
                var csv = new StringBuilder("t,state");
                foreach (var id in PartInfo.All) csv.Append($",{id}.x,{id}.y,{id}.z");
                csv.AppendLine();
                var last = new Vector3[PartInfo.Count];
                float jump = 0f, jumpAt = 0f; string jumpPart = "";
                bool started = false;
                float upAt = -1f;
                for (int t = 0; t < 60 * 8 && unit.State != UnitState.Standing; t++)
                {
                    BalanceTests.Simulate(new[] { unit });
                    if (unit.State != UnitState.GettingUp) continue;
                    if (upAt < 0f) upAt = t / 60f;
                    csv.Append(unit.StateTime.ToString("0.000", ci)).Append(',').Append(unit.State);
                    foreach (var id in PartInfo.All)
                    {
                        Vector3 p = unit.Anim.Part(id).position;
                        csv.Append(',').Append(p.x.ToString("0.000", ci)).Append(',').Append(p.y.ToString("0.000", ci)).Append(',').Append(p.z.ToString("0.000", ci));
                        if (started && Vector3.Distance(p, last[(int)id]) > jump) { jump = Vector3.Distance(p, last[(int)id]); jumpAt = unit.StateTime; jumpPart = id.ToString(); }
                        last[(int)id] = p;
                    }
                    started = true;
                    csv.AppendLine();
                }
                for (int t = 0; t < 30; t++)
                {
                    BalanceTests.Simulate(new[] { unit });
                    csv.Append((10f + t / 60f).ToString("0.000", ci)).Append(",Standing");
                    foreach (var id in PartInfo.All)
                    {
                        Vector3 p = unit.Anim.Part(id).position;
                        csv.Append(',').Append(p.x.ToString("0.000", ci)).Append(',').Append(p.y.ToString("0.000", ci)).Append(',').Append(p.z.ToString("0.000", ci));
                    }
                    csv.AppendLine();
                }
                string name = $"{body.ToLowerInvariant()}_{(faceDown ? "facedown" : "faceup")}";
                Console.WriteLine($"{name}: down {upAt:0.00} s, getting up {unit.LastGetUp}, state {unit.State}, biggest step {jump * 100f:0.0} cm ({jumpPart} at {jumpAt:0.00} s), root ({unit.Root.position.x:0.00}, {unit.Root.position.z:0.00}) yaw {unit.FacingYaw:0}");
                if (dir != null) File.WriteAllText(Path.Combine(dir, name + ".csv"), csv.ToString());
                BalanceTests.Destroy(unit);
            }

            // A stumble.
            var s = BalanceTests.Build(body, Vector3.zero, 0f, 0);
            BalanceTests.Simulate(new[] { s }, 30);
            int before = s.Stance.StepsTaken;
            s.Knock(Vector3.back * 1.6f * Mathf.Sqrt(s.Body.SizeFactor));
            int n = 0;
            var line = new StringBuilder();
            for (; n < 300 && s.State == UnitState.Stumbling; n++)
            {
                BalanceTests.Simulate(new[] { s });
                if (n % 6 == 0) line.Append($" {s.StumbleVelocity.magnitude:0.00}");
            }
            Console.WriteLine($"{body} stumble: {n / 60f:0.00} s, {s.Stance.StepsTaken - before} steps, moved {-s.Root.position.z:0.00} m; speed every 0.1 s:{line}");
            BalanceTests.Destroy(s);
        }
        return 0;
    }
}
