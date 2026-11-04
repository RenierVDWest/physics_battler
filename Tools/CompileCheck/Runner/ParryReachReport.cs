// `parryreach [body]`: a reach study for the sword parry's plan. The body stands in its guard (the walk's guard
// clip); for each classic one-handed sword parry (the sabre and broadsword manuals' positions: where the hand is,
// which way the blade points, which way its edge faces), the fight's arm rig is asked to hold the sword there, its
// elbow where the manuals put it, and it prints what the arm made of it: how far the grip and the blade's turn
// missed, how much of the turn the wrist couldn't take, the wrist's twist/flexion/deviation, the elbow's bend and
// where round its circle it went, whether the shoulder went past its limits or the arm into the torso, and how far
// the hand travels from the guard. Then the same with the edge left free (the hand may roll about the blade).
using System;
using System.Globalization;
using System.IO;
using PhysicsBattler.Locomotion;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using PhysicsBattler.Combat;
using PhysicsBattler.Units;
using UnityEngine;

internal static class ParryReachReport
{
    // name, hand (x right, y up, z forward, metres at a Medium's size, against the ground under the body),
    // blade way (grip to tip), edge way (the edge's face), the elbow's null (degrees: 0 down, +90 out, 180 up, -90 in).
    private static readonly (string name, Vector3 hand, Vector3 blade, Vector3 edge, float swivel)[] Parries =
    {
        ("quinte / St George (head)", new Vector3(0.25f, 1.85f, 0.30f), new Vector3(-1f, 0.05f, 0.35f), Vector3.up, 80f),
        ("hanging / high prime (head, inside)", new Vector3(0.10f, 1.80f, 0.35f), new Vector3(-0.8f, -0.5f, 0.3f), new Vector3(0f, 0.6f, 0.8f), 120f),
        ("high tierce (right cheek)", new Vector3(0.30f, 1.60f, 0.35f), new Vector3(-0.6f, 0.7f, 0.2f), new Vector3(0.7f, 0.5f, 0.3f), 60f),
        ("tierce (right shoulder, arm)", new Vector3(0.30f, 1.15f, 0.35f), new Vector3(0.2f, 0.9f, 0.35f), Vector3.right, 20f),
        ("quarte (chest, left)", new Vector3(-0.05f, 1.15f, 0.35f), new Vector3(-0.15f, 0.9f, 0.35f), Vector3.left, -15f),
        ("high quarte (left cheek)", new Vector3(-0.15f, 1.60f, 0.35f), new Vector3(0.6f, 0.7f, 0.2f), new Vector3(-0.6f, 0.6f, 0.4f), 10f),
        ("prime (left shoulder)", new Vector3(-0.20f, 1.45f, 0.35f), new Vector3(0.05f, -1f, 0.15f), Vector3.left, 110f),
        ("seconde (right hip, thigh)", new Vector3(0.30f, 1.00f, 0.35f), new Vector3(0.3f, -0.8f, 0.5f), Vector3.right, 40f),
        ("low tierce (right flank)", new Vector3(0.25f, 0.95f, 0.30f), new Vector3(0.3f, 0.9f, 0.2f), Vector3.right, 10f),
        ("low quarte (belly)", new Vector3(0.15f, 0.95f, 0.30f), new Vector3(-0.5f, 0.75f, 0.15f), new Vector3(-0.3f, -0.2f, 0.9f), -10f),
        ("septime (left thigh)", new Vector3(0.00f, 0.95f, 0.35f), new Vector3(-0.2f, -0.6f, 0.75f), Vector3.left, -20f),
        ("octave (right knee)", new Vector3(0.15f, 1.00f, 0.40f), new Vector3(0.3f, -0.75f, 0.55f), Vector3.right, 0f),
    };

