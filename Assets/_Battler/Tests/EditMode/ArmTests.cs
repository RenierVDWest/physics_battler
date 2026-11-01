using System.Collections.Generic;
using NUnit.Framework;
using PhysicsBattler.Authority;
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
    }
}
