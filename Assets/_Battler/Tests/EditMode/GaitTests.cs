using System.Collections.Generic;
using NUnit.Framework;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Tests
{
    /// <summary>
    /// The captured walks: a unit walked by the game steps to its style's walk cycle (the feet
    /// planted while they stand, the cadence the capture's), its hips go along with the root, and
    /// when it stops the walk fades and the stance takes the feet home.
    /// </summary>
    [TestFixture]
    public class GaitTests
    {
        [Test]
        public void EveryBodyWalksToItsCapturedCycle()
        {
            var report = new System.Text.StringBuilder();
            bool failed = false;
            foreach (string body in new[] { "Large", "Medium", "Small" })
            {
                var (unit, opponent) = RhythmTests.Pair(body);
                opponent.Place(Vector3.forward * 30f, 180f);
                unit.Opponent = null;
                unit.Rhythm.Auto = false;
                var both = new[] { unit, opponent };
                try
                {
                    Assert.IsFalse(unit.Style.walk.IsEmpty, $"{body} has a captured walk");
                    unit.FacePoint(unit.Root.position + Vector3.forward * 100f);   // (the way it walks)
                    RhythmTests.Step(both, 40);
                    float leg = unit.Body.LegLength, speed = unit.Style.walkSpeed * unit.Body.SizeFactor;
                    int steps0 = unit.Stance.StepsTaken;
                    float slide = 0f, ahead = 0f, cycle = 0f;
                    var planted = new Vector3[2];
                    var wasPlanted = new bool[2];
                    for (int i = 0; i < 240; i++)
                    {
                        unit.Root.position += unit.Root.forward * (speed / 60f);
                        RhythmTests.Step(both);
                        if (i < 60)
                        {
                            continue;   // (getting going)
                        }

                        cycle = unit.Gait.CycleTime;
                        for (int f = 0; f < 2; f++)
                        {
                            bool p = !unit.Stance.Stepping(f);
                            if (p && wasPlanted[f])
                            {
                                slide = Mathf.Max(slide, Vector3.Distance(planted[f], unit.Stance.Position(f)));
                            }

                            if (p && !wasPlanted[f])
                            {
                                planted[f] = unit.Stance.Position(f);
                            }

                            wasPlanted[f] = p;
                        }

                        Vector3 hips = unit.Anim.Part(PartId.Pelvis).position - unit.Root.position;
                        hips.y = 0f;
                        ahead = Mathf.Max(ahead, hips.magnitude / leg);
                    }

                    float weight = unit.Gait.Weight;
                    int steps = unit.Stance.StepsTaken - steps0;
                    float expected = 2f * 4f / cycle;
                    // Stop: the walk fades and the feet go home.
                    RhythmTests.Step(both, 120);
                    float home = 0f;
                    for (int f = 0; f < 2; f++)
                    {
                        home = Mathf.Max(home, Vector3.Distance(unit.Stance.Position(f), unit.Stance.HomeWorld(f, unit.Root.position, unit.FacingYaw)) / leg);
                    }

                    bool bad = weight < 0.95f || Mathf.Abs(steps - expected) > 2.5f || slide > 0.005f || ahead > 0.35f || unit.Gait.Weight > 0.01f || home > 0.12f;
                    failed |= bad;
                    report.AppendLine($"{body}: walk {weight:0.00}, {steps} steps in 4 s (the cycle's {expected:0.0}), planted feet slid {slide * 100f:0.0} cm, "
                                      + $"hips at most {ahead:0.00} legs from the root, stopped: walk {unit.Gait.Weight:0.00}, feet {home:0.00} legs from home{(bad ? "  <-- out of bounds" : "")}");
                }
                finally
                {
                    RhythmTests.Destroy(both);
                }
            }

            System.Console.WriteLine(report.ToString());
            Assert.IsFalse(failed, report.ToString());
        }

        [Test]
        public void ABlowStopsTheWalk()
        {
            // The walk only plays on its feet with nothing else to do: a blow takes the feet back.
            var (unit, opponent) = RhythmTests.Pair("Medium");
            var both = new[] { unit, opponent };
            try
            {
                RhythmTests.Step(both, 30);
                float speed = unit.Style.walkSpeed * unit.Body.SizeFactor;
                for (int i = 0; i < 40; i++)
                {
                    unit.Root.position += unit.Root.forward * (speed / 60f);
                    RhythmTests.Step(both);
                }

                Assert.Greater(unit.Gait.Weight, 0.5f, "walking");
                var state = Combat.MovePlanner.Prepare(unit, opponent, unit.Loadout.Main.moves[0], Combat.AimPart.Chest);
                Assert.IsNotNull(state, "a blow to start");
                Assert.IsTrue(unit.StartMove(state));
                RhythmTests.Step(both, 20);
                Assert.Less(unit.Gait.Weight, 0.05f, "the walk gave way to the blow");
            }
            finally
            {
                RhythmTests.Destroy(both);
            }
        }
    }
}
