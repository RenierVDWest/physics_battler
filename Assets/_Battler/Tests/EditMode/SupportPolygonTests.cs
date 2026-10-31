using System.Collections.Generic;
using NUnit.Framework;
using PhysicsBattler.Muscles;
using UnityEngine;

namespace PhysicsBattler.Tests
{
    [TestFixture]
    public class SupportPolygonTests
    {
        // A fighting stance: left foot 0.22 m left and 0.17 m forward, right foot 0.22 m right and 0.17 m back.
        private static List<Vector2> Stance()
        {
            var soles = new List<Vector2>();
            SupportPolygon.AddSole(soles, new Vector3(-0.22f, 0.08f, 0.17f), Quaternion.Euler(0f, -12f, 0f), 0.3f, 0.14f, 0.06f);
            SupportPolygon.AddSole(soles, new Vector3(0.22f, 0.08f, -0.17f), Quaternion.Euler(0f, 24f, 0f), 0.3f, 0.14f, 0.06f);
            var hull = new List<Vector2>();
            SupportPolygon.Hull(soles, hull);
            return hull;
        }

        [Test]
        public void BetweenTheFeetIsSupported()
        {
            var hull = Stance();
            Assert.AreEqual(0f, SupportPolygon.DistanceOutside(hull, Vector2.zero), 1e-5f);
            Assert.AreEqual(0f, SupportPolygon.DistanceOutside(hull, new Vector2(-0.1f, 0.1f)), 1e-5f, "toward the lead foot");
        }

        [Test]
        public void FallingSidewaysOrForwardIsMeasuredFromTheEdge()
        {
            var hull = Stance();
            float right = SupportPolygon.DistanceOutside(hull, new Vector2(0.6f, -0.17f));
            Assert.Greater(right, 0.2f);
            Assert.Less(right, 0.35f);
            float forward = SupportPolygon.DistanceOutside(hull, new Vector2(-0.2f, 0.8f));
            Assert.Greater(forward, 0.3f);
        }

        [Test]
        public void HullIsConvexAndCounterClockwise()
        {
            var hull = Stance();
            Assert.IsTrue(hull.Count >= 4 && hull.Count <= 8, $"{hull.Count} points");
            for (int i = 0; i < hull.Count; i++)
            {
                var a = hull[i];
                var b = hull[(i + 1) % hull.Count];
                var c = hull[(i + 2) % hull.Count];
                float cross = (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
                Assert.Greater(cross, 0f, "every turn is to the left");
            }
        }
    }
}
