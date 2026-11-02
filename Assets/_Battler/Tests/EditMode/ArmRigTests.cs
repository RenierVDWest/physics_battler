using NUnit.Framework;
using PhysicsBattler.Motion;
using PhysicsBattler.Muscles;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Tests
{
    /// <summary>
    /// Milestone 10 rebuild: the arm rig. An arm made from joint angles inside its limits is found
    /// again exactly (elbow and all) from its hand and its elbow's way; no goal, however impossible,
    /// takes a joint past its limits or bends the elbow sideways; the same goal gives the same arm
    /// whatever the arm was doing; a smooth goal gives a smooth arm, straightening arms included.
    /// </summary>
    [TestFixture]
    public class ArmRigTests
    {
        private static readonly string[] Bodies = { "Small", "Medium", "Large" };

        private static PartId Upper(bool right) => right ? PartId.UpperArmR : PartId.UpperArmL;
        private static PartId Fore(bool right) => right ? PartId.ForearmR : PartId.ForearmL;
        private static PartId Hand(bool right) => right ? PartId.HandR : PartId.HandL;

        /// <summary>Poses the arm from joint angles (shoulder in the joint table's terms, elbow bend, wrist twist / flexion / deviation).</summary>
        private static void PoseFromAngles(AnimRig anim, bool right, Quaternion shoulder, float bend, float twist, float flexion, float deviation)
        {
            anim.Part(Upper(right)).localRotation = anim.RestLocalRotation(Upper(right)) * shoulder;
            anim.Part(Fore(right)).localRotation = anim.RestLocalRotation(Fore(right)) * Quaternion.AngleAxis(bend, ArmRig.ElbowAxis(anim, right));
            float ty = Mathf.Tan(0.25f * flexion * Mathf.Deg2Rad), tz = Mathf.Tan(0.25f * deviation * Mathf.Deg2Rad), t2 = ty * ty + tz * tz;
            var swing = new Quaternion(0f, 2f * ty / (1f + t2), 2f * tz / (1f + t2), (1f - t2) / (1f + t2));
            var frame = ArmRig.WristFrame(anim, right);
            anim.Part(Hand(right)).localRotation = anim.RestLocalRotation(Hand(right)) * (frame * (swing * Quaternion.AngleAxis(twist, Vector3.right)) * Quaternion.Inverse(frame));
        }

        private static Quaternion RandomShoulder(System.Random random, PartId id)
        {
            var limits = JointLimitTable.For(id);
            for (int tries = 0; tries < 200; tries++)
            {
                var q = Quaternion.Euler(R(random, -160f, 50f), R(random, -80f, 80f), R(random, -110f, 110f));
                if (JointMath.PastLimits(q, limits, out _, out _, out _) <= 0f)
                {
                    return q;
                }
            }

            return Quaternion.identity;
        }

        private static float R(System.Random random, float lo, float hi) => lo + (float)random.NextDouble() * (hi - lo);

        private static Vector3 ChestNull(AnimRig anim, bool right, Vector3 worldWay)
        {
            Vector3 local = Quaternion.Inverse(anim.Part(PartId.Chest).rotation) * worldWay;
            return right ? local : new Vector3(-local.x, local.y, local.z);
        }

        [Test]
        public void AnArmMadeInsideItsLimitsIsFoundAgainExactly()
        {
            var random = new System.Random(4);
            foreach (var body in Bodies)
            {
                var unit = RhythmTests.Build(body, Vector3.zero, 0f, 0);
                try
                {
                    var anim = unit.Anim;
                    int checkedArms = 0;
                    float worstPoint = 0f, worstTurn = 0f, worstElbow = 0f;
                    for (int i = 0; i < 120; i++)
                    {
                        bool right = i % 2 == 0;
                        float bend = R(random, 30f, 135f);   // (short of straight: a straight arm's reach is eased, by design)
                        // (Inside the limits, short of the ellipse's edge and of the deviation toward the thumb.)
                        float flex = R(random, -0.7f, 0.7f) * ArmRig.FlexLimit, dev = R(random, -0.7f * ArmRig.DeviationLimit, 0.7f * ArmRig.RadialLimit);
                        if ((flex / ArmRig.FlexLimit) * (flex / ArmRig.FlexLimit) + (dev / ArmRig.DeviationLimit) * (dev / ArmRig.DeviationLimit) > 0.7f)
                        {
                            continue;
                        }

                        PoseFromAngles(anim, right, RandomShoulder(random, Upper(right)), bend, R(random, -70f, 70f), flex, dev);
                        Vector3 shoulder = anim.Part(Upper(right)).position, elbow = anim.Part(Fore(right)).position;
                        Vector3 wrist = anim.Part(Hand(right)).position;
                        Quaternion hand = anim.Part(Hand(right)).rotation;
                        // (The elbow's way off the shoulder-to-wrist line: what the null names.)
                        Vector3 n = (wrist - shoulder).normalized, way = (elbow - shoulder) - n * Vector3.Dot(elbow - shoulder, n);
                        // (The upper arm inside the shoulder's reach, short of its edge.)
                        Vector3 neutral = anim.Part(PartId.Chest).rotation * (right ? new Vector3(0.3f, -0.5f, 0.8f) : new Vector3(-0.3f, -0.5f, 0.8f)).normalized;
                        if (way.magnitude < 0.02f || Vector3.Angle(elbow - shoulder, neutral) > ArmRig.ShoulderReach - 5f)
                        {
                            continue;
                        }

                        // Scrambled first: the rig keeps nothing from before.
                        PoseFromAngles(anim, right, Quaternion.Euler(-40f, 30f, 20f), 60f, 30f, 10f, 0f);
                        var solve = ArmRig.Solve(anim, right, ArmGoal.Hand(wrist, Vector3.zero, hand, ChestNull(anim, right, way.normalized)));
                        worstPoint = Mathf.Max(worstPoint, solve.pointMiss);
                        worstTurn = Mathf.Max(worstTurn, solve.turnMiss);
                        worstElbow = Mathf.Max(worstElbow, Vector3.Distance(anim.Part(Fore(right)).position, elbow));
                        checkedArms++;
                    }

                    Assert.Greater(checkedArms, 40, $"{body}: arms tried");
                    Assert.Less(worstPoint, 0.002f * unit.Body.SizeFactor, $"{body}: the hand reaches ({worstPoint * 1000f:0.0} mm)");
                    Assert.Less(worstTurn, 0.5f, $"{body}: and is turned as asked ({worstTurn:0.00} degrees)");
                    Assert.Less(worstElbow, 0.005f * unit.Body.SizeFactor, $"{body}: with the elbow where it was ({worstElbow * 1000f:0.0} mm)");
                }
                finally
                {
                    RhythmTests.Destroy(unit);
                }
            }
        }

        [Test]
        public void NoGoalTakesAJointPastItsLimits()
        {
            var random = new System.Random(9);
            foreach (var body in Bodies)
            {
                var unit = RhythmTests.Build(body, Vector3.zero, 0f, 0);
                try
                {
                    var anim = unit.Anim;
                    float full = unit.Body.SizeFactor;
                    float worstWrist = 0f, worstSide = 0f, worstShoulderOver = 0f;
                    for (int i = 0; i < 200; i++)
                    {
                        bool right = i % 2 == 1;
                        Vector3 shoulder = anim.Part(Upper(right)).position;
                        // (Anywhere round the body, out of reach and tucked in too; any turn, any null.)
                        // (In front and beside, where blows and guards ask for hands; behind the back no arm reaches.)
                        Vector3 way = Random.onUnitSphereOf(random);
                        if (Vector3.Angle(way, new Vector3(0.3f, -0.5f, 0.8f)) > 100f)
                        {
                            continue;
                        }

                        if (!right)
                        {
                            way.x = -way.x;
                        }

                        Vector3 point = shoulder + anim.Part(PartId.Chest).rotation * way * R(random, 0.05f, 1.1f) * full;
                        var goal = ArmGoal.Hand(point, Vector3.zero, RandomTurn(random), Random.onUnitSphereOf(random));
                        var solve = ArmRig.Solve(anim, right, goal);
                        Quaternion handLocal = Quaternion.Inverse(anim.RestLocalRotation(Hand(right))) * anim.Part(Hand(right)).localRotation;
                        float past = ArmRig.WristAngles(handLocal, ArmRig.WristFrame(anim, right), out float twist, out _, out float deviation);
                        worstWrist = Mathf.Max(worstWrist, past);
                        Assert.LessOrEqual(Mathf.Abs(twist), ArmRig.TwistLimit + 0.01f, $"{body}: the twist");
                        Assert.LessOrEqual(deviation, ArmRig.RadialLimit + 0.01f, $"{body}: toward the thumb");
                        // The elbow is a hinge: the forearm turns about its axis only, 0 to 145.
                        Quaternion foreLocal = Quaternion.Inverse(anim.RestLocalRotation(Fore(right))) * anim.Part(Fore(right)).localRotation;
                        foreLocal.ToAngleAxis(out float angle, out Vector3 axis);
                        Vector3 hinge = ArmRig.ElbowAxis(anim, right);
                        if (angle > 0.5f)
                        {
                            worstSide = Mathf.Max(worstSide, Vector3.Angle(axis, hinge));
                        }

                        Assert.LessOrEqual(solve.elbowBend, ArmRig.ElbowBendLimit + 0.01f, $"{body}: the elbow's bend");
                        worstShoulderOver = Mathf.Max(worstShoulderOver, solve.shoulderPast);
                    }

                    Assert.Less(worstWrist, 0.05f, $"{body}: the wrist inside its limits ({worstWrist:0.00} degrees past)");
                    Assert.Less(worstSide, 0.5f, $"{body}: the elbow bends only about its hinge ({worstSide:0.0} degrees off it)");
                    // (Hands within 100 degrees of the forward reach: the upper arm stays about within its 125, a soft limit.)
                    Assert.Less(worstShoulderOver, 10f, $"{body}: the shoulder at most a little past its limits ({worstShoulderOver:0} degrees)");
                }
                finally
                {
                    RhythmTests.Destroy(unit);
                }
            }
        }

        [Test]
        public void TheSameGoalGivesTheSameArmWhateverCameBefore()
        {
            var random = new System.Random(2);
            var unit = RhythmTests.Build("Large", Vector3.zero, 0f, 0);
            try
            {
                var anim = unit.Anim;
                for (int i = 0; i < 40; i++)
                {
                    Vector3 shoulder = anim.Part(PartId.UpperArmR).position;
                    var goal = ArmGoal.Hand(shoulder + Random.onUnitSphereOf(random) * R(random, 0.2f, 0.7f), Vector3.zero, RandomTurn(random), Random.onUnitSphereOf(random));
                    PoseFromAngles(anim, true, RandomShoulder(random, PartId.UpperArmR), R(random, 0f, 140f), R(random, -80f, 80f), 0f, 0f);
                    ArmRig.Solve(anim, true, goal);
                    Quaternion u = anim.Part(PartId.UpperArmR).rotation, f = anim.Part(PartId.ForearmR).rotation, h = anim.Part(PartId.HandR).rotation;
                    PoseFromAngles(anim, true, RandomShoulder(random, PartId.UpperArmR), R(random, 0f, 140f), R(random, -80f, 80f), 20f, -10f);
                    ArmRig.Solve(anim, true, goal);
                    Assert.Less(Quaternion.Angle(u, anim.Part(PartId.UpperArmR).rotation) + Quaternion.Angle(f, anim.Part(PartId.ForearmR).rotation)
                                + Quaternion.Angle(h, anim.Part(PartId.HandR).rotation), 0.05f, "the same arm");
                }
            }
            finally
            {
                RhythmTests.Destroy(unit);
            }
        }

        [Test]
        public void ASmoothGoalGivesASmoothArm()
        {
            foreach (var body in Bodies)
            {
                var unit = RhythmTests.Build(body, Vector3.zero, 0f, 0);
                try
                {
                    var anim = unit.Anim;
                    // Two sweeps made from joint angles moving smoothly inside their limits: a bent arm
                    // (found again exactly, step by step), and a nearly straight one swept across the
                    // front with a fixed null (it must not flip).
                    foreach (bool straight in new[] { false, true })
                    {
                        Quaternion lastU = default, lastF = default, lastH = default, lastGoal = default;
                        float worst = 0f, worstGoal = 0f, worstPoint = 0f;
                        int steps = 240;
                        for (int i = 0; i <= steps; i++)
                        {
                            float a = 2f * Mathf.PI * i / steps;
                            var shoulder = Quaternion.Euler(-70f + 40f * Mathf.Sin(a), 25f * Mathf.Cos(a), -20f + 30f * Mathf.Sin(2f * a));
                            float bend = straight ? 3f : 70f + 40f * Mathf.Sin(a + 1f);
                            PoseFromAngles(anim, true, shoulder, bend, 60f * Mathf.Sin(a), 35f * Mathf.Cos(a), -15f + 10f * Mathf.Sin(3f * a));
                            Vector3 s0 = anim.Part(PartId.UpperArmR).position, e0 = anim.Part(PartId.ForearmR).position, w0 = anim.Part(PartId.HandR).position;
                            Quaternion h0 = anim.Part(PartId.HandR).rotation;
                            Vector3 n = (w0 - s0).normalized, way = (e0 - s0) - n * Vector3.Dot(e0 - s0, n);
                            Vector3 elbowNull = straight ? new Vector3(0.4f, -1f, 0f) : ChestNull(anim, true, way.normalized);
                            var solve = ArmRig.Solve(anim, true, ArmGoal.Hand(w0, Vector3.zero, h0, elbowNull));
                            if (!straight)
                            {
                                worstPoint = Mathf.Max(worstPoint, solve.pointMiss);
                            }

                            Quaternion u = anim.Part(PartId.UpperArmR).rotation, f = anim.Part(PartId.ForearmR).rotation, h = anim.Part(PartId.HandR).rotation;
                            if (i > 0)
                            {
                                worst = Mathf.Max(worst, Mathf.Max(Quaternion.Angle(u, lastU), Mathf.Max(Quaternion.Angle(f, lastF), Quaternion.Angle(h, lastH))));
                                worstGoal = Mathf.Max(worstGoal, Quaternion.Angle(h0, lastGoal));
                            }

                            lastU = u;
                            lastF = f;
                            lastH = h;
                            lastGoal = h0;
                        }

                        string what = straight ? "a straight arm swept across" : "a bent arm";
                        Assert.Less(worstPoint, 0.002f * unit.Body.SizeFactor, $"{body}, {what}: the hand reaches ({worstPoint * 1000f:0.0} mm)");
                        Assert.Less(worst, 3f * worstGoal + 2f, $"{body}, {what}: no part turns much faster than the hand asked ({worst:0.0} degrees a step, the hand {worstGoal:0.0})");
                    }
                }
                finally
                {
                    RhythmTests.Destroy(unit);
                }
            }
        }

        private static Quaternion RandomTurn(System.Random random) => Quaternion.Euler(R(random, -180f, 180f), R(random, -180f, 180f), R(random, -180f, 180f));

        /// <summary>Seeded directions (UnityEngine.Random isn't seeded per test).</summary>
        private static class Random
        {
            public static Vector3 onUnitSphereOf(System.Random random)
            {
                for (;;)
                {
                    var v = new Vector3(R(random, -1f, 1f), R(random, -1f, 1f), R(random, -1f, 1f));
                    if (v.sqrMagnitude > 0.01f && v.sqrMagnitude <= 1f)
                    {
                        return v.normalized;
                    }
                }
            }
        }
    }
}
