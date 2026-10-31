using NUnit.Framework;
using PhysicsBattler.Authority;
using PhysicsBattler.Board;
using PhysicsBattler.Motion;
using UnityEngine;

namespace PhysicsBattler.Tests
{
    [TestFixture]
    public class StanceTests
    {
        private static Stance Make(out CombatStyle style, float shuffle = 0f)
        {
            style = CombatStyle.CreateDefault();
            style.shuffleInterval = shuffle;
            var stance = new Stance(3);
            stance.Configure(style, BodyMeasure.Simple());
            stance.Plant(Vector3.zero, 0f);
            return stance;
        }

        [Test]
        public void HomeIsAStaggeredStance()
        {
            var stance = Make(out var style);
            float leg = BodyMeasure.Simple().LegLength;
            var left = stance.HomeLocal(Stance.Left);
            var right = stance.HomeLocal(Stance.Right);
            Assert.AreEqual(style.stanceWidth * leg, right.x - left.x, 1e-4f, "width");
            Assert.AreEqual(style.stanceLength * leg, left.z - right.z, 1e-4f, "left foot leads");
            Assert.IsTrue(stance.IsLead(Stance.Left));
            Assert.Less(stance.HomeYawLocal(Stance.Left), 0f, "left toes turn out to the left");
            Assert.Greater(stance.HomeYawLocal(Stance.Right), 0f, "right toes turn out to the right");
        }

        [Test]
        public void StandingStillTakesNoSteps()
        {
            var stance = Make(out _);
            for (int i = 0; i < 300; i++)
            {
                stance.Update(Vector3.zero, 0f, 1f / 60f);
            }

            Assert.AreEqual(0, stance.StepsTaken);
            Assert.AreEqual(0f, stance.Error(Stance.Left, Vector3.zero, 0f), 1e-5f);
        }

        [Test]
        public void SmallMovesStayPlanted()
        {
            var stance = Make(out var style);
            float leg = BodyMeasure.Simple().LegLength;
            var root = new Vector3(0.5f * style.stepThreshold * leg, 0f, 0f);
            for (int i = 0; i < 60; i++)
            {
                stance.Update(root, 0.5f * style.stepTurnThreshold, 1f / 60f);
            }

            Assert.AreEqual(0, stance.StepsTaken, "under the thresholds the feet hold");
        }

        [Test]
        public void MovingTheRootStepsBothFeetHome()
        {
            var stance = Make(out _);
            var root = new Vector3(0.4f, 0f, 0.3f);
            int landed = 0;
            stance.Landed += (_, __) => landed++;
            for (int i = 0; i < 240; i++)
            {
                stance.Update(root, 0f, 1f / 60f);
            }

            Assert.GreaterOrEqual(stance.StepsTaken, 2);
            Assert.AreEqual(stance.StepsTaken, landed);
            Assert.Less(stance.Error(Stance.Left, root, 0f), 0.01f);
            Assert.Less(stance.Error(Stance.Right, root, 0f), 0.01f);
        }

        [Test]
        public void TurningAroundStepsAndOneFootAtATime()
        {
            var stance = Make(out _);
            for (int i = 0; i < 360; i++)
            {
                stance.Update(Vector3.zero, 180f, 1f / 60f);
                Assert.IsFalse(stance.Stepping(Stance.Left) && stance.Stepping(Stance.Right), "never both feet off the ground");
            }

            Assert.GreaterOrEqual(stance.StepsTaken, 2);
            Assert.AreEqual(0f, Mathf.DeltaAngle(stance.Yaw(Stance.Left), 180f + stance.HomeYawLocal(Stance.Left)), 1f);
        }

        [Test]
        public void StepsLiftOffTheGround()
        {
            var stance = Make(out _);
            float maxLift = 0f;
            for (int i = 0; i < 60; i++)
            {
                stance.Update(new Vector3(0.5f, 0f, 0f), 0f, 1f / 60f);
                maxLift = Mathf.Max(maxLift, stance.Lift(Stance.Left), stance.Lift(Stance.Right));
            }

            Assert.Greater(maxLift, 0.02f);
        }

        [Test]
        public void IdleShufflesHappen()
        {
            var stance = Make(out _, shuffle: 1f);
            for (int i = 0; i < 60 * 10; i++)
            {
                stance.Update(Vector3.zero, 0f, 1f / 60f);
            }

            Assert.GreaterOrEqual(stance.ShufflesTaken, 4);
            Assert.Less(stance.Error(Stance.Left, Vector3.zero, 0f), 0.2f, "shuffles stay near home");
        }
    }

    [TestFixture]
    public class CombatIdleTests
    {
        [Test]
        public void MotionStaysWithinTheStyle()
        {
            var style = CombatStyle.CreateDefault();
            var idle = new CombatIdle(7);
            float leg = 0.845f;
            float maxShift = 0f, minBob = 0f, maxTurn = 0f;
            for (int i = 0; i < 2000; i++)
            {
                var s = idle.Sample(i * 0.02f, style, leg);
                maxShift = Mathf.Max(maxShift, Mathf.Abs(s.hipShift.x));
                minBob = Mathf.Min(minBob, s.bob);
                Assert.LessOrEqual(s.bob, 1e-6f, "bob only dips");
                maxTurn = Mathf.Max(maxTurn, Mathf.Abs(s.chest.y));
            }

            Assert.LessOrEqual(maxShift, style.swayAmplitude * leg + 1e-5f);
            Assert.Greater(maxShift, 0.5f * style.swayAmplitude * leg, "it does sway");
            Assert.GreaterOrEqual(minBob, -style.bobAmplitude * leg - 1e-5f);
            Assert.LessOrEqual(maxTurn, style.weaveDegrees + 1e-4f);
        }

