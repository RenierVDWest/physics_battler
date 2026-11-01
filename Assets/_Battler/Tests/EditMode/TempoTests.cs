using System.Text;
using NUnit.Framework;
using PhysicsBattler.Combat;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Tests
{
    /// <summary>
    /// Milestone 8: the tempo. A unit gathers energy holding back, probes once rested, presses when
    /// charged and fired up, shells up under a barrage and counters when it stops; its energy comes
    /// back fastest gathering and not at all pressing.
    /// </summary>
    [TestFixture]
    public class TempoTests
    {
        private const float Dt = 1f / 60f;

        private static Temperament Plain(float holdChance = 0f) => new Temperament
        {
            probeAt = 0.45f, pressAt = 0.6f, pressAggression = 0.45f, pressEnd = 0.25f, burstMax = 4, composure = 0.5f,
            holdChance = holdChance, fullCharge = 0.9f,
        };

        /// <summary>Runs the tempo for a while with energy following its regeneration (no blows); the energy at the end.</summary>
        private static float Run(TempoDirector tempo, ref float energy, float seconds, float aggression = 0.6f, bool engaged = true, bool vulnerable = false)
        {
            for (float t = 0f; t < seconds; t += Dt)
            {
                tempo.Update(Dt, new TempoInput { energy = energy, aggression = aggression, canAct = true, engaged = engaged, targetVulnerable = vulnerable });
                energy = Mathf.Min(1f, energy + tempo.Regen * Dt);
            }

            return energy;
        }

        private static TempoDirector Fresh(Temperament t)
        {
            var tempo = new TempoDirector(t, 3);
            tempo.Force(TempoState.Gather);   // (a gather that doesn't hold on: the temperament's chance is 0)
            return tempo;
        }

        [Test]
        public void ItGathersProbesOnceRestedThenPressesWhenCharged()
        {
            var tempo = Fresh(Plain());
            float energy = 0.25f;
            Run(tempo, ref energy, 0.5f);
            Assert.AreEqual(TempoState.Gather, tempo.State, "it gathers first");
            Assert.AreEqual(tempo.Temperament.gatherRegen, tempo.Regen, 1e-4f, "fast");
            Run(tempo, ref energy, 1.5f);
            Assert.AreEqual(TempoState.Probe, tempo.State, $"rested: it probes (energy {energy:0.00})");
            Run(tempo, ref energy, 3f);
            Assert.AreEqual(TempoState.Press, tempo.State, $"charged and fired up: it presses (energy {energy:0.00})");
            Assert.AreEqual(0f, tempo.Regen, "no energy back pressing");
        }

        [Test]
        public void ALongGatherExplodesIntoAPress()
        {
            var tempo = Fresh(Plain(holdChance: 1f));
            float energy = 0.25f;
            Run(tempo, ref energy, 2f);
            Assert.AreEqual(TempoState.Gather, tempo.State, $"it holds on past its probe level (energy {energy:0.00})");
            Run(tempo, ref energy, 3f);
            Assert.AreEqual(TempoState.Press, tempo.State, $"and goes straight into a press, fully charged (energy {energy:0.00})");
        }

        [Test]
        public void ItOnlyGoesOnOnceItIsClose()
        {
            var tempo = Fresh(Plain());
            float energy = 1f;
            Run(tempo, ref energy, 3f, engaged: false);
            Assert.AreEqual(TempoState.Gather, tempo.State, "walking in from afar, it keeps gathering");
        }

        [Test]
        public void APressEndsSpentOrAfterItsBurst()
        {
            var tempo = Fresh(Plain());
            tempo.Force(TempoState.Press);
            float energy = 0.9f;
            for (int i = 0; i < 4; i++)
            {
                tempo.NoteBlowStarted();
                energy -= 0.1f;
                Run(tempo, ref energy, 0.1f);
            }

            Assert.AreEqual(TempoState.Gather, tempo.State, "four blows: the burst is done");

            tempo.Force(TempoState.Press);
            energy = 0.3f;
            tempo.NoteBlowStarted();
            energy -= 0.12f;
            Run(tempo, ref energy, 0.1f);
            Assert.AreEqual(TempoState.Gather, tempo.State, "spent: back to gathering");
        }

        [Test]
        public void TwoBlockedBlowsOrAHeavyHitBreakAPress()
        {
            var tempo = Fresh(Plain());
            tempo.Force(TempoState.Press);
            float energy = 0.9f;
            tempo.NoteBlocked();
            Run(tempo, ref energy, 0.1f);
            Assert.AreEqual(TempoState.Press, tempo.State, "one block doesn't");
            tempo.NoteBlocked();
            Run(tempo, ref energy, 0.1f);
            Assert.AreEqual(TempoState.Gather, tempo.State, "two do");

            tempo.Force(TempoState.Press);
            tempo.NoteHit(0.5f);
            Run(tempo, ref energy, 0.1f);
            Assert.AreNotEqual(TempoState.Press, tempo.State, "a heavy hit does too");
        }

        [Test]
        public void ABarrageShellsItUpAndItCountersWhenItStops()
        {
            var tempo = Fresh(Plain());
            tempo.Force(TempoState.Probe);
            float energy = 0.6f;
            tempo.NoteIncoming(true);
            Run(tempo, ref energy, 0.1f, aggression: 0.3f);
            Assert.AreEqual(TempoState.Probe, tempo.State, "one blow at it doesn't");
            tempo.NoteIncoming(true);
            Run(tempo, ref energy, 0.1f, aggression: 0.3f);
            Assert.AreEqual(TempoState.Shell, tempo.State, $"a second from a presser does (pressure {tempo.Pressure:0.00})");
            Assert.AreEqual(TempoUse.Shell, tempo.Use, "only shell postures now");
            Assert.IsFalse(tempo.Strikes, "and no blows");
            Run(tempo, ref energy, 1.4f, aggression: 0.3f);
            Assert.AreEqual(TempoState.Shell, tempo.State, "it stays shelled up between a press's blows");
            Run(tempo, ref energy, 1.6f, aggression: 0.3f);
            Assert.AreEqual(TempoState.Counter, tempo.State, $"the barrage stopped: it answers (pressure {tempo.Pressure:0.00})");
            Assert.AreEqual(0f, tempo.BlowDelay(0.5f), "at once");
            tempo.NoteBlowStarted();
            Run(tempo, ref energy, 0.1f, aggression: 0.3f);
            Assert.AreEqual(TempoState.Probe, tempo.State, "answered, and not fired up: it probes");
        }

        [Test]
        public void PressingItTakesMoreBeforeItShells()
        {
            var tempo = Fresh(Plain());
            tempo.Force(TempoState.Press);
            float energy = 0.9f;
            tempo.NoteIncoming(true);
            tempo.NoteIncoming(false);
            Run(tempo, ref energy, 0.1f);
            Assert.AreEqual(TempoState.Press, tempo.State, $"0.55 pressure doesn't shell a presser (pressure {tempo.Pressure:0.00})");
            tempo.NoteIncoming(true);
            Run(tempo, ref energy, 0.1f);
            Assert.AreEqual(TempoState.Shell, tempo.State, $"0.95 does (pressure {tempo.Pressure:0.00})");
        }

        /// <summary>Every style's stance in every tempo state, held: the weight stays over the feet, arms and weapon out of the body, no pops.</summary>
        [Test]
        public void EveryStanceStaysBalancedAndClearOfTheBody()
        {
            var report = new StringBuilder();
            bool failed = false;
            foreach (string body in new[] { "Small", "Medium", "Large" })
            {
                var (unit, opponent) = RhythmTests.Pair(body);
                unit.TempoOn = true;
                unit.TempoEngaged = true;
                var both = new[] { unit, opponent };
                try
                {
                    RhythmTests.Step(both, 30);
                    float leg = unit.Body.LegLength, size = unit.Body.SizeFactor;
                    float weaponLength = unit.Loadout.Main.length * 0.98f;
                    foreach (TempoState state in System.Enum.GetValues(typeof(TempoState)))
                    {
                        float com = 0f, clear = float.MaxValue, pop = 0f;
                        string clearWhat = "";
                        var last = new Vector3[PartInfo.Count];
                        var lastStep = new float[PartInfo.Count];
                        unit.Rhythm.EnergyOverride = 1f;
                        for (int i = 0; i < 150; i++)
                        {
                            if (unit.Tempo.State != state)
                            {
                                unit.Tempo.Force(state);
                            }

                            RhythmTests.Step(both);
                            float off = RhythmTests.OffTheFeet(unit);
                            if (off > com && System.Environment.GetEnvironmentVariable("RHYTHM_DEBUG") != null)
                            {
                                System.Console.WriteLine($"  com {body} {state} step {i}: {off / unit.Body.LegLength * 100f:0}% (posture {unit.Rhythm.Current?.Label} {unit.Rhythm.Progress:0.00}, stepping {unit.Stance.AnyStepping}, walk {unit.Gait?.Weight:0.00})");
                            }

                            com = Mathf.Max(com, off);
                            float c = BodyClearance.Measure(unit.Rig, unit.Anim, weaponLength, unit.Loadout.Scale, out var part, out bool blade);
                            if (c < clear)
                            {
                                clear = c;
                                clearWhat = $"{(blade ? "weapon" : "arm")} on the {part}";
                            }

                            foreach (var id in PartInfo.All)
                            {
                                Vector3 p = unit.Anim.Part(id).position;
                                if (i > 0)
                                {
                                    float d = Vector3.Distance(p, last[(int)id]);
                                    if (i > 1)
                                    {
                                        if (d - 2f * lastStep[(int)id] > pop && System.Environment.GetEnvironmentVariable("RHYTHM_DEBUG") != null)
                                        {
                                            System.Console.WriteLine($"  pop {body} {state} step {i} {id}: {(d - 2f * lastStep[(int)id]) * 100f:0.0} cm (posture {unit.Rhythm.Current?.Label} {unit.Rhythm.Progress:0.00}, stepping {unit.Stance.AnyStepping})");
                                        }

                                        pop = Mathf.Max(pop, d - 2f * lastStep[(int)id]);
                                    }

                                    lastStep[(int)id] = d;
                                }

                                last[(int)id] = p;
                            }
                        }

                        bool bad = com > 0.16f * leg || clear < -(MovePlanner.OwnBodyClearance + 0.03f) * size || pop > 0.04f * size;
                        failed |= bad;
                        report.AppendLine($"{body} {state}: weight off the feet {com / leg * 100f:0}% of the leg, clearance {clear * 100f:0.0} cm ({clearWhat}), pop {pop * 100f:0.0} cm{(bad ? "  <-- out of bounds" : "")}");
                    }
                }
                finally
                {
                    RhythmTests.Destroy(both);
                }
            }

            System.Console.WriteLine(report.ToString());
            Assert.IsFalse(failed, report.ToString());
        }
    }
}