    public static int Run(string body)
    {
        body ??= "Medium";
        var ci = CultureInfo.InvariantCulture;
        LocoLibrary.Load();
        var rig = RigFile.Parse(File.ReadAllText(Path.Combine(Application.dataPath, $"_Battler/Data/Rigs/{body.ToLowerInvariant()}_rig.json")));
        var profile = UnitProfile.CreateDefault();
        profile.body = body;
        var root = UnitBuilder.Build(rig, profile, "Parrier", _ => null, 0);
        var unitRig = root.GetComponent<UnitRig>();
        var anim = new AnimRig(root.transform, unitRig);
        float leg = anim.RestLocalPosition(PartId.ShinL).magnitude + anim.RestLocalPosition(PartId.FootL).magnitude;
        float size = leg / 0.845f;
        var loco = new Locomotor(leg);
        loco.SetGuard(LocoLibrary.CombatPrefix(body));
        loco.GuardWanted = true;
        var poser = new LocoBody(anim, loco) { BodyKey = body, TwoHanded = LocoLibrary.TwoHanded(body) };
        poser.SoleFrom(unitRig);
        anim.SlotWeaponR.localRotation = UnitController.MainGrip;
        poser.BladeR = UnitController.MainGrip * Vector3.forward;
        poser.CarryR = LocoBody.Carry.Weapon;
        loco.Place(Vector3.zero, 0f);
        const float dt = 1f / 60f;
        for (int i = 0; i < 90; i++)
        {
            loco.Step(dt, Vector3.zero, 0f);
            poser.Pose(dt);
        }

        var hand = anim.Part(PartId.HandR);
        var slot = anim.SlotWeaponR;
        Vector3 gripInHand = hand.InverseTransformPoint(slot.position);
        Quaternion slotInHand = Quaternion.Inverse(hand.rotation) * slot.rotation;
        Vector3 bladeInHand = slotInHand * Vector3.forward;
        string edgeAxis = Environment.GetEnvironmentVariable("PARRY_EDGE") ?? "up";
        Vector3 edgeLocal = edgeAxis == "right" ? Vector3.right : edgeAxis == "left" ? Vector3.left : edgeAxis == "down" ? Vector3.down : Vector3.up;
        Vector3 edgeInHand = slotInHand * edgeLocal;
        float k = float.Parse(Environment.GetEnvironmentVariable("PARRY_SCALE") ?? "0.9", CultureInfo.InvariantCulture);
        Vector3 guardGrip = slot.position;
        var torso = new BodyClearance.Posed(unitRig, anim, size);
        Console.WriteLine(string.Format(ci, "{0} in its guard: the grip at {1:0.00} m up, {2:0.00} m right, {3:0.00} m ahead; the head at {4:0.00} m, the right shoulder at {5:0.00} m",
            body, guardGrip.y, guardGrip.x, guardGrip.z, anim.Part(PartId.Head).position.y, anim.Part(PartId.UpperArmR).position.y));
        float reach = ArmRig.MaxReach(anim, true, Quaternion.identity, Vector3.zero);
        Console.WriteLine(string.Format(ci, "the arm's reach (shoulder to wrist): {0:0.00} m", reach));
        Console.WriteLine();
        Console.WriteLine("Each parry held with whichever edge the arm takes best (a sword's edges face +Y and -Y of its slot):");
        foreach (var p in Parries)
        {
            Vector3 at = p.hand * (k * size);
            Vector3 blade = p.blade.normalized;
            Vector3 edge = Vector3.ProjectOnPlane(p.edge, blade).normalized;
            ArmSolve best = default;
            float bestShort = float.MaxValue;
            string bestEdge = "";
            Vector3 bestElbow = Vector3.zero, gotBlade = Vector3.zero, gotEdgeWay = Vector3.zero;
            foreach (var e in new[] { Vector3.up, Vector3.down })
            {
                Quaternion turn = Quaternion.LookRotation(blade, edge) * Quaternion.Inverse(Quaternion.LookRotation(bladeInHand, slotInHand * e));
                var goal = new ArmGoal { point = at, pointInHand = gripInHand, handTurn = turn, elbowSwivel = p.swivel, turnWeight = 1f, clearance = 0.04f * size };
                var sv = ArmRig.Solve(anim, true, goal, torso, apply: true);
                if (sv.wristExcess < bestShort)
                {
                    bestShort = sv.wristExcess;
                    best = sv;
                    bestEdge = e.y > 0f ? "+Y" : "-Y";
                    bestElbow = sv.elbow - sv.shoulder;
                    gotBlade = slot.rotation * Vector3.forward;
                    gotEdgeWay = slot.rotation * e;
                }
            }

            var s = best;
            Console.WriteLine(string.Format(ci,
                "  {0,-36} edge {1}: grip miss {2,4:0.00} m, blade off {3,3:0}, edge off {4,3:0}, wrist short {5,3:0} deg (twist {6,4:0} flex {7,4:0} dev {8,4:0}); elbow bend {9,3:0}, the elbow {10:+0.00;-0.00} right {11:+0.00;-0.00} up {12:+0.00;-0.00} ahead of the shoulder; shoulder past {13,3:0}, into the torso {14:0.00} m; the hand moves {15:0.00} m",
                p.name, bestEdge, s.pointMiss, Vector3.Angle(gotBlade, blade), Vector3.Angle(gotEdgeWay, edge), s.wristExcess, s.twist, s.flexion, s.deviation,
                s.elbowBend, bestElbow.x, bestElbow.y, bestElbow.z, s.shoulderPast, s.clearanceShort, Vector3.Distance(at, guardGrip)));
        }

        UnityEngine.Object.DestroyImmediate(root);
        return 0;
    }
}