        [Test]
        public void UnitsDoNotMoveInStep()
        {
            var style = CombatStyle.CreateDefault();
            var a = new CombatIdle(1).Sample(1.3f, style, 1f);
            var b = new CombatIdle(2).Sample(1.3f, style, 1f);
            Assert.AreNotEqual(a.hipShift.x, b.hipShift.x);
        }

        [Test]
        public void ItIsSmooth()
        {
            var style = CombatStyle.CreateDefault();
            var idle = new CombatIdle(4);
            float dt = 1f / 60f;
            for (int i = 0; i < 600; i++)
            {
                var s0 = idle.Sample(i * dt, style, 1f);
                var s1 = idle.Sample((i + 1) * dt, style, 1f);
                Assert.Less(Mathf.Abs(s1.hipShift.x - s0.hipShift.x), 0.01f);
                Assert.Less(Mathf.Abs(s1.chest.y - s0.chest.y), 1f);
            }
        }
    }

    [TestFixture]
    public class AuthorityTests
    {
        [Test]
        public void FullPinLandsOnTheTargetDespiteGravity()
        {
            float dt = 1f / 60f;
            var position = new Vector3(0f, 1f, 0f);
            var target = new Vector3(0.05f, 1.02f, -0.03f);
            var velocity = AuthorityController.PinVelocity(position, target, new Vector3(1f, 2f, 3f), 1f, dt, Physics.gravity);
            // What the solver does: gravity into the velocity, then the velocity into the position.
            velocity += Physics.gravity * dt;
            position += velocity * dt;
            Assert.Less(Vector3.Distance(position, target), 1e-5f);
        }

        [Test]
        public void NoPinKeepsTheVelocity()
        {
            var v = new Vector3(1f, 2f, 3f);
            var result = AuthorityController.PinVelocity(Vector3.zero, Vector3.one, v, 0f, 1f / 60f, Physics.gravity);
            Assert.Less(Vector3.Distance(result, v), 1e-6f);
        }

        [Test]
        public void HalfPinConvergesOverTime()
        {
            float dt = 1f / 60f;
            var position = Vector3.zero;
            var velocity = new Vector3(0f, 0f, 5f);
            var target = new Vector3(0.3f, 0.5f, 0f);
            for (int i = 0; i < 120; i++)
            {
                velocity = AuthorityController.PinVelocity(position, target, velocity, 0.5f, dt, Physics.gravity);
                velocity += Physics.gravity * dt;
                position += velocity * dt;
            }

            Assert.Less(Vector3.Distance(position, target), 0.01f);
        }

        [Test]
        public void FullAngularPinLandsOnTheTarget()
        {
            float dt = 1f / 60f;
            var rotation = Quaternion.Euler(10f, 20f, 0f);
            var target = Quaternion.Euler(14f, 15f, 6f);
            var w = AuthorityController.PinAngularVelocity(rotation, target, Vector3.zero, 1f, dt);
            float angle = w.magnitude * dt * Mathf.Rad2Deg;
            rotation = Quaternion.AngleAxis(angle, w.normalized) * rotation;
            Assert.Less(Quaternion.Angle(rotation, target), 0.05f);
        }

        [Test]
        public void RegionsCoverTheBody()
        {
            Assert.AreEqual(BodyRegion.Legs, AuthorityController.RegionOf(Rig.PartId.ShinL));
            Assert.AreEqual(BodyRegion.Legs, AuthorityController.RegionOf(Rig.PartId.FootR));
            Assert.AreEqual(BodyRegion.Head, AuthorityController.RegionOf(Rig.PartId.Head));
            Assert.AreEqual(BodyRegion.ArmL, AuthorityController.RegionOf(Rig.PartId.HandL));
            Assert.AreEqual(BodyRegion.ArmR, AuthorityController.RegionOf(Rig.PartId.UpperArmR));
            Assert.AreEqual(BodyRegion.Torso, AuthorityController.RegionOf(Rig.PartId.Chest));
        }
    }

    [TestFixture]
    public class BoardGridTests
    {
        [Test]
        public void TheBoardIsCentred()
        {
            var grid = new BoardGrid();
            var a = grid.Center(new Tile(0, 0));
            var b = grid.Center(new Tile(7, 7));
            Assert.Less(Vector3.Distance(Vector3.zero, 0.5f * (a + b)), 1e-5f);
            Assert.AreEqual(7 * grid.tileSize, b.x - a.x, 1e-4f);
        }

        [Test]
        public void TileAtFindsTheCentre()
        {
            var grid = new BoardGrid();
            for (int c = 0; c < 8; c++)
            {
                for (int r = 0; r < 8; r++)
                {
                    var tile = grid.TileAt(grid.Center(new Tile(c, r)) + new Vector3(0.3f, 0f, -0.3f));
                    Assert.AreEqual(c, tile.column);
                    Assert.AreEqual(r, tile.row);
                }
            }
        }

        [Test]
        public void TilesHaveChessNames()
        {
            Assert.AreEqual("C3", new Tile(2, 2).ToString());
            Assert.IsFalse(new BoardGrid().Contains(new Tile(8, 0)));
        }
    }

    [TestFixture]
    public class EulerTests
    {
        [Test]
        public void EulerAnglesRoundTrip()
        {
            foreach (var e in new[] { new Vector3(10f, 20f, 30f), new Vector3(-40f, 170f, 5f), new Vector3(80f, -30f, -60f) })
            {
                var q = Quaternion.Euler(e);
                var back = Quaternion.Euler(q.eulerAngles);
                Assert.Less(Quaternion.Angle(q, back), 0.01f, e.ToString());
            }
        }
    }
}
