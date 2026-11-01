using System.Collections.Generic;
using NUnit.Framework;
using PhysicsBattler.Authority;
using PhysicsBattler.Diagnostics;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Tests
{
    /// <summary>
    /// Milestone 9c: a held part is servoed critically damped (pushed off its pose, it comes back in
    /// one smooth return, never ringing); a hit's knock on an arm's pose comes back the same way; and
    /// the shield is held close and turns calmly in the guard.
    /// </summary>
    [TestFixture]
    public class ArmTests
    {
        private const float Dt = 1f / 60f;

        /// <summary>A part at rest on its pose, knocked sideways at 6 m/s: how many times its offset changes sign over the next 2 s, and the largest offset.</summary>
        private static (int crossings, float worst, float end) Knocked(float pin)
        {
            Vector3 position = Vector3.zero, velocity = new Vector3(6f, 0f, 0f), target = Vector3.zero;
            int crossings = 0;
            float last = 0f, worst = 0f;
            for (int i = 0; i < 120; i++)
            {
                velocity = AuthorityController.PinVelocity(position, target, velocity, pin, Dt, Physics.gravity);
                velocity += Physics.gravity * Dt;   // (the solver's gravity)
                position += velocity * Dt;
                float x = position.x;
                if (Mathf.Abs(x) > 1e-4f && Mathf.Abs(last) > 1e-4f && Mathf.Sign(x) != Mathf.Sign(last))
                {
                    crossings++;
                }

                if (Mathf.Abs(x) > 1e-4f)
                {
                    last = x;
                }

                worst = Mathf.Max(worst, Mathf.Abs(x));
            }

            return (crossings, worst, position.magnitude);
        }

        [Test]
        public void AKnockedPartComesBackOnceWithoutRinging()
        {
            foreach (float pin in new[] { 0.15f, 0.3f, 0.5f, 0.7f, 0.85f, 0.95f })
            {
                var (crossings, worst, end) = Knocked(pin);
                Assert.AreEqual(0, crossings, $"pin {pin}: it never swings past its pose ({crossings} times)");
                Assert.Less(end, 0.01f, $"pin {pin}: back on its pose within 2 s ({end * 100f:0.0} cm off)");
                Assert.Greater(worst, 0.005f, $"pin {pin}: but it was knocked aside");
            }

            Assert.Greater(Knocked(0.3f).worst, Knocked(0.85f).worst, "a softer hold gives more");
        }

        [Test]
        public void AHeldPartFollowsAMovingPoseClosely()
        {
            Vector3 position = Vector3.zero, velocity = Vector3.zero, last = Vector3.zero;
            float worst = 0f;
            for (int i = 0; i < 180; i++)
            {
                // A pose swinging round in a circle at 2 m/s, as a hand in a slow swing.
                float t = i * Dt;
                Vector3 target = new Vector3(Mathf.Cos(4f * t), 0f, Mathf.Sin(4f * t)) * 0.5f;
                Vector3 poseVelocity = i > 0 ? (target - last) / Dt : Vector3.zero;
                last = target;
                velocity = AuthorityController.PinVelocity(position, target, velocity, 0.7f, Dt, Physics.gravity, poseVelocity);
                velocity += Physics.gravity * Dt;
                position += velocity * Dt;
                if (i > 60)
                {
                    worst = Mathf.Max(worst, Vector3.Distance(position, target));
                }
            }

            Assert.Less(worst, 0.02f, $"it keeps up with its pose's own motion ({worst * 100f:0.0} cm behind at most)");
        }

        [Test]
        public void AFullPinStillArrivesExactlyAndNoPinLeavesItAlone()
        {
            var v = AuthorityController.PinVelocity(Vector3.zero, new Vector3(0.1f, 0f, 0f), Vector3.one, 1f, Dt);
            Assert.AreEqual(0.1f / Dt, v.x, 1e-3f);
            Assert.AreEqual(Vector3.one, AuthorityController.PinVelocity(Vector3.zero, Vector3.right, Vector3.one, 0f, Dt));
            Assert.Less(AuthorityController.HoldFrequency(0.3f), AuthorityController.HoldFrequency(0.7f), "a firmer pin is stiffer");
        }

        [Test]
        public void AnArmsKnockPeaksAndComesBackWithoutOvershoot()
        {
            float w = 2f * Mathf.PI * UnitController.ArmKnockFrequency;
            Vector3 x = Vector3.zero, v = Vector3.right * ((float)System.Math.E * w * 0.1f);   // (kicked to peak at 10 cm)
            float peak = 0f, peakAt = 0f;
            for (int i = 0; i < 90; i++)
            {
                UnitController.CriticalSpring(ref x, ref v, w, Dt);
                Assert.GreaterOrEqual(x.x, -1e-5f, $"never past the pose (step {i}: {x.x * 100f:0.00} cm)");
                if (x.x > peak)
                {
                    peak = x.x;
                    peakAt = i * Dt;
                }
            }

            Assert.AreEqual(0.1f, peak, 0.005f, "it peaks where it was kicked to");
            Assert.Less(peakAt, 0.12f, $"quickly ({peakAt:0.00} s)");
            Assert.Less(x.magnitude, 0.002f, "and is back within 1.5 s");
        }

        [Test]
        public void TheShieldsAimEasesInWithoutOvershootAndNoFasterThanItsLimit()
        {
            float x = 0f, v = 0f, peak = 0f, fastest = 0f;
            for (int i = 0; i < 120; i++)
            {
                float before = x;
                x = UnitController.Ease(x, ref v, 90f, UnitController.ShieldStiffSlow, UnitController.ShieldSlow, Dt);
                peak = Mathf.Max(peak, x);
                fastest = Mathf.Max(fastest, Mathf.Abs(x - before) / Dt);
            }

            Assert.LessOrEqual(peak, 90.01f, "it doesn't overshoot");
            Assert.AreEqual(90f, x, 0.5f, "it arrives");
            Assert.LessOrEqual(fastest, UnitController.ShieldSlow + 0.5f, "at a person's pace");
        }

        /// <summary>A shield-holder squared up to an enemy that steps about in front of it (no blows): the shield is held close and turns calmly.</summary>
        [Test]
        public void InTheGuardTheShieldIsHeldCloseAndTurnsCalmly()
        {
            UnitController.ResetSeeds(3);
            var (unit, enemy) = RhythmTests.Pair("Medium");
            var both = new[] { unit, enemy };
            try
            {
                Assert.IsTrue(unit.Loadout != null && unit.Loadout.HasShield, "the medium unit holds a shield");
                RhythmTests.Step(both, 60);
                var turning = new List<float>();
                var away = new List<float>();
                Vector3 last = unit.Pose.ShieldFace;
                Vector3 home = enemy.transform.position;
                for (int i = 0; i < 6 * 60; i++)
                {
                    // (The enemy sways about its place, 20 cm each way.)
                    float t = i * Dt;
                    enemy.Place(home + new Vector3(0.2f * Mathf.Sin(1.3f * t), 0f, 0.1f * Mathf.Sin(0.7f * t)), enemy.FacingYaw);
                    RhythmTests.Step(both);
                    Vector3 face = unit.Pose.ShieldFace;
                    turning.Add(Vector3.Angle(last, face) / Dt);
                    last = face;
                    away.Add(Vector3.Distance(unit.Anim.Part(PartId.HandL).position, unit.Anim.Part(PartId.Chest).position));
                }

                turning.Sort();
                away.Sort();
                float median = turning[turning.Count / 2], p99 = turning[(int)(0.99f * (turning.Count - 1))];
                float held = away[away.Count / 2];
                Assert.LessOrEqual(median, 20f, $"the shield is mostly still (median {median:0} degrees a second)");
                Assert.LessOrEqual(p99, 250f, $"and never whips round (99th {p99:0} degrees a second)");
                Assert.LessOrEqual(held, 0.5f, $"held close: the shield hand {held:0.00} m from the chest (it was 0.67 before 9c)");
            }
            finally
            {
                RhythmTests.Destroy(both);
            }
        }

        /// <summary>
        /// The arm points the weapon, not the wrist: squared up in the guard, the weapon wrist stays
        /// in its easy range (the reference attacks hold it within about 25 degrees of straight; the
        /// solve weighs it, the hand's joint limits hold it).
        /// </summary>
        [Test]
        public void InTheGuardTheWeaponWristStaysNearlyStraight()
        {
            UnitController.ResetSeeds(3);
            var (unit, enemy) = RhythmTests.Pair("Medium");
            var both = new[] { unit, enemy };
            try
            {
                RhythmTests.Step(both, 60);
                var bends = new List<float>();
                Vector3 home = enemy.transform.position;
                for (int i = 0; i < 4 * 60; i++)
                {
                    float t = i * Dt;
                    enemy.Place(home + new Vector3(0.2f * Mathf.Sin(1.3f * t), 0f, 0.1f * Mathf.Sin(0.7f * t)), enemy.FacingYaw);
                    RhythmTests.Step(both);
                    if (unit.Pose.Move == null)
                    {
                        bends.Add(unit.Pose.WristBend);
                    }
                }

                Assert.Greater(bends.Count, 60, "it stood in the guard");
                bends.Sort();
                float p95 = bends[(int)(0.95f * (bends.Count - 1))], worst = bends[bends.Count - 1];
                Assert.LessOrEqual(p95, PosePipeline.EasyBend + 3f, $"the wrist is kept inside its easy range ({p95:0} degrees at the 95th percentile)");
                Assert.LessOrEqual(worst, PosePipeline.EasyBend + 10f, $"and never far past it ({worst:0} degrees)");
            }
            finally
            {
                RhythmTests.Destroy(both);
            }
        }

        /// <summary>Milestone 9c: the log's shield contacts: one per shield and part, from the step they first overlap to the last, with the deepest overlap.</summary>
        [Test]
        public void ShieldContactsAreFollowedFromStartToEnd()
        {
            var contacts = new ShieldContacts();
            object shield = new object(), chest = new object(), arm = new object();
            var ended = new List<ShieldContacts.Contact>();
            float[] chestDepth = { 0.005f, 0.012f, 0.008f, 0f, 0f, 0.03f };
            for (int i = 0; i < chestDepth.Length; i++)
            {
                contacts.BeginStep();
                if (chestDepth[i] > 0f)
                {
                    contacts.Seen(shield, chest, "A's shield", "B Chest", i * Dt, chestDepth[i]);
                }

                if (i < 2)
                {
                    contacts.Seen(shield, arm, "A's shield", "B UpperArmL", i * Dt, 0.004f);
                }

                contacts.EndStep(Dt, ended);
            }

            Assert.AreEqual(2, ended.Count, "the arm's ended at step 2, the chest's first at step 3");
            Assert.AreEqual(2f * Dt, ended[0].duration, 1e-5f);
            Assert.AreEqual(3f * Dt, ended[1].duration, 1e-5f, "three steps");
            Assert.AreEqual(0.012f, ended[1].deepest, 1e-6f, "its deepest");
            Assert.AreEqual(1, contacts.OpenCount, "a second one with the chest is going on");
            contacts.CloseAll(Dt, ended);
            Assert.AreEqual(3, contacts.Done.Count);
            var (count, p95, deepest, longest, under) = ShieldContacts.Summary(contacts.Done);
            Assert.AreEqual(3, count);
            Assert.AreEqual(0.03f, deepest, 1e-6f);
            Assert.AreEqual(0.03f, p95, 1e-6f, "of three, the 95th is the deepest");
            Assert.AreEqual(3f * Dt, longest, 1e-5f);
            Assert.AreEqual(2f / 3f, under, 1e-5f, "two of three under 2 cm");
        }

        /// <summary>A physical unit's shield is solid and its weight is on the hand; taking it off takes the weight away.</summary>
        [Test]
        public void TheShieldIsSolidAndWeighsOnTheHand()
        {
            UnitController.ResetSeeds(3);
            var (unit, enemy) = RhythmTests.Pair("Medium");
            var both = new[] { unit, enemy };
            try
            {
                Assert.IsTrue(unit.IsPhysical && unit.OffWeapon != null && unit.OffWeapon.definition.IsShield, "a physical unit with a shield");
                var hand = unit.Rig.Part(PartId.HandL).body;
                float withShield = hand.mass;
                foreach (var collider in unit.OffWeapon.GetComponentsInChildren<Collider>(true))
                {
                    Assert.IsFalse(collider.isTrigger, "the shield's collider is solid");
                }

                var shield = unit.OffWeapon.definition;
                float scale = unit.Loadout.Scale;
                unit.Equip(unit.MainWeapon.definition, null);
                Assert.AreEqual(withShield - shield.massKg * scale * scale * scale, hand.mass, 1e-3f, "without it the hand is lighter by the shield's mass");
                unit.Equip(unit.MainWeapon.definition, shield);
                Assert.AreEqual(withShield, hand.mass, 1e-3f, "and it comes back once (not added twice)");
            }
            finally
            {
                RhythmTests.Destroy(both);
            }
        }
    }
}
