// `guardpop`: a unit standing in its guard with its rhythm going, facing an enemy; every step where
// the weapon arm's elbow or hand moves further than it moved the step before by more than a
// centimetre is printed with what the arm solve did (the elbow's swivel, the keyed hand's move, the
// torso's push).  GUARD_BODY=Large (default), GUARD_STEPS=400
using System;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

internal static class GuardPopReport
{
    public static int Run()
    {
        string body = Environment.GetEnvironmentVariable("GUARD_BODY") ?? "Large";
        int steps = int.TryParse(Environment.GetEnvironmentVariable("GUARD_STEPS"), out int n) ? n : 400;
        // (As the rhythm test builds its pair.)
        var (a, target) = PhysicsBattler.Tests.RhythmTests.Pair(body);
        Vector3 lastElbow = Vector3.zero, lastHand = Vector3.zero, lastKeyed = Vector3.zero;
        float lastElbowMove = 0f, lastHandMove = 0f, lastSwivel = 0f;
        Vector3 lastGrip = Vector3.zero, lastShoulder = Vector3.zero;
        Quaternion lastTurn = Quaternion.identity;
        for (int i = 0; i < steps; i++)
        {
            a.PreviewPose(1f / 60f);
            target.PreviewPose(1f / 60f);
            Vector3 elbow = a.Anim.Part(PartId.ForearmR).position, hand = a.Anim.Part(PartId.HandR).position;
            var arm = a.Pose.LastWeaponArm;
            float e = Vector3.Distance(elbow, lastElbow), h = Vector3.Distance(hand, lastHand);
            if (i > 2 && (e - lastElbowMove > 0.01f || h - lastHandMove > 0.01f))
            {
                Console.WriteLine($"step {i}: elbow moved {e * 100f:0.0} cm (before {lastElbowMove * 100f:0.0}), hand {h * 100f:0.0} (before {lastHandMove * 100f:0.0}); swivel {lastSwivel:0} -> {arm.swivel:0}, "
                                  + $"keyed hand moved {Vector3.Distance(a.Pose.HandKeyed, lastKeyed) * 100f:0.0} cm, torso push {a.Pose.HandCleared.magnitude * 100f:0.0} cm, guard grip moved {Vector3.Distance(a.Pose.GuardGrip, lastGrip) * 100f:0.0} cm, turned {Quaternion.Angle(a.Pose.GuardTurn, lastTurn):0.0}°, wrist {arm.twist:0}/{arm.flexion:0}/{arm.deviation:0}, bend {arm.elbowBend:0.0}, shoulder moved {Vector3.Distance(a.Anim.Part(PartId.UpperArmR).position, lastShoulder) * 100f:0.0}, swivel exact {arm.swivel:0.00}, posture {a.Rhythm.Current?.Label}");
            }

            lastElbowMove = e;
            lastHandMove = h;
            lastElbow = elbow;
            lastHand = hand;
            lastKeyed = a.Pose.HandKeyed;
            lastSwivel = arm.swivel;
            lastGrip = a.Pose.GuardGrip;
            lastShoulder = a.Anim.Part(PartId.UpperArmR).position;
            lastTurn = a.Pose.GuardTurn;
        }

        return 0;
    }
}
