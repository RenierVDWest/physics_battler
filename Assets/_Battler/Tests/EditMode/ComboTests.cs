using System.Collections.Generic;
using NUnit.Framework;
using PhysicsBattler.Combat;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Tests
{
    /// <summary>
    /// Milestone 10: combos. Each link after the first starts at the last one's link-out, part way
    /// into its own wind-up, so the links come about half a second apart; the pose blends from link
    /// to link without a pop; the lunge is banked so the body doesn't snap back; a link that no
    /// longer reaches ends the combo; and every body has its strings.
    /// </summary>
    [TestFixture]
    public class ComboTests
    {
        private const float Dt = 1f / 60f;

        internal sealed class Played
        {
            public readonly List<(float time, float startedAt, MoveState state)> starts = new List<(float, float, MoveState)>();
            public readonly List<string> changes = new List<string>();
            public float pop, fastest, pelvisJump, popAfterStop;
            public string popWhere = "", fastWhere = "";
        }

        /// <summary>Plays a combo at a standing opponent to its end (at most 6 s) and records its links and how the pose moved.</summary>
        internal static Played Play(string body, string comboName, out UnitController unit, out UnitController enemy, System.Action<UnitController, UnitController, int> each = null)
        {
            UnitController.ResetSeeds(3);
            var pair = RhythmTests.Pair(body);
            unit = pair.unit;
            enemy = pair.opponent;
            var both = new[] { unit, enemy };
            RhythmTests.Step(both, 60);
            var played = new Played();
            var me = unit;
            void OnStart(UnitController u, MoveState s)
            {
                if (u == me)
                {
                    played.starts.Add((me.LocalTime, s.T, s));
                }
            }

            void OnChange(UnitController u, ComboRun c, string what)
            {
                if (u == me)
                {
                    played.changes.Add(what);
                }
            }

            UnitController.MoveStarted += OnStart;
            UnitController.ComboChanged += OnChange;
            try
            {
                var combo = System.Array.Find(ComboDefinition.All, c => c.name == comboName);
                Assert.IsNotNull(combo, comboName);
                var first = unit.PlanLink(combo, 0, enemy, new System.Random(1));
                Assert.IsNotNull(first, $"{comboName}: its first link reaches");
                Assert.IsTrue(unit.StartCombo(combo, combo.links.Length, enemy, first), "it starts");
                var last = new Vector3[PartInfo.Count];
                var lastStep = new float[PartInfo.Count];
                int seen = 0;
                for (int i = 0; i < 6 * 60 && (unit.Combo != null || unit.CurrentMove != null); i++)
                {
                    each?.Invoke(unit, enemy, i);
                    RhythmTests.Step(both);
                    foreach (var id in PartInfo.All)
                    {
                        Vector3 p = unit.Anim.Part(id).position;
                        if (seen > 0)
                        {
                            float step = Vector3.Distance(p, last[(int)id]);
                            // (At the hand-overs: from a step before a link starts to its blend's end. A
                            // stopped blow's rebound is a single blow's, measured elsewhere.)
                            float sinceLink = unit.LocalTime - played.starts[played.starts.Count - 1].time;
                            bool atLink = played.starts.Count > 1 && sinceLink > 1e-4f && sinceLink <= UnitController.LinkBlendTime + 0.05f;
                            // (Not the shield arm: its guard jumps as a single blow stops too, a thing of its own.)
                            bool shieldArm = unit.Loadout.HasShield && (id == PartId.UpperArmL || id == PartId.ForearmL || id == PartId.HandL);
                            atLink &= !shieldArm;
                            // (A link after a stopped one starts from the rebound, near still: measured on its own.)
                            bool afterStop = played.changes.Count > 0 && played.changes[played.changes.Count - 1].EndsWith("after a stop");
                            if (atLink && afterStop && seen > 1)
                            {
                                played.popAfterStop = Mathf.Max(played.popAfterStop, step - 2f * lastStep[(int)id]);
                            }

                            atLink &= !afterStop;
                            if (atLink && step > played.fastest)
                            {
                                played.fastest = step;
                                played.fastWhere = $"{id} at {unit.LocalTime:0.00} s";
                            }

                            if (atLink && seen > 1 && step - 2f * lastStep[(int)id] > played.pop)
                            {
                                played.pop = step - 2f * lastStep[(int)id];
                                played.popWhere = $"{id} at {unit.LocalTime:0.00} s";
                            }

                            lastStep[(int)id] = step;
                        }

                        last[(int)id] = p;
                    }

                    seen++;
                }
            }
            finally
            {
                UnitController.MoveStarted -= OnStart;
                UnitController.ComboChanged -= OnChange;
            }

            return played;
        }

        [Test]
        public void EachLinkStartsAtTheLastOnesLinkOutSkippingItsWindUp()
        {
            foreach (var (body, name) in new[] { ("Medium", "double thrust"), ("Large", "body then head"), ("Small", "rompida") })
            {
                var played = Play(body, name, out var unit, out var enemy);
                try
                {
                    string what = $"{body} {name} ({string.Join("; ", played.changes)})";
                    Assert.GreaterOrEqual(played.starts.Count, 2, $"{what}: at least two links");
                    for (int i = 1; i < played.starts.Count; i++)
                    {
                        var (time, startedAt, state) = played.starts[i];
                        Assert.AreEqual(ComboDefinition.LinkIn(state.Move), startedAt, 0.01f, $"{what}: link {i + 1} starts at its link-in, most of its wind-up skipped");
                        float gap = time - played.starts[i - 1].time;
                        Assert.GreaterOrEqual(gap, 0.3f, $"{what}: link {i + 1} comes {gap:0.00} s after the one before");
                        Assert.LessOrEqual(gap, 1.0f, $"{what}: link {i + 1} comes {gap:0.00} s after the one before");
                    }

                    // (A single blow's own start moves the weapon hand about 4 cm more in a step than the
                    // step before, and its stop about 16: a link hands over no harder than a blow starts.)
                    Assert.Less(played.pop, 0.05f * Mathf.Max(1f, unit.Body.SizeFactor), $"{what}: no part of the pose pops at a link ({played.pop * 100f:0.0} cm: {played.popWhere})");
                    Assert.Less(played.popAfterStop, 0.12f * Mathf.Max(1f, unit.Body.SizeFactor), $"{what}: nor at a link after a stopped one ({played.popAfterStop * 100f:0.0} cm)");
                    Assert.Less(played.fastest, 0.3f * Mathf.Max(1f, unit.Body.SizeFactor), $"{what}: or flies at a link ({played.fastest * 100f:0} cm in a step: {played.fastWhere})");
                    Assert.IsNull(unit.Combo, $"{what}: it ends");
                    Assert.IsTrue(played.changes[played.changes.Count - 1].StartsWith("ended"), what);
                }
                finally
                {
                    RhythmTests.Destroy(unit, enemy);
                }
            }
        }

        [Test]
        public void ALinkThatNoLongerReachesEndsTheCombo()
        {
            var played = Play("Medium", "double thrust", out var unit, out var enemy, (u, e, i) =>
            {
                // (The target steps well away once the first link is under way.)
                if (i == 10)
                {
                    e.Place(e.transform.position + (e.transform.position - u.transform.position).normalized * 3f, e.FacingYaw);
                }
            });
            try
            {
                Assert.AreEqual(1, played.starts.Count, $"one link only ({string.Join("; ", played.changes)})");
                Assert.IsTrue(played.changes.Exists(c => c.Contains("doesn't reach")), string.Join("; ", played.changes));
            }
            finally
            {
                RhythmTests.Destroy(unit, enemy);
            }
        }

        [Test]
        public void EveryBodyHasItsStrings()
        {
            foreach (var body in new[] { "Small", "Medium", "Large" })
            {
                var unit = RhythmTests.Build(body, Vector3.zero, 0f, 0);
                try
                {
                    var strings = ComboDefinition.For(unit.Loadout.Main, unit.Body.SizeFactor);
                    Assert.GreaterOrEqual(strings.Count, 3, $"{body}: three strings from its weapon's moves");
                    Assert.IsTrue(strings.Exists(c => c.finisher), $"{body}: one with a finisher");
                }
                finally
                {
                    RhythmTests.Destroy(unit);
                }
            }
        }

        [Test]
        public void TheLinkTimesAreWhereThePlanPutsThem()
        {
            var move = MoveDefinition.Create(MoveStyle.ClubSwing);
            float contact = move.ContactTime();
            Assert.Less(ComboDefinition.LinkIn(move), move.WindUpEnd, "link-in is inside the wind-up");
            Assert.Greater(ComboDefinition.LinkOut(move, contact), contact, "link-out is after contact");
            Assert.Less(ComboDefinition.LinkOut(move, contact), move.SwingFollowEnd, "and before the recovery");
        }
    }
}
