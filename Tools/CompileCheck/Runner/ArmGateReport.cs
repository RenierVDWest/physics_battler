using System;
using PhysicsBattler.Motion;
using UnityEngine;

/// <summary>
/// The gate (<see cref="ArmLimits.Hold"/>) on arms made at random: every arm that leaves it is inside a body's
/// range, an arm already inside is not touched, and an arm moved a little leaves it moved a little (no jumps).
/// </summary>
public static class ArmGateReport
{
    private static Quaternion RandomTurn(System.Random r) => Quaternion.AngleAxis((float)r.NextDouble() * 360f, RandomWay(r));

    private static Vector3 RandomWay(System.Random r)
    {
        while (true)
        {
            var v = new Vector3((float)r.NextDouble() * 2f - 1f, (float)r.NextDouble() * 2f - 1f, (float)r.NextDouble() * 2f - 1f);
            if (v.sqrMagnitude > 0.01f && v.sqrMagnitude <= 1f)
            {
                return v.normalized;
            }
        }
    }

    private struct Arm
    {
        public Quaternion Chest, Hand;
        public Vector3 Shoulder, Elbow, Wrist;
        public bool Left;
    }

    private static Arm Through(Arm a, in ArmReading clip, out bool held, ArmGate gate = null)
    {
        held = (gate ?? new ArmGate()).Hold(a.Chest, a.Shoulder, a.Elbow, a.Wrist, a.Hand, a.Left, clip, 1f / 60f, out var up, out var fore, out var hand);
        var b = a;
        b.Elbow = a.Shoulder + up * (a.Elbow - a.Shoulder);
        b.Wrist = b.Elbow + fore * (up * (a.Wrist - a.Elbow));
        b.Hand = hand * (fore * (up * a.Hand));
        return b;
    }

    private static ArmReading Read(Arm a, in ArmReading clip) => ArmLimits.Read(a.Chest, a.Shoulder, a.Elbow, a.Wrist, a.Hand, a.Left, clip);

