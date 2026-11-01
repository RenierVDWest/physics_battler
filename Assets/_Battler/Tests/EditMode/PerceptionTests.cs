using System.Collections.Generic;
using NUnit.Framework;
using PhysicsBattler.Combat;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Tests
{
    /// <summary>
    /// Milestone 9a: a unit sees from its head and knows only what it has seen, felt or remembers;
    /// its head looks at what holds its attention, within a head's limits, at a person's pace; its
    /// shield points at the threat it knows of, stays in the cone a shield arm covers and out of its
    /// own body, and is raised only against blows it has seen.
    /// </summary>
    [TestFixture]
    public class PerceptionTests
    {
        private const float Dt = 1f / 60f;

        // ---- What a head sees --------------------------------------------------------------------

        [Test]
        public void AHeadSeesClearlyAheadMovementAtTheEdgeAndFeelsWhatIsBesideIt()
        {
            var skill = PerceptionSkill.Default;
            Vector3 eye = Vector3.up * 1.6f;
            var head = Quaternion.identity;
            Sight At(float yaw, float distance) => Perception.Classify(eye, head, eye + Quaternion.Euler(0f, yaw, 0f) * Vector3.forward * distance, 0.9f, skill);
            Assert.AreEqual(Sight.Clear, At(0f, 2f), "straight ahead");
            Assert.AreEqual(Sight.Clear, At(25f, 2f), "within 30 degrees");
            Assert.AreEqual(Sight.Edge, At(60f, 2f), "out to the side: the edge of its view");
            Assert.AreEqual(Sight.Edge, At(-95f, 2f), "just ahead of its shoulder");
            Assert.AreEqual(Sight.Unseen, At(130f, 2f), "behind it");
            Assert.AreEqual(Sight.Unseen, At(180f, 2f), "right behind it");
            Assert.AreEqual(Sight.Felt, At(180f, 0.6f), "right behind it but close: felt");
            Assert.AreEqual(Sight.Unseen, Perception.Classify(eye, head, eye + Vector3.up * 2f, 0.9f, skill), "straight overhead, out of reach");
            Assert.IsFalse(Perception.CanReact(Sight.Felt), "a touch comes too late to react to a blow");
            Assert.AreEqual(1.5f, Perception.ReactionScale(Sight.Edge, skill), 1e-4f, "slower at the edge");
        }

        [Test]
        public void ItRemembersWhatItNoLongerSeesForAWhileThenForgets()
        {
            var known = new Knowledge<string>();
            known.BeginLook(0f);
            Assert.IsTrue(known.Observe("enemy", Vector3.zero, Sight.Clear, 0f), "noticed");
            known.BeginLook(0.5f);
            Assert.IsTrue(known.Observe("enemy", new Vector3(0.5f, 0f, 0f), Sight.Clear, 0.5f) == false, "(not noticed again)");
            Assert.AreEqual(Sight.Clear, known.Current("enemy"), "seen now");
            known.BeginLook(1f);
            Assert.AreEqual(Sight.Unseen, known.Current("enemy"), "out of view now");
            Assert.IsTrue(known.Knows("enemy"), "but remembered");
            Vector3 guess = known.Estimate("enemy", 1f);
            Assert.Greater(guess.x, 0.5f, "carried on along its last motion");
            known.Forget(3.4f, 3f);
            Assert.IsTrue(known.Knows("enemy"), "2.9 s out of view: still remembered");
            known.Forget(3.6f, 3f);
            Assert.IsFalse(known.Knows("enemy"), "3.1 s: forgotten");
        }

        // ---- Where the head looks ------------------------------------------------------------------

        private static LookPoint Point(int id, float score, float age = 10f, float yaw = 0f) => new LookPoint
        {
            id = id, score = score, age = age, kind = "test", point = Vector3.up * 1.6f + Quaternion.Euler(0f, yaw, 0f) * Vector3.forward * 2f,
        };

        [Test]
        public void TheHeadHoldsWhatItWatchesUnlessSomethingClearlyMatters()
        {
            var look = new HeadLook { Latency = 0.15f };
            look.Choose(new List<LookPoint> { Point(1, 1f) }, 0f);
            Assert.AreEqual(1, look.Current);
            look.Choose(new List<LookPoint> { Point(1, 1f), Point(2, 1.1f) }, 1f);
            Assert.AreEqual(1, look.Current, "a little better isn't enough");
            look.Choose(new List<LookPoint> { Point(1, 1f), Point(2, 1.4f) }, 1f);
            Assert.AreEqual(2, look.Current, "clearly better takes over");
            look.Choose(new List<LookPoint> { Point(1, 1.6f), Point(2, 1.4f) }, 1.2f);
            Assert.AreEqual(2, look.Current, "held at least 0.4 s");
            look.Choose(new List<LookPoint> { Point(1, 1.6f), Point(2, 1.4f), Point(3, 5f, 0.05f) }, 1.3f);
            Assert.AreEqual(2, look.Current, "a new stimulus isn't answered before the latency");
            look.Choose(new List<LookPoint> { Point(1, 1.6f), Point(2, 1.4f), Point(3, 5f, 0.2f) }, 1.35f);
            Assert.AreEqual(3, look.Current, "a much stronger one cuts in");
        }

        [Test]
        public void TheLookTurnsAtAPersonsPaceAndOnlyAsFarAsAHeadCan()
        {
            var look = new HeadLook();
            Vector3 eye = Vector3.up * 1.6f;
            look.Choose(new List<LookPoint> { Point(1, 1f, 10f, 90f) }, 0f);
            float last = 0f, fastest = 0f, at015 = 0f, at05 = 0f;
            for (int i = 1; i <= 60; i++)
            {
                look.Follow(eye, 0f, Dt);
                fastest = Mathf.Max(fastest, (look.Yaw - last) / Dt);
                last = look.Yaw;
                if (i == 9) at015 = look.Yaw;
                if (i == 30) at05 = look.Yaw;
            }

            Assert.Less(at015, 70f, $"not at once ({at015:0} degrees after 0.15 s)");
            Assert.Greater(at05, 81f, $"a big look in under half a second ({at05:0} degrees after 0.5 s)");
            Assert.LessOrEqual(fastest, HeadLook.MaxSpeed + 1f, "never faster than a neck turns");

            look.Choose(new List<LookPoint> { Point(2, 5f, 10f, 170f) }, 1f);
            for (int i = 0; i < 120; i++)
            {
                look.Follow(eye, 0f, Dt);
            }

            Assert.LessOrEqual(look.Yaw, HeadLook.Turn + HeadLook.ChestTurn + 0.01f, "behind it: as far round as the head and chest go");
            Assert.IsFalse(look.Reachable);
        }

        // ---- Units ---------------------------------------------------------------------------------

        /// <summary>A medium sword-and-shield unit at the origin facing +Z, busy with an enemy in front of it (which doesn't strike), its shield given a face to strike.</summary>
        private static (UnitController unit, UnitController busyWith) Defender()
        {
            UnitController.ResetSeeds(1);
            var unit = RhythmTests.Build("Medium", Vector3.zero, 0f, 0);
            var face = unit.OffWeapon.gameObject.AddComponent<BoxCollider>();
            face.center = unit.Loadout.Off.faceCenter;
            face.size = new Vector3(2f * unit.Loadout.Off.radius, 2f * unit.Loadout.Off.radius, 0.04f);
            unit.Style.blockChance = 1f;
            var busyWith = RhythmTests.Build("Medium", Vector3.forward * 2f, 180f, 1);
            busyWith.RhythmOn = false;
            unit.Opponent = busyWith;
            return (unit, busyWith);
        }

        /// <summary>A swordsman <paramref name="angle"/> degrees round from the defender's front (clockwise), at striking distance, facing it.</summary>
        private static UnitController Attacker(UnitController defender, float angle)
        {
            var attacker = RhythmTests.Build("Medium", Vector3.forward * 5f, 0f, 1);
            attacker.Equip(RhythmTests.Sword(), null);
            attacker.RhythmOn = false;
            float distance = attacker.Style.engageDistance * attacker.Body.SizeFactor + 0.25f * defender.Body.SizeFactor;
            attacker.Place(Quaternion.Euler(0f, angle, 0f) * Vector3.forward * distance, angle + 180f);
            return attacker;
        }

        /// <summary>Plays one blow; what it struck first (null: nothing), and what the defender did.</summary>
        private static HitReport Blow(UnitController attacker, UnitController defender, int move, UnitController[] everyone, out string defended)
        {
            HitReport first = null;
            string did = "";
            void OnHit(HitReport h)
            {
                if (h.Attacker == attacker && first == null) first = h;
            }

            void OnDefending(UnitController u, UnitController by, MoveState m, string what)
            {
                if (u == defender && by == attacker && (did == "" || !what.StartsWith("never saw it"))) did = what;
            }

            UnitController.Hit += OnHit;
            UnitController.Defending += OnDefending;
            try
            {
                var state = MovePlanner.Prepare(attacker, defender, attacker.Loadout.Main.moves[move], AimPart.Chest);
                Assert.IsNotNull(state, "a blow can be thrown from here");
                Assert.IsTrue(attacker.StartMove(state));
                for (int i = 0; i < 400 && attacker.IsAttacking; i++)
                {
                    RhythmTests.Step(everyone);
                }

                RhythmTests.Step(everyone, 2);
            }
            finally
            {
                UnitController.Hit -= OnHit;
                UnitController.Defending -= OnDefending;
            }

            defended = did;
            return first;
        }

        [Test]
        public void ABlowFromBehindItHasNotSeenGoesUnblocked()
        {
            var (unit, busyWith) = Defender();
            var attacker = Attacker(unit, 180f);
            var everyone = new[] { unit, busyWith, attacker };
            try
            {
                RhythmTests.Step(everyone, 40);
                Assert.AreEqual(Sight.Unseen, unit.SightOf(attacker), "it is looking at the one in front of it");
                Assert.IsFalse(unit.Knows(attacker), "and doesn't know the one behind it is there");
                var hit = Blow(attacker, unit, 0, everyone, out string did);
                Assert.IsTrue(did.StartsWith("never saw it"), $"it never saw the blow (it: {did})");
                Assert.IsTrue(hit == null || !hit.Shield, "the shield doesn't take it");
                Assert.IsTrue(unit.Knows(attacker) || hit == null, "struck from behind, it knows who did it");
            }
            finally
            {
                RhythmTests.Destroy(everyone);
            }
        }

        [Test]
        public void ABlowItSeesComingFromTheFrontIsTakenOnTheShield()
        {
            var (unit, busyWith) = Defender();
            busyWith.Place(new Vector3(-2f, 0f, 0f), 90f);   // (out of the way to its left)
            var attacker = Attacker(unit, 10f);
            var everyone = new[] { unit, busyWith, attacker };
            try
            {
                unit.Opponent = attacker;
                RhythmTests.Step(everyone, 120);
                Assert.AreEqual(Sight.Clear, unit.SightOf(attacker));
                var hit = Blow(attacker, unit, 0, everyone, out string did);
                Assert.IsTrue(did.StartsWith("raises the shield") && did.Contains("seen clearly"), did);
                Assert.IsNotNull(hit, "the blow strikes something");
                Assert.IsTrue(hit.Shield, $"the shield takes it (struck the {hit.Part})");
            }
            finally
            {
                RhythmTests.Destroy(everyone);
            }
        }

        /// <summary>
        /// Blows from its weapon side (right of it, further across than a shield arm reaches): it sees
        /// them, and where one comes in from outside the shield's cone it turns to face it first. The
        /// shield takes every one.
        /// </summary>
        [Test]
        public void BlowsSeenOnTheWeaponSideAreFacedAndTakenOnTheShield()
        {
            float mostTurned = 0f;
            var report = new System.Text.StringBuilder();
            for (int move = 0; move < 3; move++)
            {
                var (unit, busyWith) = Defender();
                var attacker = Attacker(unit, 90f);
                var everyone = new[] { unit, busyWith, attacker };
                try
                {
                    RhythmTests.Step(everyone, 120);
                    Assert.IsTrue(unit.SightOf(attacker) >= Sight.Edge, "it sees it, at least at the edge of its view");
                    Assert.IsFalse(ShieldCone.Contains(attacker.Root.position - unit.Root.position, unit.FacingYaw, true), "out of the shield's reach as it stands");
                    float yaw = unit.FacingYaw;
                    var hit = Blow(attacker, unit, move, everyone, out string did);
                    float turned = Mathf.DeltaAngle(yaw, unit.FacingYaw);
                    mostTurned = Mathf.Max(mostTurned, turned);
                    report.AppendLine($"{attacker.Loadout.Main.moves[move].name}: {did}; turned {turned:0} degrees; struck {(hit == null ? "nothing" : hit.Shield ? "the shield" : hit.Part.ToString())}");
                    Assert.IsNotNull(hit, "the blow strikes something\n" + report);
                    Assert.IsTrue(hit.Shield, "the shield takes it\n" + report);
                }
                finally
                {
                    RhythmTests.Destroy(everyone);
                }
            }

            System.Console.WriteLine(report);
            Assert.Greater(mostTurned, 25f, "where a blow comes from outside the cone, it turns to face it\n" + report);
        }

        [Test]
        public void ASeenBlowDrawsTheHeadToItsAttackerQuickly()
        {
            var (unit, busyWith) = Defender();
            var attacker = Attacker(unit, -60f);   // (to its left: at the edge of its view)
            var everyone = new[] { unit, busyWith, attacker };
            try
            {
                RhythmTests.Step(everyone, 120);   // (long enough that it is no longer new)
                Assert.AreEqual(Sight.Edge, unit.SightOf(attacker), "caught at the edge of its view");
                Vector3 Off() => attacker.Anim.Part(PartId.Chest).position - unit.EyePosition;
                Assert.Greater(Vector3.Angle(unit.EyeRotation * Vector3.forward, Off()), 40f, "it is looking at the one in front of it");
                var state = MovePlanner.Prepare(attacker, unit, attacker.Loadout.Main.moves[0], AimPart.Chest);
                Assert.IsTrue(attacker.StartMove(state));
                float drawn = -1f;
                for (int i = 0; i < 60; i++)
                {
                    RhythmTests.Step(everyone);
                    if (Vector3.Angle(unit.EyeRotation * Vector3.forward, Off()) <= 30f)
                    {
                        drawn = (i + 1) * Dt;
                        break;
                    }
                }

                Assert.GreaterOrEqual(drawn, 0f, "the head turns to the blow");
                Assert.LessOrEqual(drawn, 0.35f, $"within its latency and a quick turn ({drawn:0.00} s)");
            }
            finally
            {
                RhythmTests.Destroy(everyone);
            }
        }

        /// <summary>An enemy walks right round the unit (which doesn't turn): the head follows it within its limits, and the shield stays in its cone, in front of the chest and out of the body.</summary>
        [Test]
        public void WithAnEnemyWalkingRoundTheHeadAndShieldStayWithinTheBodysLimits()
        {
            var (unit, busyWith) = Defender();
            var everyone = new[] { unit, busyWith };
            try
            {
                RhythmTests.Step(everyone, 30);
                int steps = 0, onIt = 0, reachable = 0;
                float worstTurn = 0f, worstClear = float.MaxValue, worstAhead = float.MaxValue, worstOutside = 0f;
                for (int i = 0; i < 6 * 60; i++)
                {
                    float angle = 360f * i / (6f * 60f);
                    busyWith.Place(Quaternion.Euler(0f, angle, 0f) * Vector3.forward * 1.6f, angle + 180f);
                    RhythmTests.Step(everyone);
                    var pose = unit.Pose;
                    worstTurn = Mathf.Max(worstTurn, Mathf.Abs(pose.HeadTurn));
                    Assert.LessOrEqual(Mathf.Abs(pose.HeadTurn), HeadLook.Turn + 0.5f, $"the head turns at most {HeadLook.Turn} degrees from the chest (step {i})");
                    Assert.IsTrue(pose.HeadPitch >= -HeadLook.Down - 0.5f && pose.HeadPitch <= HeadLook.Up + 0.5f, $"and looks only so far up and down (step {i}: {pose.HeadPitch:0})");
                    var chest = unit.Anim.Part(PartId.Chest);
                    float chestYaw = ShieldCone.FacingOf(chest.rotation, unit.FacingYaw);
                    Vector3 front = Quaternion.Euler(0f, chestYaw, 0f) * Vector3.forward;
                    worstAhead = Mathf.Min(worstAhead, Vector3.Dot(pose.ShieldCenter - chest.position, front));
                    worstClear = Mathf.Min(worstClear, pose.ShieldClearance);
                    worstOutside = Mathf.Max(worstOutside, ShieldCone.Outside(pose.ShieldCenter - chest.position, chestYaw, true));
                    if (unit.Look.Kind == "its enemy" && unit.Look.Reachable)
                    {
                        reachable++;
                        onIt += unit.LookOff <= 25f ? 1 : 0;
                    }

                    steps++;
                }

                Assert.Greater(reachable, steps / 3, "the enemy was in the head's reach for a good part of the walk");
                Assert.GreaterOrEqual(onIt, 0.8f * reachable, $"the head points within 25 degrees of it {100f * onIt / reachable:0}% of the time it can");
                Assert.GreaterOrEqual(worstAhead, -0.01f, $"the shield never goes behind the chest ({worstAhead * 100f:0.0} cm)");
                Assert.GreaterOrEqual(worstClear, -0.02f, $"nor more than 2 cm into its own body ({worstClear * 100f:0.0} cm)");
                Assert.LessOrEqual(worstOutside, 20f, $"and stays about the cone a shield arm covers ({worstOutside:0} degrees outside it at worst)");
            }
            finally
            {
                RhythmTests.Destroy(everyone);
            }
        }
    }
}
