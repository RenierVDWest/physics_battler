// `rhythms [dir] [body] [posture]`: plays each style's base and every posture on its own body
// (facing an opponent, so feints have something to aim at) and writes each one's animated pose,
// weapon and shield per step to CSV for Tools/Playtest/rhythms.py to draw; prints the numbers.
using System;
using System.Globalization;
using System.IO;
using System.Text;
using PhysicsBattler.Combat;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using PhysicsBattler.Tests;
using PhysicsBattler.Units;
using UnityEngine;

internal static class RhythmReport
{
    public static int Run(string dir, string onlyBody, string onlyPosture)
    {
        if (dir != null) Directory.CreateDirectory(dir);
        var ci = CultureInfo.InvariantCulture;
        int skip = int.TryParse(Environment.GetEnvironmentVariable("RHYTHM_SKIP"), out int n0) ? n0 : 0;
        for (int i = 0; i < skip; i++) RhythmTests.Destroy(RhythmTests.Build("Small", Vector3.zero, 0f, 0));
        foreach (string body in new[] { "Large", "Medium", "Small" })
        {
            if (onlyBody != null && body != onlyBody) continue;
            var (unit, opponent) = RhythmTests.Pair(body);
            var both = new[] { unit, opponent };
            RhythmTests.Step(both, 30);
            unit.Rhythm.EnergyOverride = 1f;
            var list = new System.Collections.Generic.List<PostureDefinition> { null };
            list.AddRange(unit.Rhythm.Definition.postures);
            foreach (var posture in list)
            {
                string name = posture != null ? posture.Label : "Base";
                if (onlyPosture != null && name != onlyPosture) continue;
                var csv = new StringBuilder("t,u,open");
                foreach (var id in PartInfo.All) csv.Append($",{id}.x,{id}.y,{id}.z");
                csv.Append(",grip.x,grip.y,grip.z,tip.x,tip.y,tip.z,sh.x,sh.y,sh.z,shn.x,shn.y,shn.z");
                csv.AppendLine();
                if (posture != null) unit.Rhythm.Force(posture);
                float t = 0f, minClear = float.MaxValue, pop = 0f;
                string where = "", popWhere = "";
                var lastP = new Vector3[PartInfo.Count];
                var lastD = new float[PartInfo.Count];
                int frames = 0;
                for (int i = 0; i < (posture != null ? 200 : 600); i++)
                {
                    RhythmTests.Step(both);
                    t += 1f / 60f;
                    if (posture != null && unit.Rhythm.Current != posture) break;
                    float c = BodyClearance.Measure(unit.Rig, unit.Anim, unit.Loadout.Main.length * 0.98f, unit.Loadout.Scale, out var part, out bool blade);
                    if (c < minClear) { minClear = c; where = $"{(blade ? "weapon" : "arm")} on {part} at {unit.Rhythm.Progress:0.00}"; }
                    foreach (var id in PartInfo.All)
                    {
                        Vector3 q = unit.Anim.Part(id).position;
                        if (frames > 0)
                        {
                            float dd = Vector3.Distance(q, lastP[(int)id]);
                            if (frames > 1 && dd - 2f * lastD[(int)id] > pop) { pop = dd - 2f * lastD[(int)id]; popWhere = $"{id} at {t:0.00} s ({(unit.Rhythm.Current != null ? unit.Rhythm.Current.Label + " " + unit.Rhythm.Progress.ToString("0.00") : "base")}, move {(unit.Pose.Move != null ? unit.Pose.Move.T.ToString("0.000") : "-")})"; }
                            lastD[(int)id] = dd;
                        }
                        lastP[(int)id] = q;
                    }
                    frames++;
                    csv.Append(t.ToString("0.000", ci)).Append(',').Append(unit.Rhythm.Progress.ToString("0.000", ci)).Append(',').Append(unit.Openings.ToString().Replace(",", "|").Replace(" ", ""));
                    foreach (var id in PartInfo.All) Add(csv, unit.Anim.Part(id).position, ci);
                    var slot = unit.Anim.SlotWeaponR;
                    Add(csv, slot.position, ci);
                    Add(csv, slot.position + slot.rotation * (Vector3.forward * unit.Loadout.Main.length * unit.Loadout.Scale), ci);
                    var sl = unit.Anim.SlotWeaponL;
                    Add(csv, sl.position, ci);
                    Add(csv, sl.position + sl.rotation * (Vector3.forward * 0.3f), ci);
                    csv.AppendLine();
                }
                Console.WriteLine($"{body} {name}: {t:0.00} s, closest to own body {minClear * 100f:0.0} cm ({where}), pop {pop * 100f:0.0} cm {popWhere}");
                if (dir != null) File.WriteAllText(Path.Combine(dir, $"{body.ToLowerInvariant()}_{name}.csv"), csv.ToString());
                RhythmTests.Step(both, 20);
            }
            RhythmTests.Destroy(both);
        }
        return 0;
    }

    private static void Add(StringBuilder csv, Vector3 p, CultureInfo ci)
    {
        csv.Append(',').Append(p.x.ToString("0.000", ci)).Append(',').Append(p.y.ToString("0.000", ci)).Append(',').Append(p.z.ToString("0.000", ci));
    }
}
