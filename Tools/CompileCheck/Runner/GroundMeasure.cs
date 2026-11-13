using System;
using System.Collections.Generic;
using System.Linq;
using PhysicsBattler.Locomotion;
using PhysicsBattler.Units;
using UnityEngine;

/// <summary>
/// The legs' opinion over a fight (the plan "Grounded legs", step 0: the table "What the playtest logs show", made by
/// the report): how long the fighters stand, how long a foot is planted, how long both feet are down, how long swings
/// last and how far the foot goes in them against the body, how much a foot on the ground slides, and whether the
/// body's weight, and the place a foot is needed, are over what the feet hold. Read from each fighter's
/// <see cref="Grounding"/>; it changes nothing.
/// </summary>
public sealed class GroundMeasure
{
    private sealed class Track
    {
        public readonly bool[] Up = new bool[2];
        public readonly float[] UpSince = new float[2], SlideRun = new float[2];
        public readonly Vector3[] FootFrom = new Vector3[2], BodyFrom = new Vector3[2];
        public readonly int[] SlideMode = new int[2];
        public bool Seen;
    }

    private readonly List<UnitController> units;
    private readonly Dictionary<UnitController, Track> tracks = new Dictionary<UnitController, Track>();
    private float clock;

    private int steps, both, one, none;
    private readonly int[] byMode = new int[3], bothByMode = new int[3];
    private int footSteps;
    private readonly int[] states = new int[5];
    private readonly int[,] statesByMode = new int[3, 5];
    private float slidMetres;
    private readonly List<(float length, string who, float at, int mode)> slides = new List<(float, string, float, int)>();
    private readonly List<(float lasted, float foot, float body, int mode)> swings = new List<(float, float, float, int)>();
    private readonly List<float> oneFootOut = new List<float>(), standingOut = new List<float>();
    private readonly List<float>[] neededOut = { new List<float>(), new List<float>(), new List<float>() };

    public GroundMeasure(List<UnitController> units)
    {
        this.units = units;
    }

    public void Step(float dt)
    {
        clock += dt;
        foreach (var u in units)
        {
            var clip = u.Clip;
            if (clip == null)
            {
                continue;
            }

            if (!tracks.TryGetValue(u, out var t))
            {
                tracks[u] = t = new Track();
            }

            var g = clip.Ground;
            // (Not its first second: it is put on its tile, and turned to its enemy, at once.)
            if (!u.IsAlive || clip.Fallen || !clip.Snapped || !g.Known || clock < 1f)
            {
                t.Seen = false;
                continue;
            }

            int mode = Mathf.Clamp((int)clip.Loco.Current, 0, 2);
            float size = clip.Size;
            steps++;
            byMode[mode]++;
            int down = g.FeetDown;
            if (down == 2)
            {
                both++;
                bothByMode[mode]++;
            }
            else if (down == 1)
            {
                one++;
            }
            else
            {
                none++;
            }

            Vector3 body = new Vector3(g.Weight.x, 0f, g.Weight.z);
            float speed = new Vector2(g.WeightSpeed.x, g.WeightSpeed.z).magnitude;
            for (int f = 0; f < 2; f++)
            {
                var foot = g.Feet[f];
                if (float.IsNaN(foot.Lowest))
                {
                    continue;
                }

                footSteps++;
                states[(int)foot.State]++;
                statesByMode[mode, (int)foot.State]++;
                // (Slides: each run of a foot on the ground and moving, its length.)
                if (foot.State == Grounding.FootState.Sliding)
                {
                    float moved = foot.Speed * dt;
                    slidMetres += moved;
                    if (t.SlideRun[f] <= 0f)
                    {
                        t.SlideMode[f] = mode;
                    }

                    t.SlideRun[f] += moved;
                }
                else if (t.SlideRun[f] > 0f)
                {
                    slides.Add((t.SlideRun[f], u.name, clock, t.SlideMode[f]));
                    t.SlideRun[f] = 0f;
                }

                // (Swings: from leaving the ground to touching it again.)
                Vector3 at = new Vector3(0.5f * (foot.Heel.x + foot.Toe.x), 0f, 0.5f * (foot.Heel.z + foot.Toe.z));
                if (!foot.Touching)
                {
                    if (!t.Up[f] && t.Seen)
                    {
                        t.Up[f] = true;
                        t.UpSince[f] = clock;
                        t.FootFrom[f] = at;
                        t.BodyFrom[f] = body;
                    }
                }
                else
                {
                    if (t.Up[f])
                    {
                        swings.Add((clock - t.UpSince[f], (at - t.FootFrom[f]).magnitude, (body - t.BodyFrom[f]).magnitude, mode));
                    }

                    t.Up[f] = false;
                }
            }

            // (The weight and the place a foot is needed, against the support.)
            if (down == 1 && mode != 2 && speed < 0.3f && !float.IsNaN(g.WeightOut))
            {
                oneFootOut.Add(g.WeightOut / size);
            }

            if (down == 2 && speed < 0.15f && !float.IsNaN(g.WeightOut))
            {
                standingOut.Add(g.WeightOut / size);
            }

            if (down == 2 && !float.IsNaN(g.NeededOut))
            {
                neededOut[mode].Add(g.NeededOut / size);
            }

            t.Seen = true;
        }
    }

    private static float At(List<float> sorted, float share) => sorted.Count == 0 ? 0f : sorted[Mathf.Clamp((int)(share * sorted.Count), 0, sorted.Count - 1)];

    private static string Pct(int part, int of) => of > 0 ? $"{100f * part / of:0}%" : "-";

