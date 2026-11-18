// `stumble`: the stumble model by itself (StumbleSteps), no fighter: a body standing in a guard's stance (feet 40 cm
// apart across and 30 cm fore and aft) is thrown at several speeds and from several sides, and the model is stepped
// until it ends. For each: steps, seconds, how far he went, how far he turned, how far his hips sank, and how it ended.
// STUMBLE_REACH, STUMBLE_FOOT, STUMBLE_TAKES, STUMBLE_BRAKE, STUMBLE_REACTS, STUMBLE_DOWN: the levers. STUMBLE_SPIN: degrees a second he turns at as it begins.
// STUMBLE_TRACE="2.2,180": that one case step by step.
using System;
using System.Globalization;
using PhysicsBattler.Locomotion;
using UnityEngine;

internal static class StumbleReport
{
    private static void Lever(string name, Action<float> set)
    {
        if (float.TryParse(Environment.GetEnvironmentVariable(name), NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
        {
            set(v);
        }
    }

    public static void Levers()
    {
        Lever("STUMBLE_REACH", v => StumbleSteps.StepReach = v);
        Lever("STUMBLE_FOOT", v => StumbleSteps.FootSpeed = v);
        Lever("STUMBLE_TAKES", v => StumbleSteps.StepTakes = v);
        Lever("STUMBLE_BRAKE", v => StumbleSteps.LegsBrake = v);
        Lever("STUMBLE_REACTS", v => StumbleSteps.ReactsIn = v);
        Lever("STUMBLE_DOWN", v => StumbleSteps.DownAt = v);
        Lever("STUMBLE_KNEES", v => StumbleSteps.KneesGive = v);
        Lever("STUMBLE_TURNED", v => StumbleSteps.TurnedBy = v);
        Lever("STUMBLE_TIPS", v => StumbleSteps.TrunkTips = v);
    }

    public static int Run()
    {
        var ci = CultureInfo.InvariantCulture;
        Levers();
        float spin = 0f;
        Lever("STUMBLE_SPIN", v => spin = v);
        string trace = Environment.GetEnvironmentVariable("STUMBLE_TRACE");
        const float leg = 0.85f, dt = 1f / 60f;
        // (Facing +z; the left foot ahead, as a shield bearer stands.)
        Vector3 left = new Vector3(-0.2f, 0f, 0.15f), right = new Vector3(0.2f, 0f, -0.15f);
        Console.WriteLine(string.Format(ci, "STUMBLE levers: reach {0:0.00} leg, foot {1:0.0} m/s, step {2:0.00} s, brake {3:0.00}, reacts {4:0.00} s, down at {5:0.00} leg; spin {6:0} deg/s", StumbleSteps.StepReach, StumbleSteps.FootSpeed, StumbleSteps.StepTakes, StumbleSteps.LegsBrake, StumbleSteps.ReactsIn, StumbleSteps.DownAt, spin));
        Console.WriteLine("STUMBLE thrown: 180 is straight back, 0 forward, 90 to his right, 270 to his left");
        foreach (float way in new[] { 180f, 0f, 90f, 270f, 135f })
        {
            foreach (float speed in new[] { 0.5f, 0.8f, 1.1f, 1.4f, 1.7f, 2.0f, 2.3f, 2.6f, 2.9f, 3.2f, 3.6f, 4.2f })
            {
                Vector3 vel = Quaternion.Euler(0f, way, 0f) * Vector3.forward * speed;
                bool traced = trace != null && trace == string.Format(ci, "{0:0.0},{1:0}", speed, way);
                if (!StumbleSteps.OffBalance(Vector3.zero, vel, left, right, leg))
                {
                    Console.WriteLine(string.Format(ci, "STUMBLE {0,3:0} deg {1:0.0} m/s: not off balance (his stance takes it)", way, speed));
                    continue;
                }

                var s = new StumbleSteps();
                s.Begin(Vector3.zero, vel, 0f, spin, left, right, 0f, 0f, leg);
                float sunk = 0f, air = 0f;
                while (s.Active)
                {
                    s.Step(dt);
                    sunk = Mathf.Max(sunk, s.Sunk);
                    air += !s.Down[0] && !s.Down[1] ? dt : 0f;
                    if (traced)
                    {
                        Console.WriteLine(string.Format(ci, "  t {0:0.00} com ({1:0.00},{2:0.00}) v {3:0.00} L({4:0.00},{5:0.00},{6:0.00}){7} R({8:0.00},{9:0.00},{10:0.00}){11} sink {12:0.00} sunk {13:0.00} face {14:0}",
                            s.For, s.Com.x, s.Com.z, s.Vel.magnitude, s.Foot[0].x, s.Foot[0].z, s.Foot[0].y, s.Down[0] ? "d" : "-", s.Foot[1].x, s.Foot[1].z, s.Foot[1].y, s.Down[1] ? "d" : "-", s.Sink, s.Sunk, s.Facing));
                    }
                }

                Console.WriteLine(string.Format(ci, "STUMBLE {0,3:0} deg {1:0.0} m/s: {2}: {3} step{4}, {5:0.00} s, went {6:0.00} m, turned {7:0} deg, hips down {8:0} cm at most, {9:0.00} s with no foot down{10}",
                    way, speed, s.Ended == StumbleSteps.Ends.Down ? "DOWN  " : "caught", s.Steps, s.Steps == 1 ? "" : "s", s.For, s.Carried, s.TurnedIn, 100f * sunk, air, s.Ended == StumbleSteps.Ends.Down ? " (" + s.Why + ")" : ""));
            }
        }

        return 0;
    }
}
