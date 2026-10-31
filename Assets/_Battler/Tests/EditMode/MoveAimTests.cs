using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using PhysicsBattler.Combat;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Tests
{
    /// <summary>
    /// Milestone 2's gate: every blow the planner chooses reaches the body part it is aimed at
    /// (the striking part passes within 3 cm of the aim point at contact), for every body size
    /// against every other, every move and every part; and the moves themselves are well formed.
    /// </summary>
    [TestFixture]
    public class MoveAimTests
    {
        private static readonly string[] Bodies = { "Small", "Medium", "Large" };

        private static WeaponDefinition Sword()
        {
            var sword = WeaponDefinition.CreateDefault(WeaponKind.Sword);
            sword.moves = new[] { MoveDefinition.Create(MoveStyle.SwordCut), MoveDefinition.Create(MoveStyle.SwordBackhand), MoveDefinition.Create(MoveStyle.SwordThrust) };
            return sword;
        }

        private static WeaponDefinition Club()
        {
            var club = WeaponDefinition.CreateDefault(WeaponKind.Club);
            club.moves = new[] { MoveDefinition.Create(MoveStyle.ClubSwing), MoveDefinition.Create(MoveStyle.ClubSmash) };
            return club;
        }

        private static UnitController Build(string body, Vector3 at, float yaw, int team)
        {
            string path = Path.Combine(Application.dataPath, $"_Battler/Data/Rigs/{body.ToLowerInvariant()}_rig.json");
            var rig = RigFile.Parse(File.ReadAllText(path));
            var profile = UnitProfile.CreateDefault();
            profile.body = body;
            var root = UnitBuilder.Build(rig, profile, "Test " + body, _ => null, 0);
            root.transform.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            var unit = root.GetComponent<UnitController>();
            unit.Profile = profile;
            unit.Team = team;
            // Edit mode: nothing calls Awake by itself.
            typeof(UnitController).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(unit, null);
            return unit;
        }

        private static void Destroy(params UnitController[] units)
        {
            foreach (var unit in units)
            {
                if (unit != null)
                {
                    UnityEngine.Object.DestroyImmediate(unit.gameObject);
                }
            }
        }

        private static void Settle(UnitController a, UnitController b, int steps)
        {
            for (int i = 0; i < steps; i++)
            {
                a.PreviewPose(1f / 60f);
                b.PreviewPose(1f / 60f);
            }
        }

        /// <summary>
        /// Plays a blow to the end; returns how close the striking part passed the aim point at
        /// contact (-1: no contact), and the least clearance of the weapon and weapon arm from the
        /// attacker's own body on any step.
        /// </summary>
        private static float Play(UnitController attacker, UnitController target, MoveState state) => Play(attacker, target, state, out _);

        private static float Play(UnitController attacker, UnitController target, MoveState state, out float clearance) =>
            Play(attacker, target, state, out clearance, out _);

        private static float Play(UnitController attacker, UnitController target, MoveState state, out float clearance, out float hipDrop)
        {
            float error = -1f;
            clearance = float.MaxValue;
            hipDrop = 0f;
            void OnContact(UnitController unit, MoveState move, ContactReport report)
            {
                if (unit == attacker)
                {
                    error = report.AnimError;
                }
            }

            UnitController.MoveContact += OnContact;
            try
            {
                Assert.IsTrue(attacker.StartMove(state));
                float length = state.Weapon != null ? state.Weapon.length * 0.98f : 0.8f;
                for (int i = 0; i < 400 && attacker.IsAttacking; i++)
                {
                    attacker.PreviewPose(1f / 60f);
                    target.PreviewPose(1f / 60f);
                    clearance = Mathf.Min(clearance, BodyClearance.Measure(attacker.Rig, attacker.Anim, length, attacker.Loadout.Scale, out _, out _));
                    hipDrop = Mathf.Max(hipDrop, attacker.Pose.PelvisDrop);
                }
            }
            finally
            {
                UnitController.MoveContact -= OnContact;
            }

            Assert.IsFalse(attacker.IsAttacking, "the blow ends");
            Settle(attacker, target, 30);
            return error;
        }

        [Test]
        public void MovesAreWellFormed()
        {
            foreach (MoveStyle style in Enum.GetValues(typeof(MoveStyle)))
            {
                var move = MoveDefinition.Create(style);
                float tc = move.ContactTime();
                Assert.Less(move.PointAt(move.ContactProgress).magnitude, 1e-4f, $"{style}: the contact point is the aim point");
                Assert.AreEqual(1f, move.BlendAt(tc), 0.01f, $"{style}: the move has taken over fully at contact");
                Assert.Greater(move.BodyAt(tc), 0.9f, $"{style}: the lunge is whole at contact");
                float last = -1f;
                for (int i = 0; i <= 200; i++)
                {
                    float p = move.ProgressAt(i / 200f);
                    Assert.GreaterOrEqual(p, last - 1e-4f, $"{style}: the path never runs backwards (t={i / 200f:0.000})");
                    last = p;
                }

                Assert.AreEqual(0f, move.BlendAt(0f), 1e-4f, $"{style}: blends in");
                Assert.AreEqual(0f, move.BlendAt(1f), 1e-4f, $"{style}: blends out");
                Assert.Greater(move.activeWindow.y, move.ContactProgress, $"{style}: contact is inside the active window");
                Assert.Less(move.activeWindow.x, move.ContactProgress, $"{style}: contact is inside the active window");
            }
        }

        [Test] public void SmallBlowsLand() => EveryPlannedBlowLands("Small");
        [Test] public void MediumBlowsLand() => EveryPlannedBlowLands("Medium");
        [Test] public void LargeBlowsLand() => EveryPlannedBlowLands("Large");

        /// <summary>
        /// Against each target size, at the distance the sandbox closes in to: every move at every
        /// part the planner accepts lands within 3 cm and keeps the weapon and weapon arm out of the
        /// attacker's own body, and every size pair has something to hit.
        /// </summary>
        private static void EveryPlannedBlowLands(string attackerBody)
        {
            foreach (var targetBody in Bodies)
            {
                foreach (var weapon in new[] { Sword(), Club() })
                {
                    var attacker = Build(attackerBody, Vector3.zero, 0f, 0);
                    var target = Build(targetBody, Vector3.forward * 5f, 180f, 1);
                    try
                    {
                        float distance = attacker.Style.engageDistance * attacker.Body.SizeFactor + 0.25f * target.Body.SizeFactor;
                        target.Place(Vector3.forward * distance, 180f);
                        attacker.Equip(weapon, null);
                        Settle(attacker, target, 20);
                        int planned = 0;
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

                                planned++;
                                float error = Play(attacker, target, state, out float clearance, out float hipDrop);
                                string what = $"{attackerBody} {move.name} at {targetBody}'s {part} from {distance:0.00} m";
                                Assert.GreaterOrEqual(error, 0f, what + ": reached contact");
                                Assert.Less(error, MovePlanner.Tolerance, what + ": lands on the aim point");
                                // The planner checks a handful of poses; played step by step, allow a little more.
                                Assert.Greater(clearance, -(MovePlanner.OwnBodyClearance + 0.02f) * attacker.Body.SizeFactor,
                                    what + ": the weapon and arm stay out of the attacker's own body");
                                // The lunge's steps keep the legs under the hips: no deep drop to reach the feet.
                                Assert.Less(hipDrop, 0.12f * attacker.Body.LegLength, what + ": the hips stay up through the lunge");
                            }
                        }

                        Assert.Greater(planned, 0, $"{attackerBody} with a {weapon.kind} has a blow that reaches a {targetBody}");
                    }
                    finally
                    {
                        Destroy(attacker, target);
                    }
                }
            }
        }

        [Test]
        public void MediumReachesUpToALargeChestAndDownToASmallHead()
        {
            foreach (var (targetBody, part) in new[] { ("Large", AimPart.Chest), ("Small", AimPart.Head) })
            {
                var attacker = Build("Medium", Vector3.zero, 0f, 0);
                var target = Build(targetBody, Vector3.forward * 1.6f, 180f, 1);
                try
                {
                    attacker.Equip(Sword(), null);
                    Settle(attacker, target, 20);
                    var state = MovePlanner.Prepare(attacker, target, attacker.Loadout.Main.moves[0], part);
                    Assert.IsNotNull(state, $"{targetBody} {part} is within a lunge");
                    Assert.Less(attacker.TryOut(state), MovePlanner.Tolerance, $"{targetBody} {part} is planned to land");
                    Assert.Less(Play(attacker, target, state), MovePlanner.Tolerance, $"{targetBody} {part} lands");
                }
                finally
                {
                    Destroy(attacker, target);
                }
            }
        }

        [Test]
        public void TheAimIsLockedWhereThePartWas()
        {
            var attacker = Build("Medium", Vector3.zero, 0f, 0);
            var target = Build("Medium", Vector3.forward * 1.6f, 180f, 1);
            try
            {
                attacker.Equip(Sword(), null);
                Settle(attacker, target, 20);
                var state = MovePlanner.Prepare(attacker, target, attacker.Loadout.Main.moves[0], AimPart.Chest);
                Vector3 locked = state.Aim;
                attacker.StartMove(state);
                // The target steps aside during the wind-up: the blow still goes where the chest was.
                target.Place(new Vector3(0.6f, 0f, 1.6f), 180f);
                for (int i = 0; i < 400 && attacker.IsAttacking; i++)
                {
                    attacker.PreviewPose(1f / 60f);
                    target.PreviewPose(1f / 60f);
                    if (attacker.CurrentMove != null)
                    {
                        Assert.AreEqual(locked, attacker.CurrentMove.Aim, "the aim point never follows the target");
                    }
                }
            }
            finally
            {
                Destroy(attacker, target);
            }
        }

        [Test]
        public void PlantedFeetStayPutThroughABlow()
        {
            var attacker = Build("Medium", Vector3.zero, 0f, 0);
            var target = Build("Large", Vector3.forward * 1.7f, 180f, 1);
            try
            {
                attacker.Equip(Club(), null);
                Settle(attacker, target, 20);
                var state = MovePlanner.Prepare(attacker, target, attacker.Loadout.Main.moves[0], AimPart.Belly);
                Assert.IsNotNull(state);
                attacker.StartMove(state);
                var last = new[] { attacker.Stance.Position(Stance.Left), attacker.Stance.Position(Stance.Right) };
                var wasStepping = new[] { false, false };
                int steps = 0;
                for (int i = 0; i < 400 && attacker.IsAttacking; i++)
                {
                    attacker.PreviewPose(1f / 60f);
                    for (int f = 0; f < 2; f++)
                    {
                        bool stepping = attacker.Stance.Stepping(f);
                        if (!stepping && !wasStepping[f])
                        {
                            Assert.Less(Vector3.Distance(last[f], attacker.Stance.Position(f)), 0.001f, $"foot {f} slid while planted");
                        }

                        steps += stepping && !wasStepping[f] ? 1 : 0;
                        wasStepping[f] = stepping;
                        last[f] = attacker.Stance.Position(f);
                    }
                }

                Assert.GreaterOrEqual(steps, 2, "the lunge steps out and back");
                Assert.Less(attacker.Pose.PelvisDrop, 0.05f, "the hips are back up after the blow");
            }
            finally
            {
                Destroy(attacker, target);
            }
        }

        [Test]
        public void ShieldIsHeldInFrontOnTheLeft()
        {
            var unit = Build("Medium", Vector3.zero, 0f, 0);
            try
            {
                unit.Equip(Sword(), WeaponDefinition.CreateDefault(WeaponKind.Shield));
                for (int i = 0; i < 30; i++)
                {
                    unit.PreviewPose(1f / 60f);
                }

                var slot = unit.Anim.SlotWeaponL;
                Vector3 face = slot.position + slot.rotation * unit.Loadout.Off.faceCenter;
                Vector3 chest = unit.Anim.Part(PartId.Chest).position;
                Assert.Greater(face.z - chest.z, 0.2f, "in front of the chest");
                Assert.Less(face.x - chest.x, 0f, "on the left");
                Assert.Greater(Vector3.Dot(slot.rotation * Vector3.forward, Vector3.forward), 0.8f, "facing forward");
            }
            finally
            {
                Destroy(unit);
            }
        }
    }
}
