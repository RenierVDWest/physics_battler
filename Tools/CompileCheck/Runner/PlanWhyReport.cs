// `planwhy`: why a prepared blow is refused. Every body against a Medium, every move and aim part:
// the dry run's aim error at contact (and the tolerance), and the attacker's own-body clearance
// (the part and the moment it is closest, and whether the weapon or the arm).
using System;
using PhysicsBattler.Combat;
using PhysicsBattler.Units;
using UnityEngine;

internal static class PlanWhyReport
{
    public static int Run()
    {
        var sword = SwingReport.Sword();
        var club = SwingReport.Club();
        string only = Environment.GetEnvironmentVariable("PLAN_MOVE");
        string bodyOnly = Environment.GetEnvironmentVariable("PLAN_BODY");
        foreach (var (body, weapon) in new[] { ("Small", club), ("Small", sword), ("Medium", sword), ("Medium", club), ("Large", sword), ("Large", club) })
        {
            if (bodyOnly != null && body != bodyOnly)
            {
                continue;
            }

            foreach (var move in weapon.moves)
            {
                if (only != null && !move.name.Contains(only))
                {
                    continue;
                }

                foreach (AimPart part in new[] { AimPart.Head, AimPart.Neck, AimPart.Chest, AimPart.Belly, AimPart.KneeL, AimPart.ElbowR })
                {
                    if (!move.targets.Contains(part))
                    {
                        continue;
                    }

                    var a = AimMatrix.Build(body, Vector3.zero, 0f, 0);
                    if (!move.UsableBy(a.Body.SizeFactor))
                    {
                        UnityEngine.Object.DestroyImmediate(a.gameObject);
                        break;
                    }

                    var target = AimMatrix.Build(Environment.GetEnvironmentVariable("PLAN_TARGET") ?? "Medium", Vector3.forward * 5f, 180f, 1);
                    float distance = a.Style.engageDistance * a.Body.SizeFactor + 0.25f * target.Body.SizeFactor;
                    target.Place(Vector3.forward * distance, 180f);
                    a.Equip(weapon, null);
                    for (int i = 0; i < 20; i++)
                    {
                        a.PreviewPose(1f / 60f);
                        target.PreviewPose(1f / 60f);
                    }

                    var s = MovePlanner.Prepare(a, target, move, part);
                    if (s == null)
                    {
                        Console.WriteLine($"{body} {weapon.kind} {move.name} at {part}: not prepared");
                    }
                    else
                    {
                        float err = a.TryOut(s);
                        float tol = (s.Generated ? 0.66f : 1f) * MovePlanner.Tolerance;
                        float c = a.Clearance(s, MovePlanner.ClearanceSamples, out var closest, out float when, out bool blade);
                        float need = -MovePlanner.OwnBodyClearance * a.Body.SizeFactor;
                        Console.WriteLine($"{body} {weapon.kind} {move.name} at {part}: aim {err * 100f:0.0} cm (tol {tol * 100f:0.0}){(err > tol ? " MISS" : "")}, "
                                          + $"clearance {c * 100f:0.0} cm ({(blade ? "weapon" : "arm")} on the {closest} at {when:0.00}){(c < need ? " SHORT" : "")}, contact hand miss {s.ContactHandMiss * 100f:0.0} cm");
                    }

                    if (s != null && Environment.GetEnvironmentVariable("PLAN_PLAY") != null && MovePlanner.Acceptable(a, s))
                    {
                        float error = -1f;
                        void OnContact(UnitController unit, MoveState m, ContactReport report)
                        {
                            if (unit == a) error = report.AnimError;
                        }

                        UnitController.MoveContact += OnContact;
                        a.DealsHits = false;
                        a.StartMove(s);
                        var trace = new System.Text.StringBuilder();
                        var times = new System.Collections.Generic.List<float>();
                        for (int i = 0; i < 400 && a.IsAttacking; i++)
                        {
                            a.PreviewPose(1f / 60f);
                            target.PreviewPose(1f / 60f);
                            if (Mathf.Abs(s.T - s.ContactTime) < 0.03f)
                            {
                                trace.Append($" [T {s.T:0.0000} p {s.Progress:0.0000}: {Vector3.Distance(a.Pose.StrikePoint, s.Aim) * 100f:0.0}, hand {a.Anim.Part(PhysicsBattler.Rig.PartId.HandR).position}]");
                                times.Add(s.T);
                            }
                        }

                        UnitController.MoveContact -= OnContact;
                        Console.WriteLine($"   played: swept error {error * 100f:0.0} cm, contact T {s.ContactTime:0.0000};{trace}");
                        var dry = new System.Text.StringBuilder();
                        foreach (float t in times)
                        {
                            a.PoseDryAt(s, t);
                            dry.Append($" [T {t:0.0000} p {s.Progress:0.0000}: {Vector3.Distance(a.Pose.StrikePoint, s.Aim) * 100f:0.0}, hand {a.Anim.Part(PhysicsBattler.Rig.PartId.HandR).position}]");
                        }

                        Console.WriteLine($"   dry at those times:{dry}");
                    }

                    if (s != null && Environment.GetEnvironmentVariable("PLAN_AT") != null)
                    {
                        foreach (var ts in Environment.GetEnvironmentVariable("PLAN_AT").Split(','))
                        {
                            float t = float.Parse(ts, System.Globalization.CultureInfo.InvariantCulture);
                            a.PoseDryAt(s, t);
                            var an = a.Anim;
                            Vector3 o = an.Part(PhysicsBattler.Rig.PartId.Chest).position;
                            Vector3 R(PhysicsBattler.Rig.PartId id) => Quaternion.Euler(0f, -a.FacingYaw, 0f) * (an.Part(id).position - o);
                            var slot = an.SlotWeaponR;
                            Vector3 tip = slot.position + slot.rotation * (Vector3.forward * weapon.length * 0.98f * a.Loadout.Scale);
                            Vector3 F(Vector3 v) => Quaternion.Euler(0f, -a.FacingYaw, 0f) * (v - o);
                            var arm = a.Pose.LastWeaponArm;
                            Console.WriteLine($"   t {t:0.00} (from the chest, facing): shoulder {R(PhysicsBattler.Rig.PartId.UpperArmR)}, elbow {R(PhysicsBattler.Rig.PartId.ForearmR)}, wrist {R(PhysicsBattler.Rig.PartId.HandR)}, grip {F(slot.position)}, tip {F(tip)}, swivel {arm.swivel:0}, bend {arm.elbowBend:0}, wrist {arm.twist:0}/{arm.flexion:0}/{arm.deviation:0}, spine {Quaternion.Angle(Quaternion.identity, an.Part(PhysicsBattler.Rig.PartId.Spine).localRotation):0}");
                        }
                    }

                    UnityEngine.Object.DestroyImmediate(a.gameObject);
                    UnityEngine.Object.DestroyImmediate(target.gameObject);
                }
            }
        }

        return 0;
    }
}
