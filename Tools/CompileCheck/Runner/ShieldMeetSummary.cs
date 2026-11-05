// The plan "Shield and blade contact", step 1: how the blows met a shield-bearer, summed up for the runners.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PhysicsBattler.Locomotion;

internal static class ShieldMeetSummary
{
    /// <summary>Prints one summary of every blow's first meeting with a shield-bearer (none: nothing).</summary>
    public static void Print(IEnumerable<StrikeHits.ShieldMeet> meets)
    {
        var ci = CultureInfo.InvariantCulture;
        var all = meets.ToList();
        if (all.Count == 0)
        {
            return;
        }

        var on = all.Where(m => m.OnBoard).ToList();
        var off = all.Where(m => !m.OnBoard).ToList();
        float Median(IEnumerable<float> xs) { var s = xs.OrderBy(x => x).ToList(); return s.Count == 0 ? 0f : s[s.Count / 2]; }
        Console.WriteLine(string.Format(ci, "The shield met: {0} blows; on the board {1}, off it {2}", all.Count, on.Count, off.Count));
        if (on.Count > 0)
        {
            Console.WriteLine(string.Format(ci, "  on the board: the blade from its centre median {0:0.00} of the radius, most {1:0.00} ({4} on the rim, past 0.8); the face off square median {2:0} deg, most {3:0}",
                Median(on.Select(m => m.Off)), on.Max(m => m.Off), Median(on.Select(m => m.FaceOff)), on.Max(m => m.FaceOff), on.Count(m => m.Off > 0.8f)));
        }

        foreach (var m in off)
        {
            Console.WriteLine(string.Format(ci, "  off the board: {0,-20} in the {1,-10} {2:+0.00;-0.00} m from the chest; the blade {3:0.00} of the radius from the board's centre, its face {4:0} deg off square",
                m.Clip, m.Part, m.Height, m.Off, m.FaceOff));
        }
    }
}
