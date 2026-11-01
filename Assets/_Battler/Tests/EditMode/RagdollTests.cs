using NUnit.Framework;
using PhysicsBattler.Authority;
using PhysicsBattler.Battle;
using PhysicsBattler.Muscles;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Tests
{
    /// <summary>
    /// Milestone 9b: the maths under bodies on the ground and natural arms. How far a joint is past
    /// its limits (split as PhysX splits it); the eased limits that start where a joint is; the cap
    /// on a standing unit's push; the way round a body on the ground; and the posed wrists and
    /// shield arm staying within a body's limits. (What PhysX does with these, the stand-in physics
    /// here can't show: that is for the sandbox's ground test.)
    /// </summary>
    [TestFixture]
    public class RagdollTests
    {
        // ---- How far past its limits ---------------------------------------------------------------

        [Test]
        public void AJointInsideItsLimitsIsNotPastThem()
        {
            var elbow = JointLimitTable.For(PartId.ForearmR);
            Assert.AreEqual(0f, JointMath.PastLimits(Quaternion.identity, elbow, out _, out _, out _), 1e-3f, "at rest");
            Assert.AreEqual(0f, JointMath.PastLimits(Quaternion.Euler(-90f, 0f, 0f), elbow, out float pitch, out _, out _), 1e-3f, "bent");
            Assert.AreEqual(-90f, pitch, 0.5f, "the bend is about X");
        }

        [Test]
        public void ABendPastItsRangeAndASwingPastItsConeAreMeasuredApart()
        {
            var elbow = JointLimitTable.For(PartId.ForearmR);   // (bend -145..5, swing about Z 15)
            float past = JointMath.PastLimits(Quaternion.Euler(-160f, 0f, 0f), elbow, out float pitch, out _, out _);
            Assert.AreEqual(-160f, pitch, 0.5f);
            Assert.AreEqual(15f, past, 0.5f, "15 degrees past its bend");

            past = JointMath.PastLimits(Quaternion.Euler(0f, 0f, 35f), elbow, out pitch, out _, out float aboutZ);
            Assert.AreEqual(0f, pitch, 0.5f, "no bend");
            Assert.AreEqual(35f, aboutZ, 0.5f, "the swing is about Z");
            Assert.AreEqual(35f - elbow.swing, past, 0.5f, "past its swing");

            // A bend and a twist together: the bend is taken first (about X), then the swing.
            past = JointMath.PastLimits(Quaternion.Euler(0f, 30f, 0f) * Quaternion.Euler(-60f, 0f, 0f), elbow, out pitch, out float aboutY, out _);
            Assert.AreEqual(-60f, pitch, 1f);
            Assert.AreEqual(30f, aboutY, 1f);
            Assert.AreEqual(0f, past, 1e-3f, "both inside");
        }

        // ---- Eased ragdoll limits -------------------------------------------------------------------

        [Test]
        public void LimitsWidenToHoldAJointWhereItIsAndNoFurther()
        {
            var knee = JointLimitTable.ForRagdoll(PartId.ShinR);   // (a hinge: no side swing on the ground)
            var inside = AuthorityController.Widened(knee, new Vector3(40f, 0f, 0f));
            Assert.AreEqual(knee.pitchMin, inside.pitchMin, 1e-3f, "inside: its own limits");
            Assert.AreEqual(knee.pitchMax, inside.pitchMax, 1e-3f);
            Assert.AreEqual(AuthorityController.LockWithin, inside.swing, 1e-3f, "a locked axis keeps a little room to lock in");

            var past = AuthorityController.Widened(knee, new Vector3(160f, -20f, 30f));
            Assert.AreEqual(162f, past.pitchMax, 1e-3f, "just past where it is bent");
            Assert.AreEqual(22f, past.twist, 1e-3f, "and twisted");
            Assert.AreEqual(32f, past.swing, 1e-3f, "and swung");

            var wild = AuthorityController.Widened(knee, new Vector3(0f, 179f, -179f));
            Assert.LessOrEqual(wild.twist, 177f, "never a full turn (PhysX's cone stops short of 180)");
            Assert.LessOrEqual(wild.swing, 177f);
        }

        // ---- A standing unit's push on a body on the ground ------------------------------------------

        [Test]
        public void AStandingUnitsPushOnALyingPartIsCappedByThatPartsWeight()
        {
            Assert.AreEqual(4f * GroundContacts.MaxNudge, GroundContacts.Cap(4f, 1), 1e-5f, "one point: the whole nudge");
            Assert.AreEqual(4f * GroundContacts.MaxNudge / 4f, GroundContacts.Cap(4f, 4), 1e-5f, "shared out over the points");
            Assert.AreEqual(4f * GroundContacts.MaxNudge, GroundContacts.Cap(4f, 0), 1e-5f, "no points counted: as one");
            Assert.Less(GroundContacts.Cap(1f, 1), GroundContacts.Cap(10f, 1), "a heavier part may be pushed harder (the same change of speed)");
        }

        // ---- Walking round a body on the ground ------------------------------------------------------

        [Test]
        public void AWalkerKeepsRoundABodyOnTheGround()
        {
            var lying = RhythmTests.Build("Medium", Vector3.zero, 0f, 0);
            try
            {
                // Flat on its back along +Z: hips at the origin, head at +Z, feet at -Z.
                lying.Rig.Part(PartId.Pelvis).transform.position = new Vector3(0f, 0.15f, 0f);
                lying.Rig.Part(PartId.Head).transform.position = new Vector3(0f, 0.15f, 0.75f);
                lying.Rig.Part(PartId.FootL).transform.position = new Vector3(-0.15f, 0.1f, -0.9f);
                lying.Rig.Part(PartId.FootR).transform.position = new Vector3(0.15f, 0.1f, -0.9f);
                const float mine = 0.3f, speed = 1f;

                Vector3 beside = FightLoop.AroundBody(new Vector3(0.25f, 0f, 0.4f), mine, lying, speed);
                Assert.Greater(beside.x, 0.1f, $"beside the chest: pushed away to the side ({beside})");
                Assert.AreEqual(0f, beside.y, 1e-5f, "along the ground");

                Vector3 legs = FightLoop.AroundBody(new Vector3(-0.35f, 0f, -0.6f), mine, lying, speed);
                Assert.Less(legs.x, -0.05f, $"beside the left leg: pushed out to the left ({legs})");

                Vector3 head = FightLoop.AroundBody(new Vector3(0f, 0f, 0.95f), mine, lying, speed);
                Assert.Greater(head.z, 0.1f, $"past the head: pushed on past it ({head})");

                Vector3 clear = FightLoop.AroundBody(new Vector3(1.2f, 0f, 0f), mine, lying, speed);
                Assert.AreEqual(Vector3.zero, clear, "well clear: no push");

                Vector3 close = FightLoop.AroundBody(new Vector3(0.1f, 0f, 0.4f), mine, lying, speed);
                Assert.Greater(close.magnitude, beside.magnitude, "the closer, the harder");
            }
            finally
            {
                RhythmTests.Destroy(lying);
            }
        }

        // ---- Natural arms ----------------------------------------------------------------------------

        /// <summary>Each body squared up to an opponent, its rhythm running but no blows: the wrists stay inside their limits (the elbow and forearm take the turn), and a shield stays clear of its own arm.</summary>
        [Test]
        public void InGuardTheWristsStayInsideTheirLimitsAndTheShieldClearOfItsArm()
        {
            foreach (string body in new[] { "Small", "Medium", "Large" })
            {
                UnitController.ResetSeeds(0);
                var (unit, opponent) = RhythmTests.Pair(body);
                var both = new[] { unit, opponent };
                try
                {
                    RhythmTests.Step(both, 30);
                    float worstR = 0f, worstL = 0f, worstClear = float.MaxValue;
                    for (int i = 0; i < 5 * 60; i++)
                    {
                        RhythmTests.Step(both);
                        if (unit.Pose.Move != null)
                        {
                            continue;   // (in a blow the weapon turns as the blow needs)
                        }

                        worstR = Mathf.Max(worstR, unit.JointPast(PartId.HandR, false));
                        worstL = Mathf.Max(worstL, unit.JointPast(PartId.HandL, false));
                        if (unit.Loadout.Off != null && unit.Loadout.Off.IsShield)
                        {
                            worstClear = Mathf.Min(worstClear, unit.Pose.ShieldClearance);
                        }
                    }

                    Assert.LessOrEqual(worstR, 3f, $"{body}: the weapon wrist at most 3 degrees past its limits ({worstR:0.0})");
                    Assert.LessOrEqual(worstL, 3f, $"{body}: the other wrist too ({worstL:0.0})");
                    if (worstClear < float.MaxValue)
                    {
                        Assert.GreaterOrEqual(worstClear, -0.01f * unit.Body.SizeFactor, $"{body}: the shield clear of its own arm and body ({worstClear * 100f:0.0} cm)");
                    }
                }
                finally
                {
                    RhythmTests.Destroy(both);
                }
            }
        }
    }
}
