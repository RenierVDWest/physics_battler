// `feint`: a body's feint posture played as RhythmTests plays it, the weapon hand's step and what moved it.
//   FEINT_BODY=Large FEINT_NAME=ClubFeint
using System;
using PhysicsBattler.Rig;
using PhysicsBattler.Tests;
using UnityEngine;

internal static class FeintReport
{
    public static int Run()
    {
        string body = Environment.GetEnvironmentVariable("FEINT_BODY") ?? "Large";
        string name = Environment.GetEnvironmentVariable("FEINT_NAME") ?? "ClubFeint";
        var (unit, opponent) = RhythmTests.Pair(body);
        var both = new[] { unit, opponent };
        RhythmTests.Step(both, 30);
        foreach (var posture in unit.Rhythm.Definition.postures)
        {
            if (posture.Label != name) continue;
            unit.Rhythm.Force(posture);
            unit.Rhythm.EnergyOverride = 1f;
            Vector3 last = unit.Anim.Part(PartId.HandR).position;
            for (int i = 0; i < 60 && unit.Rhythm.Current == posture; i++)
            {
                RhythmTests.Step(both);
                Vector3 hand = unit.Anim.Part(PartId.HandR).position;
                var p = unit.Pose;
                Console.WriteLine($"frame {i}: hand step {Vector3.Distance(hand, last) * 100f:0.0} cm, pushed {p.HandCleared.magnitude * 100f:0.0}, keyed {p.HandKeyed}, move {p.Move?.Move.name} T {p.Move?.T:0.000} blend {p.Move?.Blend:0.00}, swivel {p.LastWeaponArm.swivel:0}");
                last = hand;
            }
        }

        return 0;
    }
}
