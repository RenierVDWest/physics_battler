// The recorded strikes' blades (BladeFlow, the combos plan's step 1): each cut's corners and carry-through, and how
// well each pair of cuts joins.
//   flow            the Medium's cuts: the map, and every pair's join
//   flow all        the Large's clubs as well
//   flow raw        each cut's hand and blade at its marks; "raw speeds": the point's speed frame by frame
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PhysicsBattler.Locomotion;
using UnityEngine;

internal static class FlowReport
{
    /// <summary>Where a performer went straight on from one cut into the next: slices of one take, cut at a shared frame (the bake's table, Tools/Mocap/Combat/combat_bake.py, STRIKES).</summary>
    private static readonly (string, string)[] Own =
    {
        ("sns_combo_forehand", "sns_combo_backhand"), ("sns_combo_backhand", "sns_combo_chop"),
        ("lg_slash_a", "lg_slash_b"), ("lg_slash_b", "lg_slash_c"), ("lg_club_a", "lg_club_b"), ("lg_club_b", "lg_club_c"),
    };

    public static int Run(string[] args)
    {
        LocoLibrary.Load();
        var ci = CultureInfo.InvariantCulture;
        bool all = args.Contains("all"), raw = args.Contains("raw");
        var strikes = LocoLibrary.Clips.Values.Where(c => c.Role == "strike" && c.ContactFrame >= 0 && (all || !c.Name.StartsWith("lg_"))).OrderBy(c => c.Name).ToList();
        var flows = strikes.Select(BladeFlow.Of).ToList();
        if (raw)
        {
            foreach (var f in flows)
            {
                var sc = f.Score;
                foreach (var (what, at) in new[] { ("start", 0), ("drive", sc.Drive), ("enter", f.Enter), ("hit", f.Hit), ("follow", sc.Follow), ("leave", f.Leave), ("end", f.Count - 1) })
                {
                    int i = Mathf.Clamp(at, 0, f.Count - 1);
                    Console.WriteLine(string.Format(ci, "AT {0,-20} {1,-6} f{2,2} {3:0.00}s {4,4:0.0} m/s  hand {5} way {6} height {7:0.00} | hips: grip {8} tip {9} vel {10} | turned {11,5:+0;-0} facing {12,5:+0;-0}  {13}",
                        f.Name, what, i, i / f.Rate, f.Speed[i], V(f.Hand[i]), V(f.Way[i]), f.Grip[i].y, V(f.Grip[i]), V(f.Tip[i]), V(f.Velocity[i].normalized), f.Turn[i] - f.Turn[0], Mathf.DeltaAngle(0f, f.Facing(i)), BladeFlow.Say(f.CornerAt(i))));
                }
            }

            if (args.Contains("speeds"))
            {
                foreach (var f in flows)
                {
                    Console.WriteLine(f.Name + " speeds: " + string.Join(" ", Enumerable.Range(0, f.Count).Select(i => (i == f.Hit ? "[" : i == f.Enter ? "<" : i == f.Leave ? ">" : "") + f.Speed[i].ToString("0.0", ci))));
                }
            }

            return 0;
        }

        Console.WriteLine("THE MAP: where each recorded cut's blade starts and ends (BladeFlow; a Medium's hold of its 0.9 m sword on the recorded hand).");
        Console.WriteLine("cut                    from           to             lands   fastest  run-up (top of the swing to the landing)  carried through (landing to the blade's turn)   slowest there");
        foreach (var f in flows)
        {
            Console.WriteLine(string.Format(ci, "{0,-22} {1,-14} {2,-14} {3,5:0.00} s {4,5:0.0} m/s  {5,5:0.00} s {6,5:0.00} m {7,4:0}°                 {8,5:0.00} s {9,5:0.00} m {10,4:0}°                       {11,5:0.0} m/s",
                f.Name, BladeFlow.Say(f.From), BladeFlow.Say(f.To), f.HitAt, f.Peak, f.HitAt - f.EnterAt, f.RunUp, f.RunUpSwing, f.LeaveAt - f.HitAt, f.Carry, f.CarrySwing, f.Speed[f.Leave]));
        }

        var joins = new List<BladeJoin>();
        var soon = new Dictionary<(BladeFlow, BladeFlow), BladeJoin>();
        foreach (var a in flows)
        {
            foreach (var b in flows)
            {
                joins.Add(BladeFlow.Join(a, b));
                soon[(a, b)] = BladeFlow.Join(a, b, BladeFlow.PlanBend);
            }
        }

        Console.WriteLine();
        Console.WriteLine("THE JOINS: each cut into each other, left and entered where there is least to make up; the least first. \"Soonest\": the soonest second landing with a bend the plan allows (0.15 s).");
        Console.WriteLine("first                  next                   bend     hit to hit  (left after, entered before)   point gap  hand gap  blades   speed out > in (differ)   path turns  body turned  carried  slowest   corners                          verdict             soonest (its bend)");
        // The performers' own joins: the first slice's last frame into the next slice's first. One counts as a join
        // only if he went straight on: he left the first cut within the search's own time of its blade's turn.
        int place = 0, flowing = 0;
        float worstOwn = 0f;
        var ownTold = new List<string>();
        var ordered = joins.OrderBy(j => j.Bend).ThenBy(j => j.HitToHit).ToList();
        foreach (var j in ordered)
        {
            place++;
            bool own = Own.Contains((j.First.Name, j.Next.Name));
            string mark = "";
            if (own)
            {
                var his = BladeFlow.At(j.First, j.Next, j.First.Count - 1, 0);
                bool wentOn = j.First.Count - 1 <= j.First.Leave + Mathf.RoundToInt(BladeFlow.LeavesWithin * j.First.Rate);
                flowing += wentOn ? 1 : 0;
                worstOwn = wentOn ? Mathf.Max(worstOwn, j.Bend) : worstOwn;
                mark = wentOn ? "   <- THE PERFORMER'S OWN" : "   (the performer's own, but he did not go straight on)";
                ownTold.Add(string.Format(ci, "{0} into {1}: {2}he left {3:0.00} s after the landing, his blade's turn {4:0.00} s after it; his own join's bend {5:0.000} s, hit to hit {6:0.00} s; the search's: place {7} of {8}, bend {9:0.000} s, hit to hit {10:0.00} s",
                    Short(j.First.Name), Short(j.Next.Name), wentOn ? "" : "NOT STRAIGHT ON, ", his.After, (j.First.Leave - j.First.Hit) / j.First.Rate, his.Bend, his.HitToHit, place, joins.Count, j.Bend, j.HitToHit));
            }

            var q = soon[(j.First, j.Next)];
            Console.WriteLine(string.Format(ci, "{0,-22} {1,-22} {2,5:0.000} s  {3,5:0.00} s     ({4,4:0.00} s, {5,4:0.00} s)              {6,5:0.00} m   {7,5:0.00} m  {8,4:0}°   {9,5:0.0} > {10,4:0.0} ({11,4:0.0}) m/s   {12,5:0}°      {13,5:+0;-0}°     {14,5:0.00} m  {15,4:0.0} m/s  {16,-32} {17,-19} {18}{19}",
                Short(j.First.Name), Short(j.Next.Name), j.Bend, j.HitToHit, j.After, j.Before, j.Gap, j.HandGap, j.Angle, j.SpeedOut, j.SpeedIn, j.SpeedOff, j.PathTurn, j.TurnOff, j.Carry, j.Slowest,
                BladeFlow.Say(j.First.CornerAt(j.Leave)) + " > " + BladeFlow.Say(j.Next.CornerAt(j.Enter)), Verdict(j),
                q.Bend <= BladeFlow.PlanBend ? string.Format(ci, "{0:0.00} s ({1:0.00})", q.HitToHit, q.Bend) : "none", mark));
        }

        Console.WriteLine();
        Console.WriteLine("BEND (seconds), rows: first; columns: next");
        Console.WriteLine("".PadRight(18) + string.Join("", flows.Select(f => Short(f.Name).PadLeft(16).Substring(0, 16))));
        foreach (var a in flows)
        {
            Console.WriteLine(Short(a.Name).PadRight(18) + string.Join("", flows.Select(b => joins.First(j => j.First == a && j.Next == b).Bend.ToString("0.00", ci).PadLeft(16))));
        }

        Console.WriteLine();
        Console.WriteLine("HIT TO HIT (seconds, at the recorded pace), joined where there is least to make up");
        Console.WriteLine("".PadRight(18) + string.Join("", flows.Select(f => Short(f.Name).PadLeft(16).Substring(0, 16))));
        foreach (var a in flows)
        {
            Console.WriteLine(Short(a.Name).PadRight(18) + string.Join("", flows.Select(b => joins.First(j => j.First == a && j.Next == b).HitToHit.ToString("0.00", ci).PadLeft(16))));
        }

        Console.WriteLine();
        Console.WriteLine("SOONEST HIT TO HIT (seconds) with a bend the plan allows (0.15 s); -: no such join");
        Console.WriteLine("".PadRight(18) + string.Join("", flows.Select(f => Short(f.Name).PadLeft(16).Substring(0, 16))));
        foreach (var a in flows)
        {
            Console.WriteLine(Short(a.Name).PadRight(18) + string.Join("", flows.Select(b => (soon[(a, b)].Bend <= BladeFlow.PlanBend ? soon[(a, b)].HitToHit.ToString("0.00", ci) : "-").PadLeft(16))));
        }

        Console.WriteLine();
        // The check: every join where a performer went straight on is among the best, with none but such joins before it.
        int before = ordered.Count(j => j.Bend < worstOwn - 1e-6f && !Own.Contains((j.First.Name, j.Next.Name)));
        Console.WriteLine("THE PERFORMERS' OWN JOINS (slices of one take, cut at a shared frame):");
        foreach (string told in ownTold)
        {
            Console.WriteLine("  " + told);
        }

        Console.WriteLine(ownTold.Count == 0 ? "FLOW: nothing to check against" : flowing == 0 ? "FLOW: no performer went straight on among these cuts"
            : before == 0 ? $"FLOW OK: the {flowing} join{(flowing == 1 ? "" : "s")} where a performer went straight on {(flowing == 1 ? "is" : "are")} the best of {joins.Count}"
            : $"FLOW: {before} OTHER JOIN{(before == 1 ? "" : "S")} COME OUT BETTER THAN A PERFORMER'S OWN");
        return 0;
    }

    /// <summary>A join in words, by how much there is to make up.</summary>
    private static string Verdict(BladeJoin j)
    {
        return j.Bend <= 0.06f ? "joins as it is" : j.Bend <= 0.15f ? "joins with a bend" : j.Bend <= 0.30f ? "needs a loop" : "winds up again";
    }

    private static string Short(string name) => name.Replace("sns_", "");

    private static string V(Vector3 v) => string.Format(CultureInfo.InvariantCulture, "({0,5:0.00},{1,5:0.00},{2,5:0.00})", v.x, v.y, v.z);
}
