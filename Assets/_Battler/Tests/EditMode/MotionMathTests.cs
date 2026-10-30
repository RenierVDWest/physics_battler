using System.IO;
using NUnit.Framework;
using PhysicsBattler.Motion;
using PhysicsBattler.Muscles;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Tests
{
    [TestFixture]
    public class SwingSolverTests
    {
        // An orc-sized swing: shoulder 1.56 m up, the target's chest 1.9 m ahead and a little lower.
        private static readonly Vector3 Shoulder = new Vector3(0.4f, 1.56f, 0f);
        private static readonly Vector3 Target = new Vector3(0.1f, 1.46f, 1.9f);

        private static SwingReach Reach => new SwingReach
        {
            sweetSpot = 1.5f, tip = 1.86f, minArm = 0.3f, maxArm = 0.72f, maxLean = 0.2f,
        };

        [Test]
        public void ContactDirectionPointsAtTheTarget()
        {
            var plan = SwingSolver.Plan(Shoulder, Target, Reach, 45f, 1);
            Vector3 direction = SwingSolver.Direction(plan, 0f);
            Assert.Less(Vector3.Angle(direction, Target - Shoulder), 0.01f);
        }

        [Test]
        public void SweetSpotReachesTheTargetAtContact()
        {
            var plan = SwingSolver.Plan(Shoulder, Target, Reach, 45f, 1);
            Assert.IsTrue(plan.inRange);
            Vector3 sweet = Shoulder + SwingSolver.Direction(plan, 0f) * (plan.contactReach + Reach.sweetSpot);
            Assert.AreEqual(0f, Vector3.Distance(sweet, Target), 0.001f);
            Assert.AreEqual(1f, plan.crowdedFactor, 1e-4f);
        }

        [Test]
        public void PositiveThetaLiftsTheWeaponAndNegativeThetaDropsIt()
        {
            var plan = SwingSolver.Plan(Shoulder, Target, Reach, 0f, 1);
            Vector3 contact = SwingSolver.Direction(plan, 0f);
            Assert.Greater(SwingSolver.Direction(plan, 20f).y, contact.y);
            Assert.Less(SwingSolver.Direction(plan, -20f).y, contact.y);
            Assert.Less(Vector3.Dot(SwingSolver.Direction(plan, -170f), contact), -0.9f, "-170 points back");
        }

        [Test]
        public void RightHandedRollPutsTheLowPointOnTheRight()
        {
            // Attacker faces +Z, so its right is +X. At θ = -90 the weapon is at the bottom of the arc.
            var plan = SwingSolver.Plan(new Vector3(0f, 1.5f, 0f), new Vector3(0f, 1.5f, 2f), Reach, 45f, 1);
            Vector3 bottom = SwingSolver.Direction(plan, -90f);
            Assert.Greater(bottom.x, 0.5f, "out to the right");
            Assert.Less(bottom.y, -0.5f, "and down");

            var left = SwingSolver.Plan(new Vector3(0f, 1.5f, 0f), new Vector3(0f, 1.5f, 2f), Reach, 45f, -1);
            Assert.Less(SwingSolver.Direction(left, -90f).x, -0.5f, "a left-handed swing mirrors it");
        }

        [Test]
        public void AngleOfInvertsDirection()
        {
            var plan = SwingSolver.Plan(Shoulder, Target, Reach, 45f, 1);
            foreach (float theta in new[] { -150f, -90f, -30f, 0f, 25f, 40f })
            {
                Assert.AreEqual(theta, SwingSolver.AngleOf(plan, SwingSolver.Direction(plan, theta)), 0.05f);
            }
        }

        [Test]
        public void LongWeaponGetsAFlatterPlaneToClearTheGround()
        {
            float roll = SwingSolver.RollForClearance(Shoulder, Target, Reach, 0f, 1, -160f, 0f, 0.15f);
            Assert.Greater(roll, 0f);
            var plan = SwingSolver.Plan(Shoulder, Target, Reach, roll, 1);
            for (float theta = -160f; theta <= 0f; theta += 5f)
            {
                Vector3 d = SwingSolver.Direction(plan, theta);
                float lowest = Shoulder.y + Mathf.Min(0f, d.y) * (Reach.minArm + Reach.tip);
                Assert.Greater(lowest, 0.149f, $"clear at θ {theta}");
            }
        }

        [Test]
        public void GroundLimitedReachKeepsTheTipUp()
        {
            Vector3 down = new Vector3(0f, -0.8f, 0.6f).normalized;
            float reach = SwingSolver.GroundLimitedReach(Shoulder, down, 1.86f, 0f, 0.15f);
            float tipHeight = Shoulder.y + down.y * (reach + 1.86f);
            Assert.AreEqual(0.15f, tipHeight, 1e-3f);
            Assert.IsTrue(SwingSolver.GroundLimitedReach(Shoulder, Vector3.up, 1.86f, 0f, 0.15f) > 100f);
        }

        [Test]
        public void TooCloseIsACrowdedHitAndTooFarIsOutOfRange()
        {
            var close = SwingSolver.Plan(Shoulder, Shoulder + Vector3.forward * 1.2f, Reach, 45f, 1);
            Assert.Less(close.crowdedFactor, 1f);
            Assert.IsTrue(close.inRange);

            var lean = SwingSolver.Plan(Shoulder, Shoulder + Vector3.forward * 2.35f, Reach, 45f, 1);
            Assert.IsTrue(lean.inRange);
            Assert.Greater(lean.extraLean, 0.05f);

            var far = SwingSolver.Plan(Shoulder, Shoulder + Vector3.forward * 3f, Reach, 45f, 1);
            Assert.IsFalse(far.inRange);
        }
    }

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
            Assert.Less(JointLimits.For(PartId.ForearmR).pitchMin, -100f, "elbows bend forward (negative pitch)");
            Assert.Greater(JointLimits.For(PartId.ShinL).pitchMax, 100f, "knees bend backward (positive pitch)");
            Assert.Less(JointLimits.For(PartId.ThighR).pitchMin, -90f, "hips raise the leg forward");
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
        private static RigFile Load()
        {
            string path = Path.Combine(Application.dataPath, "_Battler/Data/Rigs/orc_rig.json");
            return RigFile.Parse(File.ReadAllText(path));
        }

        [Test]
        public void EveryPartHasANullAndAnEntry()
        {
            var rig = Load();
            foreach (var id in PartInfo.All)
            {
                Assert.IsNotNull(rig.FindNull(PartInfo.NullName(id)), $"null {id}");
                Assert.IsNotNull(rig.FindPart(PartInfo.NullName(id)), $"part {id}");
            }
        }

        [Test]
        public void NullParentsMatchThePartTree()
        {
            var rig = Load();
            foreach (var id in PartInfo.All)
            {
                if (PartInfo.HasParent(id))
                {
                    Assert.AreEqual(PartInfo.NullName(PartInfo.Parent(id)), rig.FindNull(PartInfo.NullName(id)).parent, $"{id}");
                }
            }
        }

        [Test]
        public void MassesAddUpAndCollidersAreComplete()
        {
            var rig = Load();
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

            Assert.AreEqual(rig.totalMassKg, total, 0.05f);
            Assert.IsNotNull(rig.weapon);
            Assert.AreEqual(1.5f, RigMath.ToVector(rig.weapon.markers.sweet_spot).magnitude, 0.01f);
        }

        [Test]
        public void RightSideIsPositiveX()
        {
            var rig = Load();
            Assert.Greater(rig.FindNull("UpperArm_R").LocalPosition.x, 0f);
            Assert.Less(rig.FindNull("UpperArm_L").LocalPosition.x, 0f);
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

        [Test]
        public void EasingHitsItsEnds()
        {
            Assert.AreEqual(0f, Ease.InOutCubic(0f), 1e-6f);
            Assert.AreEqual(1f, Ease.InOutCubic(1f), 1e-6f);
            Assert.AreEqual(1f, Ease.OutCubic(1f), 1e-6f);
            Assert.AreEqual(0.25f, Ease.InPower(0.5f, 2f), 1e-6f);
        }
    }
}
