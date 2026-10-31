using NUnit.Framework;
using PhysicsBattler.Muscles;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Tests
{
    [TestFixture]
    public class DriveGainTests
    {
        [Test]
        public void LoadIsDampedAsAskedWhateverTheBoost()
        {
            foreach (float boost in new[] { 1f, 25f, 300f })
            {
                JointMath.DriveGains(3.4f, 0.75f, 0.8f, boost, out float spring, out float damper);
                // The load's inertia is boost × the inertia PhysX scales by, so divide both by boost.
                float omega = Mathf.Sqrt(spring / boost);
                float ratio = damper / boost / (2f * omega);
                Assert.AreEqual(0.75f, ratio, 1e-3f, $"damping ratio of the load at boost {boost}");
                Assert.AreEqual(2f * Mathf.PI * 3.4f * Mathf.Sqrt(0.8f), omega, 1e-2f, "frequency of the load");
            }
        }

        [Test]
        public void BodyAngularVelocityRecoversAxisAndRate()
        {
            var from = Quaternion.AngleAxis(20f, new Vector3(0.3f, 1f, 0.2f).normalized);
            var axis = new Vector3(1f, 0.5f, -0.25f).normalized;
            var to = from * Quaternion.AngleAxis(30f, axis);
            Vector3 omega = JointMath.BodyAngularVelocity(from, to, 0.5f);
            Vector3 expected = axis * (30f * Mathf.Deg2Rad / 0.5f);
            Assert.AreEqual(expected.x, omega.x, 1e-3f);
            Assert.AreEqual(expected.y, omega.y, 1e-3f);
            Assert.AreEqual(expected.z, omega.z, 1e-3f);
        }
    }

    [TestFixture]
    public class GetUpTests
    {
        private const float Dt = 1f / 60f;

        private static UnitState KnockedDownAndGettingUp(UnitProfile profile)
        {
            var state = new UnitState(profile);
            state.LoseStability(1000f);
            Assert.AreEqual(UnitStateKind.Knockdown, state.Kind);
            var lying = Sense(0.25f, 95f);
            for (int i = 0; i < 600 && state.Kind == UnitStateKind.Knockdown; i++)
            {
                state.Step(Dt, lying, false);
            }

            Assert.AreEqual(UnitStateKind.GettingUp, state.Kind);
            return state;
        }

        private static BodySense Sense(float pelvis, float tilt) => new BodySense
        {
            pelvisHeight = pelvis,
            restPelvisHeight = 0.92f,
            tiltDegrees = tilt,
            pelvisSpeed = 0f,
            grounded = true,
        };

        [Test]
        public void StillHangingFaceDownIsNotStanding()
        {
            var profile = UnitProfile.CreateDefault();
            var state = KnockedDownAndGettingUp(profile);
            var hanging = Sense(0.9f, 100f);   // what the first playtest did: pelvis up, chest face down
            for (float t = 0f; t < profile.getUpDuration + 0.2f; t += Dt)
            {
                state.Step(Dt, hanging, true);
            }

            Assert.AreEqual(UnitStateKind.GettingUp, state.Kind, "must not be declared standing while tilted 100°");
        }

        [Test]
        public void UprightAfterTheGetUpTimeIsStanding()
        {
            var profile = UnitProfile.CreateDefault();
            var state = KnockedDownAndGettingUp(profile);
            var upright = Sense(0.9f, 8f);
            for (float t = 0f; t < profile.getUpDuration + 0.1f && state.Kind == UnitStateKind.GettingUp; t += Dt)
            {
                state.Step(Dt, upright, true);
            }

            Assert.AreEqual(UnitStateKind.Steady, state.Kind);
        }

        [Test]
        public void NeverComingUpFailsBackToTheGround()
        {
            var profile = UnitProfile.CreateDefault();
            var state = KnockedDownAndGettingUp(profile);
            var hanging = Sense(0.5f, 80f);
            for (float t = 0f; t < profile.getUpDuration + profile.getUpGrace + 0.2f && state.Kind == UnitStateKind.GettingUp; t += Dt)
            {
                state.Step(Dt, hanging, true);
            }

            Assert.AreEqual(UnitStateKind.Knockdown, state.Kind);
            Assert.IsTrue(state.LastReason.StartsWith("get-up failed"), state.LastReason);
        }

        [Test]
        public void AssistIsAlreadyHelpingAtTheStartOfTheGetUp()
        {
            var profile = UnitProfile.CreateDefault();
            var state = KnockedDownAndGettingUp(profile);
            Assert.Greater(state.Balance, 0.5f, "the lift starts straight away, not from zero");
        }
    }
}
