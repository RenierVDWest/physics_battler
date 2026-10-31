using System.IO;
using NUnit.Framework;
using PhysicsBattler.Motion;
using PhysicsBattler.Muscles;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Tests
{
    [TestFixture]
    public class TwoBoneIKTests
    {
        private static (Transform upper, Transform lower, Transform end) Arm()
        {
            var upper = new GameObject("Upper").transform;
            upper.position = new Vector3(0.4f, 1.56f, 0f);
            var lower = new GameObject("Lower").transform;
            lower.SetParent(upper, false);
            lower.localPosition = new Vector3(0.1f, -0.39f, 0f);
            var end = new GameObject("End").transform;
            end.SetParent(lower, false);
            end.localPosition = new Vector3(0.09f, -0.37f, 0f);
            return (upper, lower, end);
        }

        [Test]
        public void EndReachesAReachableTarget()
        {
            var (upper, lower, end) = Arm();
            foreach (var target in new[]
                     {
                         new Vector3(0.6f, 1.2f, 0.5f), new Vector3(0.3f, 1.8f, 0.4f), new Vector3(0.75f, 1.0f, -0.15f),
                         new Vector3(0.45f, 1.0f, 0.1f),
                     })
            {
                TwoBoneIK.Solve(upper, lower, end, target, Vector3.down);
                Assert.AreEqual(0f, Vector3.Distance(end.position, target), 0.002f, $"target {target}");
            }
        }

        [Test]
        public void BoneLengthsArePreserved()
        {
            var (upper, lower, end) = Arm();
            float a = Vector3.Distance(upper.position, lower.position);
            float b = Vector3.Distance(lower.position, end.position);
            TwoBoneIK.Solve(upper, lower, end, new Vector3(0.7f, 1.3f, 0.4f), Vector3.back);
            Assert.AreEqual(a, Vector3.Distance(upper.position, lower.position), 1e-4f);
            Assert.AreEqual(b, Vector3.Distance(lower.position, end.position), 1e-4f);
        }

        [Test]
        public void MiddleJointPointsTowardThePole()
        {
            var (upper, lower, end) = Arm();
            Vector3 target = new Vector3(0.4f, 1.56f, 0.55f);
            TwoBoneIK.Solve(upper, lower, end, target, Vector3.down);
            Vector3 axis = (target - upper.position).normalized;
            Vector3 elbow = Vector3.ProjectOnPlane(lower.position - upper.position, axis);
            Assert.Greater(Vector3.Dot(elbow.normalized, Vector3.down), 0.95f);
        }

        [Test]
        public void UnreachableTargetStretchesTowardIt()
        {
            var (upper, lower, end) = Arm();
            Vector3 target = upper.position + new Vector3(0f, 0f, 3f);
            TwoBoneIK.Solve(upper, lower, end, target, Vector3.down);
            Assert.Greater(Vector3.Dot((end.position - upper.position).normalized, Vector3.forward), 0.999f);
        }
    }

    [TestFixture]
    public class JointMathTests
    {
        [Test]
        public void TargetRotationIsTheInverseOfTheWantedLocalRotation()
        {
            var wanted = Quaternion.Euler(30f, -10f, 5f);
            var target = JointMath.TargetRotation(wanted, Quaternion.identity);
            if (!JointConventions.DriveInverted)
            {
                Assert.Less(Quaternion.Angle(target, Quaternion.Inverse(wanted)), 0.01f);
            }
        }

        [Test]
        public void PitchOfReadsRotationAboutX()
        {
            Assert.AreEqual(30f, JointMath.PitchOf(Quaternion.AngleAxis(30f, Vector3.right)), 0.01f);
            Assert.AreEqual(-75f, JointMath.PitchOf(Quaternion.AngleAxis(-75f, Vector3.right)), 0.01f);
        }

        [Test]
        public void RotationErrorIsAxisTimesAngle()
        {
            var error = JointMath.RotationError(Quaternion.identity, Quaternion.AngleAxis(40f, Vector3.up));
            Assert.AreEqual(40f * Mathf.Deg2Rad, error.magnitude, 1e-4f);
            Assert.Greater(Vector3.Dot(error.normalized, Vector3.up), 0.9999f);

            var back = JointMath.RotationError(Quaternion.AngleAxis(40f, Vector3.up), Quaternion.identity);
            Assert.Less(Vector3.Dot(back.normalized, Vector3.up), -0.9999f);
        }

        [Test]
        public void RotationErrorTakesTheShortWayRound()
        {
            var error = JointMath.RotationError(Quaternion.identity, Quaternion.AngleAxis(300f, Vector3.right));
            Assert.AreEqual(60f * Mathf.Deg2Rad, error.magnitude, 1e-3f);
        }

        [Test]
        public void HingeLimitsMatchTheirAnatomy()
        {
            Assert.Less(JointLimitTable.For(PartId.ForearmR).pitchMin, -100f, "elbows bend forward (negative pitch)");
            Assert.Greater(JointLimitTable.For(PartId.ShinL).pitchMax, 100f, "knees bend backward (positive pitch)");
            Assert.Less(JointLimitTable.For(PartId.ThighR).pitchMin, -90f, "hips raise the leg forward");
        }

        [Test]
        public void ForwardRaiseIsNegativePitchForAHangingBone()
        {
            // The convention the limit table relies on: rotating a bone that hangs down by a negative
            // angle about X swings it forward (+Z).
            Vector3 hanging = Vector3.down;
            Vector3 raised = Quaternion.AngleAxis(-60f, Vector3.right) * hanging;
            Assert.Greater(raised.z, 0.8f);
        }
    }

    [TestFixture]
    public class RigFileTests
    {
        private static readonly string[] Bodies = { "small", "medium", "large" };

        private static RigFile Load(string body)
        {
            string path = Path.Combine(Application.dataPath, $"_Battler/Data/Rigs/{body}_rig.json");
            return RigFile.Parse(File.ReadAllText(path));
        }

        [Test]
        public void EveryPartHasANullAndAnEntry()
        {
            foreach (var body in Bodies)
            {
                var rig = Load(body);
                foreach (var id in PartInfo.All)
                {
                    Assert.IsNotNull(rig.FindNull(PartInfo.NullName(id)), $"{body} null {id}");
                    Assert.IsNotNull(rig.FindPart(PartInfo.NullName(id)), $"{body} part {id}");
                }

                foreach (var aim in new[] { "Aim_Head", "Aim_Neck", "Aim_Chest", "Aim_Belly" })
                {
                    Assert.IsNotNull(rig.FindNull(aim), $"{body} {aim}");
                }
            }
        }

        [Test]
        public void NullParentsMatchThePartTree()
        {
            foreach (var body in Bodies)
            {
                var rig = Load(body);
                foreach (var id in PartInfo.All)
                {
                    if (PartInfo.HasParent(id))
                    {
                        Assert.AreEqual(PartInfo.NullName(PartInfo.Parent(id)), rig.FindNull(PartInfo.NullName(id)).parent, $"{body} {id}");
                    }
                }
            }
        }

        [Test]
        public void MassesAddUpAndCollidersAreComplete()
        {
            foreach (var body in Bodies)
            {
                var rig = Load(body);
                float total = 0f;
                foreach (var part in rig.parts)
                {
                    total += part.massKg;
                    Assert.IsNotNull(part.collider, part.name);
                    if (part.collider.type == "capsule")
                    {
                        Assert.IsTrue(part.collider.start != null && part.collider.end != null, part.name);
                    }
                }

                Assert.AreEqual(rig.totalMassKg, total, 0.05f * rig.totalMassKg / 80f + 0.05f, body);
            }
        }

        [Test]
        public void RightSideIsPositiveX()
        {
            foreach (var body in Bodies)
            {
                var rig = Load(body);
                Assert.Greater(rig.FindNull("UpperArm_R").LocalPosition.x, 0f, body);
                Assert.Less(rig.FindNull("UpperArm_L").LocalPosition.x, 0f, body);
            }
        }
    }

    [TestFixture]
    public class PartInfoTests
    {
        [Test]
        public void EveryPartLeadsBackToThePelvis()
        {
            foreach (var id in PartInfo.All)
            {
                var current = id;
                int guard = 0;
                while (PartInfo.HasParent(current) && guard++ < 10)
                {
                    current = PartInfo.Parent(current);
                }

                Assert.AreEqual(PartId.Pelvis, current, $"{id}");
            }
        }

        [Test]
        public void NamesRoundTrip()
        {
            foreach (var id in PartInfo.All)
            {
                Assert.IsTrue(PartInfo.TryParse(PartInfo.NullName(id), out var parsed));
                Assert.AreEqual(id, parsed);
            }
        }
    }
}
