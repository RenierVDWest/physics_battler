using NUnit.Framework;
using PhysicsBattler.Motion;
using UnityEngine;

namespace PhysicsBattler.Tests
{
    [TestFixture]
    public class CarryPoseTests
    {
        // The rig's rest pose is identity rotations: arm hanging, the weapon slot's +Z (the club) pointing
        // forward, square to the forearm. The idle upper arm is pitched a little and spread a little.
        private static readonly Quaternion IdleUpperArm = Quaternion.Euler(-6f, 0f, 6f);

        private static Vector3 ClubDirection()
        {
            return IdleUpperArm * PoseComposer.CarryForearm * PoseComposer.CarryHand * Vector3.forward;
        }

        [Test]
        public void CarriedClubStandsUpOverTheShoulder()
        {
            Vector3 club = ClubDirection();
            Assert.IsTrue(club.y > 0.9f, $"club {club} should point mostly up");
            Assert.IsTrue(club.z < 0f, $"club {club} should tip back, away from the front");
        }
    }
}
