using System;
using System.Collections.Generic;
using System.Linq;
using PhysicsBattler.Locomotion;
using PhysicsBattler.Motion;
using UnityEngine;

/// <summary>
/// The recorded clips' own legs, read as a body's legs (<see cref="LegLimits"/>): the range each reading takes over
/// every frame of every clip, and how much of it is outside the model's range. The recorded motion is a person's:
/// what it does, a body can do, so the model's ranges must hold all of it (the plan "Grounded legs", step 0).
/// </summary>
public static class LegRangeReport
{
    public static int Run()
    {
        LocoLibrary.Load();
        var all = new Dictionary<string, List<LegReading>>();
        var worst = new List<(float past, string at)>();
        // (The thigh's place: the highest it is raised, by which way, over all the clips.)
        var highest = new float[24];
        foreach (var pair in LocoLibrary.Clips.OrderBy(p => p.Key))
        {
            var clip = pair.Value;
            string set = pair.Key.Contains("_") ? pair.Key.Substring(0, pair.Key.IndexOf('_')) : "walks";
            float clipWorst = 0f;
            string clipAt = "";
            for (int f = 0; f < clip.Frames; f++)
            {
                float t = f / clip.Rate;
                var p = new Vector3[LocoClip.BoneCount];
                for (int b = 0; b < p.Length; b++)
                {
                    p[b] = clip.Point((LocoBone)b, t);
                }

                for (int side = 0; side < 2; side++)
                {
                    var a = LegLimits.ReadClip(p, (int)LocoBone.Hips, (int)LocoBone.Spine, (int)LocoBone.LeftUpLeg, (int)LocoBone.RightUpLeg, side == 0);
                    string key = set + (side == 0 ? " left" : " right");
                    if (!all.TryGetValue(key, out var list))
                    {
                        all[key] = list = new List<LegReading>();
                    }

                    list.Add(a);
                    if (a.Raised > 15f)
                    {
                        int slot = Mathf.Clamp((int)((Mathf.DeltaAngle(0f, a.Toward) + 180f) / 15f), 0, 23);
                        highest[slot] = Mathf.Max(highest[slot], a.Raised);
                    }

                    if (a.Worst > clipWorst)
                    {
                        clipWorst = a.Worst;
                        clipAt = $"{pair.Key} frame {f} {(side == 0 ? "left" : "right")}: raised {a.Raised:0} toward {a.Toward:0} (highest there {LegLimits.Highest(a.Toward):0}), the knee folding {a.Turn:0} off straight back, bent {a.Bend:0}";
                    }
                }
            }

            worst.Add((clipWorst, clipAt));
        }

        string Range(IEnumerable<float> values)
        {
            var v = values.OrderBy(x => x).ToList();
            return v.Count == 0 ? "-" : $"{v[0],5:0} {v[(int)(0.01f * (v.Count - 1))],5:0} {v[v.Count / 2],5:0} {v[(int)(0.99f * (v.Count - 1))],5:0} {v[v.Count - 1],5:0}";
        }

        foreach (var e in all.OrderBy(e => e.Key))
        {
            var l = e.Value;
            var folded = l.Where(a => a.Folded > 0.5f).ToList();
            Console.WriteLine($"{e.Key} leg, {l.Count} frames (least, 1%, middle, 99%, most):");
            Console.WriteLine($"    raised           {Range(l.Select(a => a.Raised))}     past its place over 10°: {100f * l.Count(a => a.PastPlace > 10f) / l.Count:0.0}% (most {l.Max(a => a.PastPlace):0}°)");
            Console.WriteLine($"    the knee's fold  {Range(folded.Select(a => a.Turn))}     past over 10°: {100f * l.Count(a => a.PastTurn > 10f) / l.Count:0.0}% (most {l.Max(a => a.PastTurn):0}°)");
            Console.WriteLine($"    the knee's bend  {Range(l.Select(a => a.Bend))}     past over 10°: {100f * l.Count(a => a.PastBend > 10f) / l.Count:0.0}%");
            Console.WriteLine($"    any past over 10°: {100f * l.Count(a => a.Worst > 10f) / l.Count:0.0}%, over 30°: {100f * l.Count(a => a.Worst > 30f) / l.Count:0.0}%");
        }

        Console.WriteLine("the highest a thigh is raised in any clip, by which way (0 forward, 90 out to its own side, 180 back, negative across), against the range:");
        for (int slot = 0; slot < 24; slot++)
        {
            if (highest[slot] > 0f)
            {
                float toward = -180f + 15f * slot + 7.5f;
                Console.WriteLine($"    toward {toward,6:0}: {highest[slot],4:0}°   (the range there {LegLimits.Highest(toward):0})");
            }
        }

        Console.WriteLine("the clips furthest past:");
        foreach (var w in worst.OrderByDescending(w => w.past).Take(12))
        {
            Console.WriteLine($"    {w.past,3:0}°  {w.at}");
        }

        return 0;
    }
}
