// `aimseq`: MoveAimTests' every-planned-blow-lands, one attacker body against each target body, each
// blow printed: the dry run's miss, the played pass, and at the striking step how far the contact's
// correction and the torso's push moved the hand.  AIM_BODY=Medium
using System;
using PhysicsBattler.Combat;
using PhysicsBattler.Tests;
using PhysicsBattler.Units;
using UnityEngine;

internal static class AimSequenceReport
{
    public static int Run()
    {
        string attackerBody = Environment.GetEnvironmentVariable("AIM_BODY") ?? "Medium";
        foreach (var targetBody in MoveAimTests.Bodies)
        {
            foreach (var weapon in new[] { MoveAimTests.Sword(), MoveAimTests.Club() })
            {
                var attacker = MoveAimTests.Build(attackerBody, Vector3.zero, 0f, 0);
                var target = MoveAimTests.Build(targetBody, Vector3.forward * 5f, 180f, 1);
                float distance = attacker.Style.engageDistance * attacker.Body.SizeFactor + 0.25f * target.Body.SizeFactor;
                target.Place(Vector3.forward * distance, 180f);
                attacker.Equip(weapon, null);
                MoveAimTests.Settle(attacker, target, 20);
                foreach (var move in weapon.moves)
                {
                    foreach (var part in AimPartInfo.All)
                    {
                        if (!move.targets.Contains(part))
                        {
                            continue;
                        }

                        var state = MovePlanner.Prepare(attacker, target, move, part);
                        if (state == null || !MovePlanner.Acceptable(attacker, state))
                        {
                            continue;
                        }

                        float dry = attacker.TryOut(state);
                        float armLength = attacker.Anim.RestLocalPosition(PhysicsBattler.Rig.PartId.ForearmR).magnitude + attacker.Anim.RestLocalPosition(PhysicsBattler.Rig.PartId.HandR).magnitude;
                        float reachShare = Vector3.Distance(state.Aim + state.Frame * state.ContactHand, state.ContactShoulder) / armLength;
                        float error = -1f;
                        void OnContact(UnitController unit, MoveState s, ContactReport report)
                        {
                            if (unit == attacker) error = report.AnimError;
                        }

                        UnitController.MoveContact += OnContact;
                        attacker.StartMove(state);
                        string atContact = "";
                        bool seen = false;
                        for (int i = 0; i < 400 && attacker.IsAttacking; i++)
                        {
                            attacker.PreviewPose(1f / 60f);
                            target.PreviewPose(1f / 60f);
                            if (!seen && state.T >= state.ContactTime)
                            {
                                seen = true;
                                atContact = $"pushed {attacker.Pose.HandCleared.magnitude * 100f:0.0} cm, corrected {Vector3.Distance(attacker.Pose.HandKeyed, attacker.Anim.Part(PhysicsBattler.Rig.PartId.HandR).position) * 100f:0.0} cm, strike {Vector3.Distance(attacker.Pose.StrikePoint, state.Aim) * 100f:0.0} cm from the aim, T {state.T:0.000} (contact {state.ContactTime:0.000}), weight {attacker.Pose.CorrectionWeight:0.00}, size {attacker.Pose.CorrectionSize * 100f:0.0} cm, found misses by {attacker.Pose.CorrectionMiss * 100f:0.0} cm, blend {state.Blend:0.00}, shoulder {Vector3.Distance(attacker.Anim.Part(PhysicsBattler.Rig.PartId.UpperArmR).position, state.ContactShoulder) * 100f:0.0} cm from the planned, chest {Vector3.Distance(attacker.Anim.Part(PhysicsBattler.Rig.PartId.Chest).position, attacker.Pose.ChestBeforeArms) * 100f:0.0}";
                            }
                        }

                        UnitController.MoveContact -= OnContact;
                        Console.WriteLine($"{attackerBody} {move.name} at {targetBody}'s {part}: reach {reachShare:0.00}, dry {dry * 100f:0.0} cm, played {error * 100f:0.0} cm{(error > MovePlanner.Tolerance ? " MISS" : "")}; at contact {atContact}");
                        MoveAimTests.Settle(attacker, target, 30);
                    }
                }

                UnityEngine.Object.DestroyImmediate(attacker.gameObject);
                UnityEngine.Object.DestroyImmediate(target.gameObject);
            }
        }

        return 0;
    }
}
