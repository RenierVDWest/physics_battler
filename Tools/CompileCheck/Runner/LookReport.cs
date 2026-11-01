// `look [dir]`: milestone 9's look runner. A medium sword-and-shield unit stands at the origin
// facing +Z (it doesn't turn); two enemies walk round it, one close and one further out, at
// different speeds, and now and then one swings at it. Prints how often the head is within 25
// degrees of what it attends to, whether the head ever leaves its limits, and how the unit
// perceived each enemy over the run; with a folder, writes every step to CSV for
// Tools/Playtest/lookview.py to draw (the view zones, what it sees, where it looks).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using PhysicsBattler.Combat;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using PhysicsBattler.Tests;
using PhysicsBattler.Units;
using UnityEngine;

internal static class LookReport
{
    public static int Run(string dir)
    {
        if (dir != null) Directory.CreateDirectory(dir);
        var ci = CultureInfo.InvariantCulture;
        var sword = WeaponDefinition.CreateDefault(WeaponKind.Sword);
        sword.moves = new[] { MoveDefinition.Create(MoveStyle.SwordCut), MoveDefinition.Create(MoveStyle.SwordBackhand) };
        UnitController.ResetSeeds(1);
        var unit = ShieldReport.Build("Medium", Vector3.zero, 0f, 0, CombatStyle.CreateFor("Medium"));
        unit.Equip(sword, WeaponDefinition.CreateDefault(WeaponKind.Shield));
        var near = ShieldReport.Build("Medium", Vector3.forward * 1.5f, 180f, 1, CombatStyle.CreateFor("Medium"));
        var far = ShieldReport.Build("Medium", Vector3.forward * 3f, 180f, 1, CombatStyle.CreateFor("Medium"));
        near.Equip(sword, null);
        far.Equip(sword, null);
        near.RhythmOn = far.RhythmOn = false;
        near.DealsHits = far.DealsHits = false;
        var everyone = new[] { unit, near, far };
        var csv = new StringBuilder("t");
        foreach (var id in PartInfo.All) csv.Append($",{id}.x,{id}.y,{id}.z");
        csv.Append(",eye.x,eye.y,eye.z,fwd.x,fwd.y,fwd.z,point.x,point.y,point.z,kind,near.x,near.z,near.sight,near.swing,far.x,far.z,far.sight,far.swing,facing");
        csv.AppendLine();
        const float dt = 1f / 60f;
        int steps = 0, onIt = 0, reachable = 0, outside = 0;
        var seen = new Dictionary<string, int>();
        float nextSwing = 1.5f;
        var random = new System.Random(3);
        for (int i = 0; i < 12 * 60; i++)
        {
            float t = i * dt;
            // The near one circles slowly one way, the far one faster the other.
            float a = 25f * t, b = -50f * t + 140f;
            if (!near.IsAttacking) near.Place(Quaternion.Euler(0f, a, 0f) * Vector3.forward * 1.5f, a + 180f);
            if (!far.IsAttacking) far.Place(Quaternion.Euler(0f, b, 0f) * Vector3.forward * 3f, b + 180f);
            if (t >= nextSwing && !near.IsAttacking)
            {
                var state = MovePlanner.Prepare(near, unit, sword.moves[random.Next(2)], AimPart.Chest);
                if (state != null) near.StartMove(state);
                nextSwing = t + 2f + 1.5f * (float)random.NextDouble();
            }

            BalanceTests.Simulate(everyone);
            var p = unit.Pose;
            if (Mathf.Abs(p.HeadTurn) > HeadLook.Turn + 0.5f || p.HeadPitch < -HeadLook.Down - 0.5f || p.HeadPitch > HeadLook.Up + 0.5f) outside++;
            steps++;
            if (unit.Look.Current >= 0 && unit.Look.Reachable)
            {
                reachable++;
                onIt += unit.LookOff <= 25f ? 1 : 0;
            }

            foreach (var other in new[] { near, far })
            {
                string k = (other == near ? "near: " : "far: ") + unit.Sees(other);
                seen[k] = (seen.TryGetValue(k, out int n) ? n : 0) + 1;
            }

            if (dir != null)
            {
                csv.Append(t.ToString("0.000", ci));
                foreach (var id in PartInfo.All) P(csv, unit.Anim.Part(id).position, ci);
                P(csv, unit.EyePosition, ci);
                P(csv, unit.EyeRotation * Vector3.forward, ci);
                P(csv, unit.Look.Current >= 0 ? unit.Look.Point : unit.EyePosition, ci);
                csv.Append(',').Append(unit.Look.Kind.Replace(',', ' '));
                foreach (var other in new[] { near, far })
                {
                    Vector3 r = other.Root.position;
                    csv.Append(',').Append(r.x.ToString("0.000", ci)).Append(',').Append(r.z.ToString("0.000", ci))
                        .Append(',').Append(unit.Sees(other)).Append(',').Append(other.IsAttacking ? 1 : 0);
                }

                csv.Append(',').Append(unit.FacingYaw.ToString("0.0", ci)).AppendLine();
            }
        }

        Console.WriteLine($"head within 25° of what it attends to: {100f * onIt / Mathf.Max(1, reachable):0}% of the time it was within reach ({100f * reachable / steps:0}% of the run)");
        Console.WriteLine($"steps with the head outside its limits: {outside}");
        foreach (var pair in seen) Console.WriteLine($"  {pair.Key}: {100f * pair.Value / steps:0}% of the run");
        if (dir != null) File.WriteAllText(Path.Combine(dir, "look.csv"), csv.ToString());
        BalanceTests.Destroy(everyone);
        return 0;
    }

    private static void P(StringBuilder csv, Vector3 v, CultureInfo ci)
    {
        csv.Append(',').Append(v.x.ToString("0.000", ci)).Append(',').Append(v.y.ToString("0.000", ci)).Append(',').Append(v.z.ToString("0.000", ci));
    }
}
