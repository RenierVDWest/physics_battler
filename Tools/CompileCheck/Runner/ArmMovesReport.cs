// `armmoves`: the arm rebuild's step 3 check. Every body's guard and every move it throws, played at
// a medium target (each part the move aims at), the weapon arm measured each step from its rig's
// solve: how often and how far the wrist couldn't take the turn (the hand then turns otherwise than
// asked), the shoulder past its reach, the arm closer to the torso than its clearance, how far the
// elbow is from its null, the weapon's roll in the hand, the striking part off the aim point at
// contact, and the fastest turn of an arm part in a step.
using System;
using System.Collections.Generic;
using PhysicsBattler.Combat;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

internal static class ArmMovesReport
{
    private sealed class Tally
    {
        public int steps, wristOut;
        public float wristWorst, shoulderWorst, clearWorst, rollWorst, turnWorst, contactMiss = -1f;
        public readonly List<float> swivel = new List<float>();
        public readonly List<string> asked = new List<string>();
        public string turnWhere = "";

        public void Add(ArmSolve s)
        {
            steps++;
            if (s.wristExcess > 0.5f) wristOut++;
            wristWorst = Mathf.Max(wristWorst, s.wristExcess);
            shoulderWorst = Mathf.Max(shoulderWorst, s.shoulderPast);
            clearWorst = Mathf.Max(clearWorst, s.clearanceShort);
            rollWorst = Mathf.Max(rollWorst, Mathf.Abs(s.roll));
            swivel.Add(s.swivelOff);
            if (System.Environment.GetEnvironmentVariable("ARM_ASKED") != null && steps % 6 == 0)
            {
                asked.Add($"twist {s.askedTwist:0} flex {s.askedFlexion:0} dev {s.askedDeviation:0} (got {s.twist:0}/{s.flexion:0}/{s.deviation:0}, roll {s.roll:0})");
            }
        }

        public string Line()
        {
            swivel.Sort();
            string sw = swivel.Count > 0 ? $"{swivel[swivel.Count / 2]:0}/{swivel[(int)(0.95f * (swivel.Count - 1))]:0}" : "-";
            return $"wrist past {100f * wristOut / Math.Max(1, steps):0}% (worst {wristWorst:0}), shoulder past {shoulderWorst:0}, torso short {clearWorst * 100f:0.0} cm, elbow off null {sw}, roll {rollWorst:0}, "
                   + $"fastest part {turnWorst:0} deg/step{turnWhere}" + (contactMiss >= 0f ? $", at contact {contactMiss * 100f:0.0} cm off" : "");
        }
    }

    public static int Run()
    {
        var sword = SwingReport.Sword();
        var club = SwingReport.Club();
        var shield = WeaponDefinition.CreateDefault(WeaponKind.Shield);
        float dt = Time.fixedDeltaTime;
        foreach (var (body, weapon, off) in new[] { ("Small", club, (WeaponDefinition)null), ("Medium", sword, shield), ("Large", club, null) })
        {
            var guard = new Tally();
            foreach (var move in weapon.moves)
            {
                var attacker = AimMatrix.Build(body, Vector3.zero, 0f, 0);
                if (!move.UsableBy(attacker.Body.SizeFactor))
                {
                    UnityEngine.Object.DestroyImmediate(attacker.gameObject);
                    continue;
                }

                var tally = new Tally();
                foreach (var part in new[] { AimPart.Head, AimPart.Chest, AimPart.Belly, AimPart.KneeL, AimPart.ElbowR })
                {
                    if (!move.targets.Contains(part))
                    {
                        continue;
                    }

                    var a = AimMatrix.Build(body, Vector3.zero, 0f, 0);
                    a.DealsHits = false;
                    var target = AimMatrix.Build("Medium", Vector3.forward * 5f, 180f, 1);
                    float distance = a.Style.engageDistance * a.Body.SizeFactor + 0.25f * target.Body.SizeFactor;
                    target.Place(Vector3.forward * distance, 180f);
                    a.Equip(weapon, off);
                    var last = new Quaternion[3];
                    for (int i = 0; i < 30; i++)
                    {
                        a.PreviewPose(dt);
                        target.PreviewPose(dt);
                        if (i >= 10)
                        {
                            guard.Add(a.Pose.LastWeaponArm);
                        }
                    }

                    var state = MovePlanner.Prepare(a, target, move, part);
                    // (Stepped in until the move reaches, as a fight's approach would.)
                    for (float k = 1f; state == null && k > 0.3f; k -= 0.1f)
                    {
                        target.Place(Vector3.forward * distance * k, 180f);
                        target.PreviewPose(dt);
                        state = MovePlanner.Prepare(a, target, move, part);
                    }

                    // (The guard's last arm, so the hand-over into the blow is measured too.)
                    last[0] = a.Anim.Part(PartId.UpperArmR).rotation;
                    last[1] = a.Anim.Part(PartId.ForearmR).rotation;
                    last[2] = a.Anim.Part(PartId.HandR).rotation;
                    if (state != null && a.StartMove(state))
                    {
                        bool known = true;
                        for (int i = 0; i < 240 && a.IsAttacking; i++)
                        {
                            a.PreviewPose(dt);
                            target.PreviewPose(dt);
                            tally.Add(a.Pose.LastWeaponArm);
                            string dump = System.Environment.GetEnvironmentVariable("ARM_DUMP");
                            if (dump != null && $"{body} {move.name.Replace("MD_", "")} {part}" == dump)
                            {
                                var sv = a.Pose.LastWeaponArm;
                                Console.WriteLine($"   T {state.T:0.000} turns {Quaternion.Angle(a.Anim.Part(PartId.UpperArmR).rotation, last[0]):0}/{Quaternion.Angle(a.Anim.Part(PartId.ForearmR).rotation, last[1]):0}/{Quaternion.Angle(a.Anim.Part(PartId.HandR).rotation, last[2]):0} prog {state.Progress:0.000} blend {state.Blend:0.00} miss {sv.pointMiss * 100f:0.0} cm bend {sv.elbowBend:0} swivelOff {sv.swivelOff:0} shoulder {sv.shoulderPast:0} wrist {sv.twist:0}/{sv.flexion:0}/{sv.deviation:0} aimErr {a.Pose.AimError * 100f:0.0} spine {a.Pose.SpineBend:0} reach {Vector3.Distance(sv.shoulder, sv.wrist):0.00}");
                            }
                            if (state.ContactPassed && tally.contactMiss < 0f)
                            {
                                tally.contactMiss = a.Pose.AimError;
                            }

                            int k = 0;
                            foreach (var id in new[] { PartId.UpperArmR, PartId.ForearmR, PartId.HandR })
                            {
                                var q = a.Anim.Part(id).rotation;
                                if (known)
                                {
                                    float turn = Quaternion.Angle(q, last[k]);
                                    if (turn > tally.turnWorst)
                                    {
                                        tally.turnWorst = turn;
                                        tally.turnWhere = $" ({id} at T {state.T:0.00})";
                                    }
                                }

                                last[k++] = q;
                            }

                            known = true;
                        }
                    }

                    UnityEngine.Object.DestroyImmediate(a.gameObject);
                    UnityEngine.Object.DestroyImmediate(target.gameObject);
                }

                UnityEngine.Object.DestroyImmediate(attacker.gameObject);
                Console.WriteLine($"{body} {move.name.Replace("MD_", ""),-16} {tally.Line()}");
                foreach (var line in tally.asked) Console.WriteLine("      " + line);
            }

            Console.WriteLine($"{body} {"guard",-16} {guard.Line()}");
        }

        return 0;
    }
}
