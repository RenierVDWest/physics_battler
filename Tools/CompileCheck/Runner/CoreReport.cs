// The strikes' combo cores (StrikeCores): a table, and each strike's tracks about its core for coresheet.py.
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using PhysicsBattler.Locomotion;
using UnityEngine;

internal static class CoreReport
{
    public static int Run(string dir)
    {
        Directory.CreateDirectory(dir);
        LocoLibrary.Load();
        var ci = CultureInfo.InvariantCulture;
        var strikes = LocoLibrary.Clips.Values.Where(c => c.Role == "strike" && c.ContactFrame >= 0).OrderBy(c => c.Name).ToList();
        Console.WriteLine("strike                     contact  turnaround  entry (lead)   exit   tip at entry  turn left  travel left  weight  feet");
        var json = new StringBuilder("{\"strikes\": [");
        bool firstClip = true;
        foreach (var clip in strikes)
        {
            StrikeCores.TryGet(clip, out var core);
            Console.WriteLine(string.Format(ci, "{0,-26} {1,6:0.00} s {2,8:0.00} s {3,6:0.00} s ({4:0.00}) {5,6:0.00} s {6,9:0%} {7,9:+0;-0}° {8,9:0.00} leg  {10,6:0.00}  {9}",
                clip.Name, core.Contact, core.Turnaround, core.Entry, core.Lead, core.Exit, core.EntrySpeedShare, core.TurnLeft, core.TravelLeft, core.Airborne ? "in the air" : core.Leap ? "leap (finisher only)" : "down", core.Weight));

            float from = Mathf.Max(0f, core.Entry - (core.Leap ? 0.15f : 0.4f)), to = Mathf.Min(clip.Duration, core.Exit + 0.25f);
            json.Append(firstClip ? "" : ",").Append("\n{\"name\": \"").Append(clip.Name).Append("\", \"kind\": \"").Append(clip.Kind)
                .Append("\", \"duration\": ").Append(clip.Duration.ToString("0.###", ci))
                .Append(", \"contact\": ").Append(core.Contact.ToString("0.###", ci))
                .Append(", \"entry\": ").Append(core.Entry.ToString("0.###", ci))
                .Append(", \"exit\": ").Append(core.Exit.ToString("0.###", ci))
                .Append(", \"turnaround\": ").Append(core.Turnaround.ToString("0.###", ci))
                .Append(", \"leap\": ").Append(core.Leap ? "true" : "false").Append(", \"frames\": [");
            firstClip = false;
            bool firstFrame = true;
            for (float t = from; t <= to + 1e-4f; t += 1f / 60f)
            {
                json.Append(firstFrame ? "" : ",").Append("{\"t\": ").Append(t.ToString("0.###", ci)).Append(", \"speed\": ")
                    .Append(StrikeCores.TipSpeed(clip, t).ToString("0.##", ci)).Append(", \"p\": [");
                firstFrame = false;
                for (int b = 0; b < LocoClip.BoneCount; b++)
                {
                    Vector3 p = StrikeCores.World(clip, (LocoBone)b, t);
                    json.Append(b == 0 ? "" : ",").Append(string.Format(ci, "[{0:0.###},{1:0.###},{2:0.###}]", p.x, p.y, p.z));
                }

                Vector3 tip = StrikeCores.Tip(clip, t);
                json.Append(string.Format(ci, "], \"tip\": [{0:0.###},{1:0.###},{2:0.###}]}}", tip.x, tip.y, tip.z));
            }

            json.Append("]}");
        }

        if (Environment.GetEnvironmentVariable("BEARINGS") != null)
        {
            foreach (var clip in strikes)
            {
                StrikeCores.TryGet(clip, out var core);
                float open = core.Leap ? core.Entry : Mathf.Max(0f, core.Contact - StrikeCombos.OpenLead);
                Console.WriteLine(string.Format(ci, "{0,-22} bearing from the opener's entry {1:+0;-0}, from the core's entry {2:+0;-0}, at contact {3:+0;-0}", clip.Name,
                    StrikeCores.Bearing(clip, open), StrikeCores.Bearing(clip, core.Entry), StrikeCores.Bearing(clip, core.Contact)));
            }
        }

        json.Append("\n]}\n");
        File.WriteAllText(Path.Combine(dir, "cores.json"), json.ToString());
        Console.WriteLine($"Wrote {Path.Combine(dir, "cores.json")} ({strikes.Count} strikes).");
        return 0;
    }
}
