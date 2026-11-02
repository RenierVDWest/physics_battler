using System;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using PhysicsBattler.Tests;
using UnityEngine;
internal static class ArmRigTime
{
    public static int Run()
    {
        var unit = RhythmTests.Build("Medium", Vector3.zero, 0f, 0);
        var anim = unit.Anim;
        var torso = new PhysicsBattler.Combat.BodyClearance.Posed(unit.Rig, anim, 1f);
        var r = new System.Random(1);
        foreach (bool feasible in new[] { true, false })
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            int n = 2000;
            for (int i = 0; i < n; i++)
            {
                ArmGoal g;
                if (feasible)
                {
                    // (A natural arm: hand in front of the chest, a straight-ish wrist, the elbow down.)
                    anim.Part(PartId.UpperArmR).localRotation = Quaternion.Euler(-30f - 50f * (float)r.NextDouble(), 20f * (float)r.NextDouble() - 10f, 10f * (float)r.NextDouble());
                    anim.Part(PartId.ForearmR).localRotation = Quaternion.AngleAxis(40f + 60f * (float)r.NextDouble(), ArmRig.ElbowAxis(anim, true));
                    anim.Part(PartId.HandR).localRotation = Quaternion.Euler(10f * (float)r.NextDouble(), 30f * (float)r.NextDouble(), 10f * (float)r.NextDouble());
                    g = ArmGoal.Hand(anim.Part(PartId.HandR).position, Vector3.zero, anim.Part(PartId.HandR).rotation, 20f);
                }
                else
                {
                    Vector3 p = anim.Part(PartId.UpperArmR).position + new Vector3((float)r.NextDouble() - 0.3f, (float)r.NextDouble() - 0.6f, (float)r.NextDouble()) * 0.6f;
                    g = ArmGoal.Hand(p, Vector3.zero, Quaternion.Euler(r.Next(360), r.Next(360), r.Next(360)), 20f);
                }

                g.rollAxisInHand = Vector3.forward; g.rollAllowance = 30f; g.clearance = 0.03f;
                ArmRig.Solve(anim, true, g, torso);
            }

            Console.WriteLine($"{(feasible ? "natural goals" : "random impossible turns")}: {sw.Elapsed.TotalMilliseconds / n * 1000:0} microseconds a solve (roll and clearance on)");
        }

        return 0;
    }
}
