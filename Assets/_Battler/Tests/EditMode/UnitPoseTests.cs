using System.IO;
using System.Reflection;
using NUnit.Framework;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Tests
{
    /// <summary>
    /// The pose pipeline on the real bodies (built from the rig files, without meshes): feet reach
    /// the ground where the stance put them, the pelvis is over the feet, every size fits its stance.
    /// </summary>
    [TestFixture]
    public class UnitPoseTests
    {
        private static UnitController Build(string body)
        {
            string path = Path.Combine(Application.dataPath, $"_Battler/Data/Rigs/{body.ToLowerInvariant()}_rig.json");
            var rig = RigFile.Parse(File.ReadAllText(path));
            var profile = UnitProfile.CreateDefault();
            profile.body = body;
            var root = UnitBuilder.Build(rig, profile, "Test " + body, _ => null, 0);
            var unit = root.GetComponent<UnitController>();
            unit.Profile = profile;
            // Edit mode: nothing calls Awake by itself.
            typeof(UnitController).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(unit, null);
            return unit;
        }

        private static void Destroy(UnitController unit)
        {
            if (unit != null)
            {
                Object.DestroyImmediate(unit.gameObject);
            }
        }

        [Test] public void SmallFeetReachTheirSpots() => FeetReachTheirSpots("Small");
        [Test] public void MediumFeetReachTheirSpots() => FeetReachTheirSpots("Medium");
        [Test] public void LargeFeetReachTheirSpots() => FeetReachTheirSpots("Large");

        private static void FeetReachTheirSpots(string body)
        {
            var unit = Build(body);
            try
            {
                for (int i = 0; i < 90; i++)
                {
                    unit.Pose.Compose(i / 60f, 1f / 60f);
                    for (int f = 0; f < 2; f++)
                    {
                        var ankle = unit.Anim.Part(f == Stance.Left ? PartId.FootL : PartId.FootR).position;
                        float miss = Vector3.Distance(ankle, unit.Pose.AnkleTarget(f));
                        Assert.Less(miss, 0.01f * unit.Body.SizeFactor + 0.002f, $"{body} foot {f} at t={i / 60f:0.00}");
                    }
                }
            }
            finally
            {
                Destroy(unit);
            }
        }

        [Test] public void SmallBalanceIsOverTheFeet() => BalanceIsOverTheFeet("Small");
        [Test] public void MediumBalanceIsOverTheFeet() => BalanceIsOverTheFeet("Medium");
        [Test] public void LargeBalanceIsOverTheFeet() => BalanceIsOverTheFeet("Large");

        private static void BalanceIsOverTheFeet(string body)
        {
            var unit = Build(body);
            try
            {
                for (int i = 0; i < 120; i++)
                {
                    unit.Pose.Compose(i / 60f, 1f / 60f);
                    var com = unit.Pose.CenterOfMass;
                    var support = unit.Pose.SupportPoint;
                    var offset = new Vector2(com.x - support.x, com.z - support.z).magnitude;
                    // (6.5% of the leg: the idle's sway is seeded per unit and some seeds lean a little further.)
                    Assert.Less(offset, 0.065f * unit.Body.LegLength, $"{body} t={i / 60f:0.00}");
                    Assert.Less(unit.Pose.PelvisDrop, 0.03f * unit.Body.LegLength, "the stance fits without dropping the hips");
                }
            }
            finally
            {
                Destroy(unit);
            }
        }

        [Test]
        public void AuthorityIsAppliedFromTheFirstStep()
        {
            var unit = Build("Medium");
            try
            {
                unit.Pose.Compose(0f, 1f / 60f);
                unit.Authority.Apply(1f / 60f);
                foreach (var id in PartInfo.All)
                {
                    bool legs = Authority.AuthorityController.RegionOf(id) == Authority.BodyRegion.Legs;
                    Assert.AreEqual(legs, unit.Rig.Part(id).body.isKinematic, $"{id}");
                }
            }
            finally
            {
                Destroy(unit);
            }
        }

        [Test]
        public void SizesScale()
        {
            var small = Build("Small");
            var large = Build("Large");
            try
            {
                Assert.Less(small.Body.LegLength, 0.6f * large.Body.LegLength);
                Assert.Greater(large.Body.Mass, 5f * small.Body.Mass);
            }
            finally
            {
                Destroy(small);
                Destroy(large);
            }
        }
    }
}
