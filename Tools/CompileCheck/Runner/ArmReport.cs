// `arms [seed]`: milestone 9c. A whole fight (as `fight`), measuring the pose each step: the weapon
// wrist's bend (the hand against the forearm, the roll about the forearm left out, as
// Tools/Playtest/reference_wrists.py measures the reference attack FBXs: median 23, 95th 45), in the
// guard and in blows; the shield's face turning (degrees per second) by its state, with no hit on
// the unit in the last second; and how far the shield hand is from the chest.
using System;
using System.Collections.Generic;
using System.IO;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

internal static class ArmReport
{
    public static int Run(int seed)
    {
        var rigs = new Dictionary<string, RigFile>();
        foreach (var body in new[] { "Small", "Medium", "Large" })
        {
            rigs[body] = RigFile.Parse(File.ReadAllText(Path.Combine(Application.dataPath, $"_Battler/Data/Rigs/{body.ToLowerInvariant()}_rig.json")));
        }

        var wristGuard = new List<float>();
        var wristBlow = new List<float>();
        var shieldTurn = new Dictionary<string, List<float>>();
        var shieldOut = new List<float>();
        var shieldAway = new Dictionary<string, List<float>>();
        var lastFace = new Dictionary<UnitController, Vector3>();
        var lastPoise = new Dictionary<UnitController, float>();
        var struckAt = new Dictionary<UnitController, float>();
        float clock = 0f;
        FightReport.OnStep = (units, dt) =>
        {
            clock += dt;
            foreach (var u in units)
            {
                if (!u.IsAlive || u.State != UnitState.Standing)
                {
                    lastFace.Remove(u);
                    continue;
                }

                if (lastPoise.TryGetValue(u, out float p) && u.Poise < p - 0.01f)
                {
                    struckAt[u] = clock;
                }

                lastPoise[u] = u.Poise;
                if (u.Loadout?.Main != null && !u.Rig.IsSevered(PartId.HandR))
                {
                    float bend = WristBend(u.Anim.Part(PartId.ForearmR), u.Anim.Part(PartId.HandR));
                    (u.Pose.Move != null ? wristBlow : wristGuard).Add(bend);
                }

                if (u.Loadout != null && u.Loadout.HasShield && !u.Rig.IsSevered(PartId.HandL))
                {
                    Vector3 face = u.Pose.ShieldFace;
                    bool quiet = !struckAt.TryGetValue(u, out float at) || clock - at > 1f;
                    if (lastFace.TryGetValue(u, out var before) && quiet && dt > 0f)
                    {
                        string mode = u.ShieldMode.ToString() + (u.Pose.Move != null ? " (own blow)" : "");
                        if (!shieldTurn.TryGetValue(mode, out var list))
                        {
                            shieldTurn[mode] = list = new List<float>();
                        }

                        list.Add(Vector3.Angle(before, face) / dt);
                        if (!shieldTurn.TryGetValue("all", out var all))
                        {
                            shieldTurn["all"] = all = new List<float>();
                        }

                        all.Add(Vector3.Angle(before, face) / dt);
                        float away = Vector3.Distance(u.Anim.Part(PartId.HandL).position, u.Anim.Part(PartId.Chest).position) / u.Body.SizeFactor;
                        shieldOut.Add(away);
                        if (!shieldAway.TryGetValue(mode, out var awayList))
                        {
                            shieldAway[mode] = awayList = new List<float>();
                        }

                        awayList.Add(away);
                    }

                    lastFace[u] = face;
                }
            }
        };

        try
        {
            FightReport.Play(rigs, seed, 120f, null, out _);
        }
        finally
        {
            FightReport.OnStep = null;
        }

        Console.WriteLine($"weapon wrist bend (the pose; references 23 / 45): guard {Pct(wristGuard, 50):0} / {Pct(wristGuard, 95):0}, blows {Pct(wristBlow, 50):0} / {Pct(wristBlow, 95):0}  ({wristGuard.Count} + {wristBlow.Count} steps)");
        foreach (var pair in shieldTurn)
        {
            Console.WriteLine($"shield face turning, no hit in the last second, {pair.Key}: median {Pct(pair.Value, 50):0} / 95th {Pct(pair.Value, 95):0} / 99th {Pct(pair.Value, 99):0} deg/s ({pair.Value.Count} steps)");
        }

        Console.WriteLine($"shield hand to chest (medium size): median {Pct(shieldOut, 50):0.00} m");
        foreach (var pair in shieldAway)
        {
            Console.WriteLine($"  {pair.Key}: median {Pct(pair.Value, 50):0.00} m, 5-95% {Pct(pair.Value, 5):0.00}-{Pct(pair.Value, 95):0.00} ({pair.Value.Count} steps)");
        }
        return 0;
    }

    /// <summary>The wrist's bend: the hand's turn against the forearm, the roll about the forearm's line left out (degrees).</summary>
    public static float WristBend(Transform forearm, Transform hand)
    {
        Quaternion rel = Quaternion.Inverse(forearm.rotation) * hand.rotation;
        if (rel.w < 0f)
        {
            rel = new Quaternion(-rel.x, -rel.y, -rel.z, -rel.w);
        }

        Vector3 axis = (Quaternion.Inverse(forearm.rotation) * (hand.position - forearm.position)).normalized;
        float along = Vector3.Dot(new Vector3(rel.x, rel.y, rel.z), axis);
        var twist = new Quaternion(axis.x * along, axis.y * along, axis.z * along, rel.w);
        float m = Mathf.Sqrt(twist.x * twist.x + twist.y * twist.y + twist.z * twist.z + twist.w * twist.w);
        if (m < 1e-6f)
        {
            return 180f;
        }

        twist = new Quaternion(twist.x / m, twist.y / m, twist.z / m, twist.w / m);
        Quaternion swing = rel * Quaternion.Inverse(twist);
        return 2f * Mathf.Acos(Mathf.Clamp01(Mathf.Abs(swing.w))) * Mathf.Rad2Deg;
    }

    private static float Pct(List<float> values, float q)
    {
        if (values.Count == 0)
        {
            return float.NaN;
        }

        var v = new List<float>(values);
        v.Sort();
        return v[Mathf.Clamp((int)(q / 100f * (v.Count - 1)), 0, v.Count - 1)];
    }
}