    public static int Run()
    {
        var r = new System.Random(11);
        int arms = 200000, touchedInside = 0, heldCount = 0, left = 0;
        float worstAfter = 0f, worstLength = 0f;
        string worstWhat = "";
        for (int i = 0; i < arms; i++)
        {
            var a = new Arm { Chest = RandomTurn(r), Shoulder = RandomWay(r), Hand = RandomTurn(r), Left = r.Next(2) == 0 };
            a.Elbow = a.Shoulder + RandomWay(r) * 0.3f;
            a.Wrist = a.Elbow + RandomWay(r) * 0.27f;
            ArmReading clip = default;
            if (i % 3 == 0)
            {
                // (A clip's arm to make room for: another arm, brought into the range itself.)
                var c = new Arm { Chest = a.Chest, Shoulder = a.Shoulder, Hand = RandomTurn(r), Left = a.Left };
                c.Elbow = c.Shoulder + RandomWay(r) * 0.3f;
                c.Wrist = c.Elbow + RandomWay(r) * 0.27f;
                clip = Read(c, default);
            }

            var before = Read(a, clip);
            var b = Through(a, clip, out bool held);
            var after = Read(b, clip);
            heldCount += held ? 1 : 0;
            left += a.Left ? 1 : 0;
            if (before.Worst <= 0f && held && Mathf.Max(Vector3.Angle(b.Elbow - b.Shoulder, a.Elbow - a.Shoulder), Mathf.Max(Vector3.Angle(b.Wrist - b.Elbow, a.Wrist - a.Elbow), Quaternion.Angle(b.Hand, a.Hand))) > 0.1f)
            {
                touchedInside++;
            }

            worstLength = Mathf.Max(worstLength, Mathf.Max(Mathf.Abs((b.Elbow - b.Shoulder).magnitude - 0.3f), Mathf.Abs((b.Wrist - b.Elbow).magnitude - 0.27f)));
            if (after.Worst > worstAfter)
            {
                worstAfter = after.Worst;
                worstWhat = $"arm {i} ({(a.Left ? "left" : "right")}{(clip.Known ? ", with a clip" : "")}): before raised {before.Raised:0} toward {before.Toward:0} turned {before.Turn:0} bent {before.Bend:0} rolled {before.Roll:0} wrist {before.Flex:0}/{before.Thumbward:0} (past {before.PastReach:0} {before.PastTurn:0} {before.PastBend:0} {before.PastRoll:0} {before.PastWrist:0}); after raised {after.Raised:0} toward {after.Toward:0} turned {after.Turn:0} bent {after.Bend:0} rolled {after.Roll:0} wrist {after.Flex:0}/{after.Thumbward:0} (past {after.PastReach:0.0} {after.PastTurn:0.0} {after.PastBend:0.0} {after.PastRoll:0.0} {after.PastWrist:0.0})";
            }
        }

        Console.WriteLine($"{arms} arms at random ({left} left), {heldCount} outside a body's range: after the gate the furthest past is {worstAfter:0.000}° ({worstWhat});");
        Console.WriteLine($"    arms inside that it moved (by over 0.1°): {touchedInside}; a bone's length changed by at most {worstLength * 1000f:0.000} mm");

        // (An arm moved a little at a time, at random, for a long while: how far its gated arm moves in a step.)
        float mostStep = 0f, mostHand = 0f, pathWorst = 0f;
        int steps = 0, over5 = 0, over15 = 0, pathSteps = 0, pastSteps = 0, longest = 0;
        string mostAt = "", handAt = "";
        for (int path = 0; path < 400; path++)
        {
            var a = new Arm { Chest = RandomTurn(r), Shoulder = Vector3.zero, Hand = RandomTurn(r), Left = path % 2 == 0 };
            Vector3 upper = RandomWay(r), fore = RandomWay(r);
            Vector3 spinU = RandomWay(r), spinF = RandomWay(r), spinH = RandomWay(r);
            Arm last = default;
            bool have = false;
            var gate = new ArmGate();
            int run = 0;
            ArmReading lastSent = default;
            for (int i = 0; i < 1500; i++)
            {
                if (i % 150 == 0)
                {
                    spinU = RandomWay(r);
                    spinF = RandomWay(r);
                    spinH = RandomWay(r);
                }

                upper = Quaternion.AngleAxis(1f, spinU) * upper;
                fore = Quaternion.AngleAxis(1f, spinF) * fore;
                a.Hand = Quaternion.AngleAxis(1f, spinH) * a.Hand;
                a.Elbow = upper * 0.3f;
                a.Wrist = a.Elbow + fore * 0.27f;
                var b = Through(a, default, out _, gate);
                float still = Read(b, default).Worst;
                pathWorst = Mathf.Max(pathWorst, still);
                pathSteps++;
                run = still > 10f ? run + 1 : 0;
                pastSteps += still > 10f ? 1 : 0;
                longest = Mathf.Max(longest, run);
                if (have)
                {
                    float moved = Mathf.Max(Vector3.Angle(b.Elbow, last.Elbow), Vector3.Angle(b.Wrist - b.Elbow, last.Wrist - last.Elbow));
                    float turned = Quaternion.Angle(b.Hand, last.Hand);
                    steps++;
                    over5 += moved > 22f || turned > 22f ? 1 : 0;
                    over15 += moved > 35f || turned > 45f ? 1 : 0;
                    if (moved > mostStep)
                    {
                        mostStep = moved;
                        ArmReading rd = Read(a, default), ro = Read(b, default), lo = Read(last, default);
                        mostAt = $"sent: raised {rd.Raised:0.0} toward {rd.Toward:0.0} turned {rd.Turn:0.0} bent {rd.Bend:0.0} folded {rd.Folded:0.00} (past {rd.PastReach:0} {rd.PastTurn:0} {rd.PastBend:0}), the step before raised {lastSent.Raised:0.0} toward {lastSent.Toward:0.0} turned {lastSent.Turn:0.0} bent {lastSent.Bend:0.0} folded {lastSent.Folded:0.00}; let go: raised {ro.Raised:0.0} toward {ro.Toward:0.0} turned {ro.Turn:0.0} bent {ro.Bend:0.0}, the step before raised {lo.Raised:0.0} toward {lo.Toward:0.0} turned {lo.Turn:0.0} bent {lo.Bend:0.0}";
                    }

                    if (turned > mostHand)
                    {
                        mostHand = turned;
                        ArmReading rd = Read(a, default), ro = Read(b, default), lo = Read(last, default);
                        handAt = $"sent: bent {rd.Bend:0.0} folded {rd.Folded:0.00} rolled {rd.Roll:0.0} wrist {rd.Flex:0.0}/{rd.Thumbward:0.0}, the step before bent {lastSent.Bend:0.0} folded {lastSent.Folded:0.00} rolled {lastSent.Roll:0.0} wrist {lastSent.Flex:0.0}/{lastSent.Thumbward:0.0}; let go: rolled {ro.Roll:0.0} wrist {ro.Flex:0.0}/{ro.Thumbward:0.0} turned {ro.Turn:0.0} raised {ro.Raised:0.0}, the step before rolled {lo.Roll:0.0} wrist {lo.Flex:0.0}/{lo.Thumbward:0.0} turned {lo.Turn:0.0} raised {lo.Raised:0.0}; bones moved {moved:0.0}";
                    }
                }

                last = b;
                lastSent = Read(a, default);
                have = true;
            }
        }

        // (An arm held still outside its range while the clip playing wobbles: the clip's arm, folded right at the far
        // side of the fold's range, moved a degree to and fro each step. The arm let go must not go to and fro with it.)
        float wobble = 0f;
        for (int trial = 0; trial < 2000; trial++)
        {
            var chest = RandomTurn(r);
            bool leftArm = trial % 2 == 0;
            Vector3 upper = chest * new Vector3(leftArm ? -0.6f : 0.6f, -0.6f, 0.5f).normalized;
            Arm Bent(float turn, float bend)
            {
                // (An arm with its upper arm along one way, bent by so much, its fold turned round the upper arm by so much.)
                Vector3 side = Vector3.Cross(upper, chest * Vector3.up).normalized;
                Vector3 fold = Quaternion.AngleAxis(turn, upper) * side;
                var arm = new Arm { Chest = chest, Shoulder = Vector3.zero, Left = leftArm };
                arm.Elbow = upper * 0.3f;
                arm.Wrist = arm.Elbow + (upper * Mathf.Cos(bend * Mathf.Deg2Rad) + fold * Mathf.Sin(bend * Mathf.Deg2Rad)) * 0.27f;
                arm.Hand = Quaternion.LookRotation(Vector3.Cross(arm.Wrist - arm.Elbow, upper).normalized, -(arm.Wrist - arm.Elbow));
                return arm;
            }

            float at = (float)r.NextDouble() * 360f, sentAt = (float)r.NextDouble() * 360f, bendBy = 25f + (float)r.NextDouble() * 100f;
            var gate = new ArmGate();
            Arm last = default;
            for (int i = 0; i < 40; i++)
            {
                var clip = Read(Bent(at + (i % 2 == 0 ? 1f : -1f), (float)r.NextDouble() * 2f + (trial % 3 == 0 ? 24f : 60f)), default);
                var b = Through(Bent(sentAt, bendBy), clip, out _, gate);
                if (i > 2)
                {
                    wobble = Mathf.Max(wobble, Mathf.Max(Vector3.Angle(b.Wrist - b.Elbow, last.Wrist - last.Elbow), Quaternion.Angle(b.Hand, last.Hand)));
                }

                last = b;
            }
        }

        Console.WriteLine($"an arm held still while the clip's own arm wobbles a degree each way: the arm let go moved at most {wobble:0.0}° in a step");
        Console.WriteLine($"{steps} steps of arms moved 1° a joint a step: the gated arm's bones moved at most {mostStep:0.0}° in a step ({mostAt}), its hand turned at most {mostHand:0.0}° ({handAt}); steps over 22° (twice the pace): {over5} ({100f * over5 / steps:0.000}%), over 35° (hand 45°): {over15}; on its way over from one side of a range to the other it was over 10° past a body's range {100f * pastSteps / pathSteps:0.00}% of the steps, for {longest / 60f:0.00} s at the longest (the furthest {pathWorst:0.0}°)");
        return worstAfter < 1f && touchedInside == 0 && over15 < 20 && wobble < 25f ? 0 : 1;
    }
}
