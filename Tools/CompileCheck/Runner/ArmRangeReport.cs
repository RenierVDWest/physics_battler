using System;
using System.Collections.Generic;
using System.Linq;
using PhysicsBattler.Locomotion;
using PhysicsBattler.Motion;
using UnityEngine;

/// <summary>
/// The recorded clips' own arms, read as a body's arms (<see cref="ArmLimits"/>): the range each reading takes over
/// every frame of every clip, and how much of it is outside the model's range. The recorded motion is a person's:
/// what it does, a body can do, so the model's ranges must hold all of it.
/// </summary>
public static class ArmRangeReport
{
    public static int Run()
    {
        LocoLibrary.Load();
        var all = new Dictionary<string, List<ArmReading>>();
        var worst = new List<(float past, string at)>();
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
                    var a = LimitMeasure.ReadClipArm(p, side == 0);
                    string key = set + (side == 0 ? " left" : " right");
                    if (!all.TryGetValue(key, out var list))
                    {
                        all[key] = list = new List<ArmReading>();
                    }

                    list.Add(a);
                    if (a.Worst > clipWorst)
                    {
                        clipWorst = a.Worst;
                        clipAt = $"{pair.Key} frame {f} {(side == 0 ? "left" : "right")}: raised {a.Raised:0} toward {a.Toward:0}, turned {a.Turn:0}, bent {a.Bend:0}, rolled {a.Roll:0}, wrist {a.Flex:0}/{a.Thumbward:0}  (past: place {a.PastReach:0}, turn {a.PastTurn:0}, bend {a.PastBend:0}, roll {a.PastRoll:0}, wrist {a.PastWrist:0})";
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
            Console.WriteLine($"{e.Key} arm, {l.Count} frames (least, 1%, middle, 99%, most):");
            Console.WriteLine($"    raised       {Range(l.Select(a => a.Raised))}     past its place over 10°: {100f * l.Count(a => a.PastReach > 10f) / l.Count:0.0}% (most {l.Max(a => a.PastReach):0}°)");
            Console.WriteLine($"    turn         {Range(folded.Select(a => a.Turn))}     past over 10°: {100f * l.Count(a => a.PastTurn > 10f) / l.Count:0.0}% (most {l.Max(a => a.PastTurn):0}°)");
            Console.WriteLine($"    bend         {Range(l.Select(a => a.Bend))}     past over 10°: {100f * l.Count(a => a.PastBend > 10f) / l.Count:0.0}%");
            Console.WriteLine($"    roll         {Range(folded.Select(a => a.Roll))}     past over 10°: {100f * l.Count(a => a.PastRoll > 10f) / l.Count:0.0}% (most {l.Max(a => a.PastRoll):0}°)");
            Console.WriteLine($"    wrist flex   {Range(l.Select(a => a.Flex))}");
            Console.WriteLine($"    to the thumb {Range(l.Select(a => a.Thumbward))}     wrist past over 10°: {100f * l.Count(a => a.PastWrist > 10f) / l.Count:0.0}% (most {l.Max(a => a.PastWrist):0}°)");
            Console.WriteLine($"    any past over 10°: {100f * l.Count(a => a.Worst > 10f) / l.Count:0.0}%, over 30°: {100f * l.Count(a => a.Worst > 30f) / l.Count:0.0}%");
        }

        Console.WriteLine("the clips furthest past:");
        foreach (var w in worst.OrderByDescending(w => w.past).Take(12))
        {
            Console.WriteLine($"    {w.past,3:0}°  {w.at}");
        }

        return 0;
    }
}
