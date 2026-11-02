// `shells [dir]`: milestone 11's guards posed, for Tools/CompileCheck/shellpose.py to draw: each body
// in its plain guard, shelled up, with its guard broken open, and parrying a cut (no shield) or
// blocking it (a shield); every part, the weapon's grip and tip, and the head's size.
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using PhysicsBattler.Combat;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using PhysicsBattler.Tests;
using PhysicsBattler.Units;
using UnityEngine;

internal static class ShellReport
{
    public static int Run(string outDir)
    {
        Directory.CreateDirectory(outDir);
        string V(Vector3 v) => string.Format(CultureInfo.InvariantCulture, "[{0:0.###},{1:0.###},{2:0.###}]", v.x, v.y, v.z);
        var json = new StringBuilder("[");
        bool firstShot = true;
        foreach (string body in new[] { "Large", "Medium", "Small" })
        {
            var (unit, opponent) = RhythmTests.Pair(body);
            unit.TempoOn = true;
            unit.TempoEngaged = true;
            var both = new[] { unit, opponent };
            try
            {
                void Shot(string name)
                {
                    var sb = new StringBuilder("{");
                    sb.Append($"\"body\":\"{body}\",\"name\":\"{name}\",\"head\":{(0.12f * unit.Body.SizeFactor).ToString("0.###", CultureInfo.InvariantCulture)},\"pose\":{{");
                    foreach (PartId id in Enum.GetValues(typeof(PartId)))
                    {
                        sb.Append($"\"{id}\":{V(unit.Anim.Part(id).position)},");
                    }

                    sb.Length--;
                    sb.Append("},");
                    var slot = unit.Anim.SlotWeaponR;
                    var main = unit.Loadout.Main;
                    sb.Append($"\"grip\":{V(slot.position)},\"tip\":{V(slot.position + slot.rotation * (Vector3.forward * main.length * unit.Loadout.Scale))}");
                    if (unit.Loadout.HasShield)
                    {
                        sb.Append($",\"shield\":{V(unit.Pose.ShieldCenter)},\"shieldFace\":{V(unit.Pose.ShieldFace)},\"shieldRadius\":{(unit.Loadout.Off.radius * unit.Loadout.Scale).ToString("0.###", CultureInfo.InvariantCulture)}");
                    }

                    sb.Append($",\"opponent\":{V(opponent.Anim.Part(PartId.Chest).position)}");
                    float clear = BodyClearance.Measure(unit.Rig, unit.Anim, main.length * 0.98f, unit.Loadout.Scale, out var part, out bool blade);
                    sb.Append($",\"clear\":\"{clear * 100f:0.0} cm ({(blade ? "weapon" : "arm")} on the {part})\"}}");
                    json.Append(firstShot ? "" : ",").Append(sb);
                    firstShot = false;
                    Console.WriteLine($"{body} {name}: clearance {clear * 100f:0.0} cm ({(blade ? "weapon" : "arm")} on the {part})");
                }

                RhythmTests.Step(both, 40);
                unit.Tempo.Force(TempoState.Probe);
                RhythmTests.Step(both, 30);
                Shot("guard");
                for (int i = 0; i < 3; i++)
                {
                    unit.Tempo.NoteIncoming(true);
                }

                unit.Tempo.Force(TempoState.Shell);
                RhythmTests.Step(both, 40);
                Shot("shell");
                var open = typeof(UnitController).GetProperty("GuardOpenUntil");
                open.SetValue(unit, unit.LocalTime + 0.6f);
                RhythmTests.Step(both, 12);
                Shot("guard broken");
                RhythmTests.Step(both, 60);

                // A cut at its chest, seen: parried (or blocked).
                unit.Tempo.Force(TempoState.Probe);
                unit.Style.parryChance = 1f;
                unit.Style.blockChance = 1f;
                opponent.DealsHits = false;
                RhythmTests.Step(both, 30);
                var state = MovePlanner.Prepare(opponent, unit, opponent.Loadout.Main.moves[0], AimPart.Head);
                if (state != null && opponent.StartMove(state))
                {
                    float best = 0f;
                    for (int i = 0; i < 200 && opponent.IsAttacking; i++)
                    {
                        RhythmTests.Step(both);
                        float b = unit.Loadout.HasShield ? unit.BlockBlend : unit.ParryBlend;
                        if (b > best + 1e-3f || (b >= 0.999f && best < 0.999f))
                        {
                            best = b;
                            if (b >= 0.999f)
                            {
                                Shot(unit.Loadout.HasShield ? "block (cut at the head)" : "parry (cut at the head)");
                                break;
                            }
                        }
                    }

                    if (best < 0.999f)
                    {
                        Console.WriteLine($"{body}: no full parry or block ({best:0.00})");
                    }
                }
            }
            finally
            {
                RhythmTests.Destroy(both);
            }
        }

        json.Append("]");
        File.WriteAllText(Path.Combine(outDir, "shells.json"), json.ToString());
        return 0;
    }
}
