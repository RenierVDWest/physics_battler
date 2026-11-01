// `poselimits`: milestone 9b. Each body, squared up to an opponent with its Fight Rhythm and tempo
// running, for 10 s: how often each joint of the pose is more than 10 degrees past its limits, and
// how its turn splits (the bend about X, the swing about Y and Z), median and 5-95%.
using System;
using System.Collections.Generic;
using PhysicsBattler.Muscles;
using PhysicsBattler.Rig;
using PhysicsBattler.Tests;
using PhysicsBattler.Units;
using UnityEngine;

internal static class PoseLimitsReport
{
    public static int Run()
    {
        foreach (string body in new[] { "Small", "Medium", "Large" })
        {
            UnitController.ResetSeeds(0);
            var (unit, opponent) = RhythmTests.Pair(body);
            unit.TempoOn = true;
            unit.TempoEngaged = true;
            var both = new[] { unit, opponent };
            var past = new Dictionary<PartId, int>();
            var angles = new Dictionary<PartId, List<Vector3>>();
            int n = 0;
            try
            {
                RhythmTests.Step(both, 30);
                for (int i = 0; i < 600; i++)
                {
                    RhythmTests.Step(both);
                    n++;
                    foreach (var id in PartInfo.All)
                    {
                        if (!PartInfo.HasParent(id)) continue;
                        var part = unit.Rig.Part(id);
                        Quaternion local = Quaternion.Inverse(part.restLocalRotation) * (Quaternion.Inverse(unit.Anim.Part(PartInfo.Parent(id)).rotation) * unit.Anim.Part(id).rotation);
                        float p = JointMath.PastLimits(local, JointLimitTable.For(id), out float x, out float y, out float z);
                        if (p > 10f) past[id] = (past.TryGetValue(id, out int c) ? c : 0) + 1;
                        if (!angles.TryGetValue(id, out var list)) angles[id] = list = new List<Vector3>();
                        list.Add(new Vector3(x, y, z));
                    }
                }
            }
            finally
            {
                RhythmTests.Destroy(both);
            }

            Console.WriteLine($"{body}:");
            foreach (var id in PartInfo.All)
            {
                if (!angles.TryGetValue(id, out var list)) continue;
                string Q(Func<Vector3, float> f)
                {
                    var v = list.ConvertAll(a => f(a));
                    v.Sort();
                    return $"{v[(int)(0.05f * (v.Count - 1))]:0}/{v[v.Count / 2]:0}/{v[(int)(0.95f * (v.Count - 1))]:0}";
                }
                int k = past.TryGetValue(id, out int c2) ? c2 : 0;
                var spec = JointLimitTable.For(id);
                Console.WriteLine($"  {id,-10} past>10° {100f * k / n,3:0}%   X {Q(a => a.x),-14} Y {Q(a => a.y),-14} Z {Q(a => a.z),-14}  limits X {spec.pitchMin:0}..{spec.pitchMax:0} Y ±{spec.twist:0} Z ±{spec.swing:0}");
            }
        }

        return 0;
    }
}