    public string Report()
    {
        var sb = new System.Text.StringBuilder();
        if (steps == 0)
        {
            return "";
        }

        float seconds = steps / 60f, minutes = seconds / 60f;
        sb.AppendLine($"    the legs' opinion (the plan \"Grounded legs\", step 0; {seconds:0} unit-seconds on their feet; a sole's end within 2 cm of the ground is on it):");
        sb.AppendLine($"      a fighter is standing (not walking, not in a clip) {Pct(byMode[0], steps)} of the time, walking {Pct(byMode[1], steps)}, in a clip (a strike, a reaction, a turn) {Pct(byMode[2], steps)}");
        sb.AppendLine($"      a foot is planted (on the ground, not moving) {Pct(states[0], footSteps)} of the time, on the ground and moving over 10 cm/s {Pct(states[1], footSteps)}, landing {Pct(states[2], footSteps)}, in the air under 9 cm {Pct(states[3], footSteps)}, in the air higher {Pct(states[4], footSteps)}");
        string[] names = { "standing", "walking", "in a clip" };
        for (int m = 0; m < 3; m++)
        {
            int n = 0;
            for (int s = 0; s < 5; s++)
            {
                n += statesByMode[m, s];
            }

            if (n > 0)
            {
                sb.AppendLine($"        {names[m]}: a foot planted {Pct(statesByMode[m, 0], n)}, sliding {Pct(statesByMode[m, 1], n)}, in the air under 9 cm {Pct(statesByMode[m, 3], n)}, higher {Pct(statesByMode[m, 4], n)}; both feet on the ground {Pct(bothByMode[m], byMode[m])} of the time");
            }
        }

        sb.AppendLine($"      both feet are on the ground {Pct(both, steps)} of the time, one {Pct(one, steps)}, neither {Pct(none, steps)}; while walking, a foot is in the air {Pct(byMode[1] - bothByMode[1], byMode[1])} of the time");
        if (swings.Count > 0)
        {
            var lasted = swings.Select(s => s.lasted).OrderBy(x => x).ToList();
            var longOnes = swings.Where(s => s.lasted > 0.6f).ToList();
            float airTime = swings.Sum(s => s.lasted);
            sb.AppendLine($"      swings (a foot off the ground until it is on it again): {swings.Count} ({swings.Count / seconds:0.0} a second for each fighter), lasting {At(lasted, 0.5f):0.00} s at the middle, a quarter over {At(lasted, 0.75f):0.00} s, a tenth over {At(lasted, 0.9f):0.00} s");
            if (longOnes.Count > 0)
            {
                var foot = longOnes.Select(s => s.foot).OrderBy(x => x).ToList();
                var body = longOnes.Select(s => s.body).OrderBy(x => x).ToList();
                sb.AppendLine($"        swings lasting over 0.6 s: {longOnes.Count} ({100f * longOnes.Count / swings.Count:0}% of them), {100f * longOnes.Sum(s => s.lasted) / airTime:0}% of all the time feet are in the air; in them the foot goes {100f * At(foot, 0.5f):0} cm while the body goes {100f * At(body, 0.5f):0} cm (at the middle)");
            }
            else
            {
                sb.AppendLine("        swings lasting over 0.6 s: none");
            }
        }

        var over5 = slides.Where(s => s.length > 0.05f).OrderByDescending(s => s.length).ToList();
        sb.AppendLine($"      a foot on the ground and moving over 10 cm/s: {Pct(states[1], footSteps)} of the time, {slidMetres / Mathf.Max(1e-3f, minutes):0.0} m a minute for each fighter; slides over 5 cm: {over5.Count} ({over5.Count / Mathf.Max(1e-3f, minutes):0} a minute for each fighter)"
            + (over5.Count > 0 ? $", the worst {100f * over5[0].length:0} cm ({over5[0].who} at {over5[0].at:0.0} s, {names[over5[0].mode]}); by what was playing: {string.Join(", ", over5.GroupBy(s => s.mode).OrderBy(x => x.Key).Select(x => $"{names[x.Key]} {x.Count()}"))}" : ""));
        if (oneFootOut.Count > 0)
        {
            oneFootOut.Sort();
            sb.AppendLine($"      on one foot, standing or walking, the body moving under 0.3 m/s ({oneFootOut.Count / 60f:0} unit-seconds): the weight is outside that foot by {100f * At(oneFootOut, 0.5f):0} cm at the middle, a quarter of the time over {100f * At(oneFootOut, 0.75f):0} cm");
        }

        if (standingOut.Count > 0)
        {
            standingOut.Sort();
            sb.AppendLine($"      still on both feet (the weight moving under 0.15 m/s; {standingOut.Count / 60f:0} unit-seconds): the weight is outside the support {100f * standingOut.Count(x => x > 0f) / standingOut.Count:0}% of the time, by over 3 cm {100f * standingOut.Count(x => x > 0.03f) / standingOut.Count:0}%, the worst {100f * standingOut[standingOut.Count - 1]:0} cm");
        }

        for (int m = 0; m < 3; m++)
        {
            var l = neededOut[m];
            if (l.Count > 0)
            {
                sb.AppendLine($"      both feet down, {names[m]} ({l.Count / 60f:0} unit-seconds): the place a foot is needed to stop the body is outside the support {100f * l.Count(x => x > 0f) / l.Count:0}% of the time, by over 10 cm {100f * l.Count(x => x > 0.10f) / l.Count:0}%");
            }
        }

        return sb.ToString();
    }
}
