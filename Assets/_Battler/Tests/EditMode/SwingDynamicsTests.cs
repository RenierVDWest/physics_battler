using NUnit.Framework;
using PhysicsBattler.Motion;
using UnityEngine;

namespace PhysicsBattler.Tests
{
    [TestFixture]
    public class SwingDynamicsTests
    {
        private static readonly Vector3 Shoulder = new Vector3(0.4f, 1.56f, 0f);
        private static readonly Vector3 Target = new Vector3(0.1f, 1.46f, 1.9f);

        private static SwingPlan PlanWithRoll(float roll)
        {
            var reach = new SwingReach { sweetSpot = 1.5f, tip = 1.86f, minArm = 0.3f, maxArm = 0.72f, maxLean = 0.2f };
            return SwingSolver.Plan(Shoulder, Target, reach, roll, 1);
        }

        // The orc and its club, roughly: about 16.5 kg·m² and 11 kg·m about the shoulder.
        private static SwingLoad Club => new SwingLoad { inertia = 16.5f, massMoment = 11f, weaponMass = 6f, weaponCentre = 1.05f };

        [Test]
        public void RestToRestStartsAndEndsStillAndIsSymmetric()
        {
            Assert.AreEqual(0f, SwingDynamics.RestToRest(0f, 1f), 1e-5f);
            Assert.AreEqual(0.5f, SwingDynamics.RestToRest(0.5f, 1f), 1e-5f);
            Assert.AreEqual(1f, SwingDynamics.RestToRest(1f, 1f), 1e-5f);
            float early = SwingDynamics.RestToRest(0.01f, 1f);
            float late = 1f - SwingDynamics.RestToRest(0.99f, 1f);
            Assert.AreEqual(early, late, 1e-5f, "accelerates as gently as it brakes");
            Assert.Less(early, 0.001f, "starts from rest");
        }

        [Test]
        public void HeavierIsSlowerToMove()
        {
            var light = new SwingLoad { inertia = 4f, massMoment = 4f };
            float lightTime = SwingDynamics.RestToRestTime(1.5f, SwingDynamics.MoveAcceleration(240f, 0.55f, light));
            float heavyTime = SwingDynamics.RestToRestTime(1.5f, SwingDynamics.MoveAcceleration(240f, 0.55f, Club));
            Assert.Greater(heavyTime, lightTime * 1.5f);
            Assert.Greater(heavyTime, 0.8f, "the big club takes a while to pull back");
        }

        [Test]
        public void GravityPullsALevelSwingDown()
        {
            var plan = PlanWithRoll(0f);   // a vertical plane: +θ lifts the weapon
            Assert.Less(SwingDynamics.GravityTorque(plan, 0f, Club.massMoment), 0f);
            var flat = PlanWithRoll(90f);
            Assert.Less(Mathf.Abs(SwingDynamics.GravityTorque(flat, 0f, Club.massMoment)),
                Mathf.Abs(SwingDynamics.GravityTorque(plan, 0f, Club.massMoment)) * 0.3f, "a flat sweep barely fights gravity");
        }

        [Test]
        public void StrikeStopsWithinItsFollowThrough()
        {
            var plan = PlanWithRoll(85f);
            var definition = AttackDefinition.CreateDefault();
            SwingDynamics.Predict(plan, Club, 240f, definition, out float time, out float omega, out float stop);
            Assert.Greater(time, 0.3f);
            Assert.Less(time, 2f);
            Assert.Greater(omega, 1f, "it still arrives with some speed");
            Assert.Less(stop, definition.followAngle + 15f, "braked to a stop near the end of the follow-through");
        }

        [Test]
        public void StrongerSwingsArriveSooner()
        {
            var plan = PlanWithRoll(60f);
            var definition = AttackDefinition.CreateDefault();
            SwingDynamics.Predict(plan, Club, 180f, definition, out float weakTime, out _, out _);
            SwingDynamics.Predict(plan, Club, 400f, definition, out float strongTime, out _, out _);
            Assert.Less(strongTime, weakTime);
        }
    }
}
