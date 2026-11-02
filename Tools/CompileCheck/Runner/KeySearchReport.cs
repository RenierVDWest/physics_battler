// `keysearch`: default key finding for the arm rebuild's blows. A medium unit with each swing at a
// medium's chest, posed dry at its wind-up's end and its follow-through's end; at each, every hand
// place (round the shoulder, in the aim frame) and wrist key (inside the wrist's limits) solved, and
// the one whose striking part is furthest round the pivot from where it strikes (on the side the
// swing comes from, or goes to) with the arm and blade clear of the torso reported, beside the
// keys the move has now.
using System;
using PhysicsBattler.Combat;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

internal static class KeySearchReport
{
    public static int Run()
    {
        string only = Environment.GetEnvironmentVariable("KEY_MOVE");
        foreach (var weapon in new[] { SwingReport.Sword(), SwingReport.Club() })
        {
            foreach (var move in weapon.moves)
            {
                if (move.shape != MoveShape.Swing || only != null && !move.name.Contains(only))
                {
                    continue;
                }

                var a = AimMatrix.Build("Medium", Vector3.zero, 0f, 0);
                var target = AimMatrix.Build("Medium", Vector3.forward * 5f, 180f, 1);
                float distance = a.Style.engageDistance * a.Body.SizeFactor + 0.25f * target.Body.SizeFactor;
                target.Place(Vector3.forward * distance, 180f);
                a.Equip(weapon, null);
                for (int i = 0; i < 20; i++)
                {
                    a.PreviewPose(1f / 60f);
                    target.PreviewPose(1f / 60f);
                }

                MoveState s = null;
                foreach (var part in new[] { AimPart.Chest, AimPart.Belly, AimPart.Neck, AimPart.Head, AimPart.KneeL })
                {
                    if (s == null && move.targets.Contains(part))
                    {
                        s = MovePlanner.Prepare(a, target, move, part);
                    }
                }

                if (s == null)
                {
                    Console.WriteLine($"{move.name}: not prepared");
                    continue;
                }

                var anim = a.Anim;
                Vector3 Strike()
                {
                    var slot = anim.SlotWeaponR;
                    return slot.position + slot.rotation * (weapon.StrikePoint(move.strikeWith) * a.Loadout.Scale);
                }

                float tc = s.ContactTime;
                a.PoseDryAt(s, tc - 0.01f);
                Vector3 before = Strike();
                a.PoseDryAt(s, tc + 0.01f);
                Vector3 after = Strike();
                a.PoseDryAt(s, tc);
                Vector3 contactRadial = Strike() - s.PivotAt(move.ContactProgress);
                Vector3 travel = (after - before).normalized;
                Console.WriteLine($"{move.name}: contact radial {contactRadial}, travel {travel}");

                foreach (var (key, t, sign) in new[] { ("wind-up", move.WindUpEnd, -1f), ("follow", move.SwingFollowEnd, 1f) })
                {
                    a.PoseDryAt(s, t);
                    Vector3 pivot = s.PivotAt(move.ProgressAt(t));
                    Vector3 shoulder = anim.Part(PartId.UpperArmR).position;
                    var torso = new BodyClearance.Posed(a.Rig, anim, a.Loadout.Scale);
                    float swivel = move.ElbowSwivel(t, 20f);
                    float reach = anim.RestLocalPosition(PartId.ForearmR).magnitude + anim.RestLocalPosition(PartId.HandR).magnitude;
                    float length = weapon.length * 0.98f * a.Loadout.Scale;
                    float best = float.MinValue;
                    string bestText = "";
                    float Score(Vector3 hand, Vector3 wrist, out string text)
                    {
                        var goal = new ArmGoal { point = hand, wristGiven = true, wristLocal = ArmRig.Wrist(anim, true, wrist), elbowSwivel = swivel };
                        var solve = ArmRig.Solve(anim, true, goal, torso);
                        var slot = anim.SlotWeaponR;
                        Vector3 tip = slot.position + slot.rotation * (Vector3.forward * length);
                        float clear = Mathf.Min(torso.Arm(shoulder, anim.Part(PartId.ForearmR).position, anim.Part(PartId.HandR).position, slot.position), torso.Blade(slot.position, tip));
                        // (Round the pivot in the plane the move's numbers lay its path in, from where
                        // it strikes: back the way it comes for the wind-up, on the way it goes for the
                        // follow-through, past straight behind counting on; near the plane; the wrist
                        // well inside its limits.)
                        Vector3 radial = Strike() - pivot;
                        float cp = move.ContactProgress;
                        Vector3 e1 = (s.Aim - s.PivotAt(cp)).normalized;
                        Vector3 e2 = Vector3.ProjectOnPlane(s.PathPoint(Mathf.Min(1f, cp + 0.01f)) - s.PathPoint(Mathf.Max(0f, cp - 0.01f)), e1).normalized;
                        Vector3 n = Vector3.Cross(e1, e2);
                        float angle = Mathf.Atan2(Vector3.Dot(radial, e2), Vector3.Dot(radial, e1)) * Mathf.Rad2Deg * sign;
                        if (angle < -150f)
                        {
                            angle += 360f;
                        }

                        float off = Mathf.Abs(Mathf.Asin(Mathf.Clamp(Vector3.Dot(radial.normalized, n), -1f, 1f)) * Mathf.Rad2Deg);
                        float point = Vector3.Angle(slot.rotation * Vector3.forward, radial);
                        float comfort = Mathf.Abs(wrist.x) / 80f + Mathf.Abs(wrist.y) / 60f + Mathf.Abs(wrist.z) / 30f;
                        float score = angle - 0.5f * off - 0.2f * point - 25f * comfort - (clear < 0.03f ? 1000f : 0f)
                                      - 100f * solve.wristExcess - 100f * solve.shoulderPast;
                        text = $"{angle:0}° round, {off:0}° off the plane, blade {point:0}° off the radial, clear {clear * 100f:0.0} cm, hand {Quaternion.Inverse(s.Frame) * (hand - shoulder) / s.Scale}, wrist {wrist}";
                        return score;
                    }

                    Vector3 nowHand = shoulder + s.Frame * ((key == "wind-up" ? move.handWindUp : move.handFollow) * s.Scale);
                    Vector3 nowWrist = key == "wind-up" ? move.wristWindUp : move.wristFollow;
                    Score(nowHand, nowWrist, out string nowText);
                    // (Near the hand's authored place: the place is the author's, the wrist the search's.)
                    Vector3 authored = key == "wind-up" ? move.handWindUp : move.handFollow;
                    for (float x = -0.3f; x <= 0.31f; x += 0.1f)
                    for (float y = -0.3f; y <= 0.31f; y += 0.1f)
                    for (float z = -0.3f; z <= 0.31f; z += 0.1f)
                    {
                        var local = authored + new Vector3(x, y, z);
                        if (local.magnitude > 0.95f * reach || local.magnitude < 0.2f)
                        {
                            continue;
                        }

                        Vector3 hand = shoulder + s.Frame * local;
                        for (float tw = -60f; tw <= 60f; tw += 15f)
                        for (float fl = -45f; fl <= 45f; fl += 15f)
                        for (float dv = -25f; dv <= 15f; dv += 10f)
                        {
                            float sc = Score(hand, new Vector3(tw, fl, dv), out string text);
                            if (sc > best)
                            {
                                best = sc;
                                bestText = text;
                            }
                        }
                    }

                    Console.WriteLine($"   {key}: now {nowText}");
                    Console.WriteLine($"   {key}: best {bestText}");
                }

                UnityEngine.Object.DestroyImmediate(a.gameObject);
                UnityEngine.Object.DestroyImmediate(target.gameObject);
            }
        }

        return 0;
    }
}
