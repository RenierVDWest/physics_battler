// `combopop`: plays a combo as ComboTests does and prints, around every link, how far the weapon hand
// moves each step and what the arm's blend is doing.  COMBO_BODY=Large COMBO_NAME="hook, backhand, smash"
using System;
using PhysicsBattler.Rig;
using PhysicsBattler.Tests;
using UnityEngine;

internal static class ComboPopReport
{
    public static int Run()
    {
        string body = Environment.GetEnvironmentVariable("COMBO_BODY") ?? "Large";
        string name = Environment.GetEnvironmentVariable("COMBO_NAME") ?? "hook, backhand, smash";
        Vector3 last = Vector3.zero, lastShoulder = Vector3.zero, lastRel = Vector3.zero;
        float lastStep = 0f;
        ComboTests.Play(body, name, out var unit, out var enemy, (u, e, i) =>
        {
            Vector3 hand = u.Anim.Part(PartId.HandR).position;
            Vector3 shoulder = u.Anim.Part(PartId.UpperArmR).position;
            float step = Vector3.Distance(hand, last);
            var p = u.Pose;
            if (i > 1 && (step - 2f * lastStep > 0.03f || p.LinkWeight > 0f && p.LinkWeight < 1f && i % 3 == 0))
            {
                Console.WriteLine($"t {u.LocalTime:0.000}: hand step {step * 100f:0.0} (before {lastStep * 100f:0.0}), move {u.CurrentMove?.Move.name} T {u.CurrentMove?.T:0.000} blend {u.CurrentMove?.Blend:0.00}, link from {p.LinkFrom?.Move.name} weight {p.LinkWeight:0.00}, keyed {p.HandKeyed}, pushed {p.HandCleared.magnitude * 100f:0.0}, shoulder step {Vector3.Distance(shoulder, lastShoulder) * 100f:0.0}, hand from shoulder changed {Vector3.Distance(hand - shoulder, lastRel) * 100f:0.0}, offset {p.LinkHandOffset.magnitude * 100f:0.0}, keyed from shoulder vs last {Vector3.Distance(p.HandKeyed - shoulder, lastRel) * 100f:0.0}, held {p.LinkArmHeld}");
            }

            lastStep = step;
            last = hand;
            lastShoulder = shoulder;
            lastRel = hand - shoulder;
        });
        return 0;
    }
}
