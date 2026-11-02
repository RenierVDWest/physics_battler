// `guard [seed]`: milestone 10. A whole fight (as `fight`): the weapon arm held in the guard (pinned,
// no blow), its physical parts' turn off the pose (the playtests' guard arm stuck 20-30 degrees off
// it), and the posed parts' own turn from one step to the next.
using System;
using System.Collections.Generic;
using PhysicsBattler.Authority;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

internal static class GuardReport
{
    public static int Run(int seed)
    {
        var off = new Dictionary<string, List<float>>();
        var turn = new Dictionary<string, List<float>>();
        var last = new Dictionary<(UnitController, PartId), Quaternion>();
        void Add(Dictionary<string, List<float>> d, string k, float v) { if (!d.TryGetValue(k, out var l)) d[k] = l = new List<float>(); l.Add(v); }
        FightReport.OnStep = (units, dt) =>
        {
            foreach (var u in units)
            {
                bool guard = u.IsAlive && u.State == UnitState.Standing && u.CurrentMove == null && u.Combo == null
                             && u.Authority.Level(BodyRegion.ArmR) == AuthorityLevel.Pinned;
                foreach (var id in new[] { PartId.UpperArmR, PartId.ForearmR, PartId.HandR })
                {
                    var q = u.Anim.Part(id).rotation;
                    if (guard)
                    {
                        u.Authority.Error(id, out _, out float angle);
                        Add(off, $"{u.Body.SizeFactor:0.00} {id}", angle);
                        if (last.TryGetValue((u, id), out var before))
                        {
                            Add(turn, $"{u.Body.SizeFactor:0.00} {id}", Quaternion.Angle(before, q));
                        }
                    }

                    if (guard) last[(u, id)] = q; else last.Remove((u, id));
                }
            }
        };
        var rigs = new Dictionary<string, PhysicsBattler.Rig.RigFile>();
        foreach (var body in new[] { "Small", "Medium", "Large" })
        {
            rigs[body] = PhysicsBattler.Rig.RigFile.Parse(System.IO.File.ReadAllText(System.IO.Path.Combine(Application.dataPath, $"_Battler/Data/Rigs/{body.ToLowerInvariant()}_rig.json")));
        }

        try
        {
            FightReport.Play(rigs, seed, 60f, null, out _);
        }
        finally
        {
            FightReport.OnStep = null;
        }
        string P(List<float> l, float p) { l.Sort(); return l[(int)(p * (l.Count - 1))].ToString("0.0"); }
        Console.WriteLine("weapon arm held in the guard: physical off the pose (median / 95th / max), posed turn a step (median / 95th / max), degrees");
        foreach (var k in off.Keys)
        {
            var t = turn.TryGetValue(k, out var tl) ? tl : new List<float> { 0f };
            Console.WriteLine($"  {k,-16} off {P(off[k], 0.5f)} / {P(off[k], 0.95f)} / {P(off[k], 1f)}   turn {P(t, 0.5f)} / {P(t, 0.95f)} / {P(t, 1f)}   ({off[k].Count} steps)");
        }

        return 0;
    }
}
