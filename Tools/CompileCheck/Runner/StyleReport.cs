// The movement styles' measuring tool (the plan "Movement Styles as Offshoots of the Standard Set", steps S0 and
// S1). "style write": each style's carriage is measured on its recordings and written to the profile file. "style":
// the profiles said in plain measures (the lean, the hips' height, where the hands are held), and the round trip
// (test T0): a style we own made again from the plain walk with its manner laid on, set against its recording.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using PhysicsBattler.Locomotion;
using UnityEngine;
using Job = PhysicsBattler.Locomotion.MoveStyle.Job;

internal static class StyleReport
{
    // What each style is measured on: for each job, the recordings (the clip library's names).
    private static readonly (string style, string about, (Job job, string[] clips)[] jobs)[] Sources =
    {
        ("plain", "The relaxed walk, empty hands: what a style's manner is counted from when it is laid over the plain clips.", new[]
        {
            (Job.Stand, new[] { "idle" }), (Job.WalkForward, new[] { "walk_fwd" }), (Job.WalkBack, new[] { "walk_back" }),
            (Job.StepLeft, new[] { "walk_left_step" }), (Job.StepRight, new[] { "walk_right_step" }), (Job.RunForward, new[] { "run_fwd" }),
        }),
        ("guard", "The standard style: the sword and shield's guard.", new[]
        {
            (Job.Stand, new[] { "sns_idle_b" }), (Job.WalkForward, new[] { "sns_walk" }), (Job.WalkBack, new[] { "sns_walk_back" }),
            (Job.StepLeft, new[] { "sns_step_left" }), (Job.StepRight, new[] { "sns_step_right" }), (Job.RunForward, new[] { "sns_run" }),
        }),
        ("sword", "A sword alone, the other hand empty.", new[]
        {
            (Job.Stand, new[] { "sw_idle_b" }), (Job.WalkForward, new[] { "sw_walk" }), (Job.WalkBack, new[] { "sw_walk_back" }),
            (Job.StepLeft, new[] { "sw_step_left" }), (Job.StepRight, new[] { "sw_step_right" }), (Job.RunForward, new[] { "sw_run" }),
        }),
        ("heavy", "A heavy walk, empty hands (one recording).", new[] { (Job.WalkForward, new[] { "walk_fwd_heavy" }) }),
        ("swagger", "A swaggering walk, empty hands (one recording).", new[] { (Job.WalkForward, new[] { "walk_fwd_swagger" }) }),
        ("capoeira", "Capoeira, empty hands: its carriage in the ginga (four takes on the spot).", new[]
        {
            (Job.Stand, new[] { "cap_ginga", "cap_ginga_b", "cap_ginga_c", "cap_ginga_d" }),
        }),
    };

    public static int Run(string[] args)
    {
        LocoLibrary.Load();
        if (args.Contains("write"))
        {
            Write();
            StyleProfile.Load();
        }

        Profiles();
        RoundTrips();
        return 0;
    }

    // ---- Measuring: the profile file -----------------------------------------------------------------

    private static void Write()
    {
        var styles = new List<StyleProfileData>();
        foreach (var (style, about, jobs) in Sources)
        {
            var profile = new StyleProfile(style, about);
            foreach (var (job, names) in jobs)
            {
                var clips = names.Select(LocoLibrary.Get).Where(c => c != null).ToArray();
                if (clips.Length > 0)
                {
                    profile.Set(job, Carriage.Of(clips), string.Join(", ", clips.Select(c => c.Name)));
                }
            }

            styles.Add(profile.ToData());
        }

        var sb = new StringBuilder();
        sb.Append("{\n  \"about\": \"Movement styles' profiles: each style's carriage in each job it was measured in (the plan 'Movement Styles as Offshoots of the Standard Set'). Written by the measuring tool (Tools/CompileCheck: 'style write') from the recordings named in 'from'. Places are leg lengths against the root (right, up, forward; a leg is the thigh and the shin, so the hips of a body standing straight are about 1.16 up), turns are degrees (pitch + forward, yaw + to the right, roll), an arm bone's way is a unit vector on the chest, a hand's turn a quaternion on the chest; the 'Move' numbers are how far each goes about its middle (root mean square).\",\n  \"styles\": [\n");
        for (int i = 0; i < styles.Count; i++)
        {
            var s = styles[i];
            sb.Append("    {\n      \"name\": ").Append(Str(s.name)).Append(",\n      \"about\": ").Append(Str(s.about)).Append(",\n      \"carriages\": [\n");
            for (int j = 0; j < s.carriages.Length; j++)
            {
                var c = s.carriages[j];
                sb.Append("        {\n");
                sb.Append("          \"job\": ").Append(Str(c.job)).Append(", \"from\": ").Append(Str(c.from)).Append(",\n");
                sb.Append("          \"hips\": ").Append(Arr(c.hips, 3)).Append(", \"hipsTurn\": ").Append(Arr(c.hipsTurn, 1)).Append(", \"chestTurn\": ").Append(Arr(c.chestTurn, 1)).Append(", \"headTurn\": ").Append(Arr(c.headTurn, 1)).Append(",\n");
                sb.Append("          \"collarL\": ").Append(Arr(c.collarL, 3)).Append(", \"upperArmL\": ").Append(Arr(c.upperArmL, 3)).Append(", \"forearmL\": ").Append(Arr(c.forearmL, 3)).Append(", \"handL\": ").Append(Arr(c.handL, 3)).Append(",\n");
                sb.Append("          \"collarR\": ").Append(Arr(c.collarR, 3)).Append(", \"upperArmR\": ").Append(Arr(c.upperArmR, 3)).Append(", \"forearmR\": ").Append(Arr(c.forearmR, 3)).Append(", \"handR\": ").Append(Arr(c.handR, 3)).Append(",\n");
                sb.Append("          \"spine\": ").Append(Arr(c.spine, 3)).Append(",\n");
                sb.Append("          \"ankleL\": ").Append(Arr(c.ankleL, 3)).Append(", \"ankleR\": ").Append(Arr(c.ankleR, 3)).Append(",\n");
                sb.Append("          \"hipsMove\": ").Append(Arr(c.hipsMove, 4)).Append(", \"hipsTurnMove\": ").Append(Arr(c.hipsTurnMove, 2)).Append(", \"chestTurnMove\": ").Append(Arr(c.chestTurnMove, 2)).Append(", \"headTurnMove\": ").Append(Arr(c.headTurnMove, 2)).Append(", \"armsMove\": ").Append(Arr(c.armsMove, 2)).Append(", \"handsMove\": ").Append(Arr(c.handsMove, 2)).Append("\n");
                sb.Append("        }").Append(j < s.carriages.Length - 1 ? "," : "").Append("\n");
            }

            sb.Append("      ]\n    }").Append(i < styles.Count - 1 ? "," : "").Append("\n");
        }

        sb.Append("  ]\n}\n");
        string path = Path.Combine(Application.dataPath, StyleProfile.AssetPath);
        File.WriteAllText(path, sb.ToString());
        Console.WriteLine($"written: {path} ({styles.Count} styles, {styles.Sum(s => s.carriages.Length)} carriages)\n");
    }

    private static string Str(string s) => "\"" + (s ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";

    private static string Arr(float[] a, int digits) =>
        "[" + string.Join(", ", (a ?? Array.Empty<float>()).Select(v => (Math.Round(v, digits) + 0.0).ToString(CultureInfo.InvariantCulture))) + "]";

    // ---- The plain measures: what a body does, said as a person would ----------------------------------

    /// <summary>A motion's plain measures, the same as Tools/Mocap/Style/style_measure.py takes of a raw sample.</summary>
    private struct Plain
    {
        public float HipsHeight, Lean, HipsYaw, Twist, Width, Along, HandSpeedL, HandSpeedR, HipsSway, HipsBob;
        public Vector3 HandL, HandR;
    }

    private const float Metre = Locomotor.ClipLeg;

    private static float YawOf(Vector3 across) => Mathf.Atan2(-across.z, across.x) * Mathf.Rad2Deg;

    /// <param name="pose">The body at a time (root frame, leg lengths) and whether each foot is down.</param>
    private static Plain Measure(int frames, float rate, Func<int, Vector3[]> pose, Func<int, int, bool> down)
    {
        var m = BodyMeasures.OfClips;
        var all = new List<Vector3[]>();
        for (int f = 0; f < frames; f++)
        {
            all.Add((Vector3[])pose(f).Clone());
        }

        // (The facing: where the chest faces on average.)
        float facing = all.Average(p => YawOf(p[(int)LocoBone.RightArm] - p[(int)LocoBone.LeftArm]));
        var turn = Quaternion.Euler(0f, -facing, 0f);
        var r = new Plain();
        var ankles = all.Select(p => p[(int)LocoBone.LeftFoot].y).OrderBy(y => y).ToList();
        float standing = m.ThighLen[0] + m.ShinLen[0] + ankles[(int)(0.05f * (ankles.Count - 1))] - m.Hip[0].y;
        var speedL = new List<float>();
        var speedR = new List<float>();
        float width = 0f, along = 0f;
        int both = 0;
        float minX = float.MaxValue, maxX = float.MinValue, minY = float.MaxValue, maxY = float.MinValue;
        for (int f = 0; f < frames; f++)
        {
            var p = all[f];
            Vector3 hips = p[(int)LocoBone.Hips], chest = p[(int)LocoBone.Spine2];
            float hipsYaw = YawOf(p[(int)LocoBone.RightUpLeg] - p[(int)LocoBone.LeftUpLeg]), chestYaw = YawOf(p[(int)LocoBone.RightArm] - p[(int)LocoBone.LeftArm]);
            Vector3 spine = turn * (chest - hips);
            r.HipsHeight += hips.y / standing;
            r.Lean += Mathf.Atan2(spine.z, spine.y) * Mathf.Rad2Deg;
            r.HipsYaw += Mathf.DeltaAngle(facing, hipsYaw);
            r.Twist += Mathf.DeltaAngle(hipsYaw, chestYaw);
            var chestTurn = Quaternion.Euler(0f, -chestYaw, 0f);
            r.HandL += chestTurn * (p[(int)LocoBone.LeftHand] - chest);
            r.HandR += chestTurn * (p[(int)LocoBone.RightHand] - chest);
            Vector3 own = turn * hips;
            minX = Mathf.Min(minX, own.x); maxX = Mathf.Max(maxX, own.x); minY = Mathf.Min(minY, own.y); maxY = Mathf.Max(maxY, own.y);
            if (f > 0)
            {
                var q = all[f - 1];
                speedL.Add(((p[(int)LocoBone.LeftHand] - chest) - (q[(int)LocoBone.LeftHand] - q[(int)LocoBone.Spine2])).magnitude * rate);
                speedR.Add(((p[(int)LocoBone.RightHand] - chest) - (q[(int)LocoBone.RightHand] - q[(int)LocoBone.Spine2])).magnitude * rate);
            }

            if (down(f, 0) && down(f, 1))
            {
                // (The feet against the root's own facing: where he is going, or facing his man.)
                Vector3 apart = p[(int)LocoBone.RightFoot] - p[(int)LocoBone.LeftFoot];
                width += Mathf.Abs(apart.x);
                along += Mathf.Abs(apart.z);
                both++;
            }
        }

        float k = 1f / Mathf.Max(1, frames);
        r.HipsHeight *= k; r.Lean *= k; r.HipsYaw *= k; r.Twist *= k;
        r.HandL *= k * Metre; r.HandR *= k * Metre;
        r.Width = both > 0 ? width / both * Metre : float.NaN;
        r.Along = both > 0 ? along / both * Metre : float.NaN;
        speedL.Sort(); speedR.Sort();
        r.HandSpeedL = speedL.Count > 0 ? speedL[speedL.Count / 2] * Metre : 0f;
        r.HandSpeedR = speedR.Count > 0 ? speedR[speedR.Count / 2] * Metre : 0f;
        r.HipsSway = (maxX - minX) * Metre;
        r.HipsBob = (maxY - minY) * Metre;
        return r;
    }

    private static Plain Measure(LocoClip clip)
    {
        var p = new Vector3[LocoClip.BoneCount];
        return Measure(clip.Frames, clip.Rate, f => { clip.Pose(f / clip.Rate, p, 1f); return p; },
            (f, foot) => (clip.Contact(foot, f / clip.Rate) & LocoContact.Down) != 0);
    }

    private static string Row(string name, Plain r) => string.Format(CultureInfo.InvariantCulture,
        "  {0,-34} hips {1:0.00} high, {2,3:0}° off the chest | lean {3,3:0}° twist {4,3:0}° | feet {5:0.00} across {6:0.00} along | hands L {7,5:0.00} {8,5:0.00} {9,5:0.00}  R {10,5:0.00} {11,5:0.00} {12,5:0.00} | hands go {13:0.00} {14:0.00} m/s | hips range {15:0.00} side {16:0.00} up",
        name, r.HipsHeight, r.HipsYaw, r.Lean, r.Twist, r.Width, r.Along, r.HandL.x, r.HandL.y, r.HandL.z, r.HandR.x, r.HandR.y, r.HandR.z, r.HandSpeedL, r.HandSpeedR, r.HipsSway, r.HipsBob);

    private static void Profiles()
    {
        Console.WriteLine("the recordings, in plain measures (hips: height against standing straight, and turned off where the chest faces; hands: right, up, forward of the chest, metres):");
        foreach (var (style, _, jobs) in Sources)
        {
            foreach (var (job, names) in jobs)
            {
                foreach (string name in names)
                {
                    var clip = LocoLibrary.Get(name);
                    if (clip != null)
                    {
                        Console.WriteLine(Row($"{style} {job} ({name})", Measure(clip)));
                    }
                }
            }
        }

        Console.WriteLine("\nthe profile file:");
        foreach (var profile in StyleProfile.All)
        {
            foreach (var job in profile.Jobs)
            {
                profile.For(job, out var c);
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "  {0,-9} {1,-12} hips {2:0.000} up, turned {3,5:0.0} {4,5:0.0} {5,5:0.0} | chest {6,5:0.0} {7,5:0.0} {8,5:0.0} | head {9,5:0.0} {10,5:0.0} {11,5:0.0} | goes: hips {12:0.000} {13:0.000} {14:0.000}, hips' yaw {15:0.0}°, chest's {16:0.0}°, arms {17:0.0}° {18:0.0}°, hands {19:0.0}° {20:0.0}°   ({21})",
                    profile.Name, job, c.Mid.Pelvis.y, c.Mid.PelvisTurn.x, c.Mid.PelvisTurn.y, c.Mid.PelvisTurn.z, c.Mid.ChestTurn.x, c.Mid.ChestTurn.y, c.Mid.ChestTurn.z, c.Mid.HeadTurn.x, c.Mid.HeadTurn.y, c.Mid.HeadTurn.z,
                    c.HipsMove.x, c.HipsMove.y, c.HipsMove.z, c.HipsTurnMove.y, c.ChestTurnMove.y, c.ArmL, c.ArmR, c.HandL, c.HandR, profile.MeasuredOn(job)));
            }
        }
    }

    // ---- The round trip (test T0) ----------------------------------------------------------------------

    private static readonly (string name, LocoBone[] bones)[] Parts =
    {
        ("hips", new[] { LocoBone.Hips }), ("chest", new[] { LocoBone.Spine2 }), ("head", new[] { LocoBone.Head }),
        ("elbows", new[] { LocoBone.LeftForeArm, LocoBone.RightForeArm }), ("hands", new[] { LocoBone.LeftHand, LocoBone.RightHand }),
        ("knees", new[] { LocoBone.LeftLeg, LocoBone.RightLeg }), ("feet", new[] { LocoBone.LeftFoot, LocoBone.RightFoot }),
    };

    /// <summary>How far apart two step cycles are, part by part, at the same moments of the stride (cm, root mean square).</summary>
    private static string Apart(LocoClip a, LocoClip b)
    {
        const int n = 60;
        Vector3[] pa = new Vector3[LocoClip.BoneCount], pb = new Vector3[LocoClip.BoneCount];
        var sum = new float[Parts.Length];
        for (int i = 0; i < n; i++)
        {
            float phase = i / (float)n;
            a.Pose(a.TimeAtPhase(phase), pa, 1f);
            b.Pose(b.TimeAtPhase(phase), pb, 1f);
            for (int k = 0; k < Parts.Length; k++)
            {
                foreach (var bone in Parts[k].bones)
                {
                    sum[k] += (pa[(int)bone] - pb[(int)bone]).sqrMagnitude / Parts[k].bones.Length;
                }
            }
        }

        return string.Join("  ", Parts.Select((part, k) => string.Format(CultureInfo.InvariantCulture, "{0} {1,4:0.0}", part.name, Mathf.Sqrt(sum[k] / n) * Metre * 100f)));
    }

    /// <summary>How far a clip's own points are from its tracks put together again (cm, root mean square): what the tracks do not hold of it.</summary>
    private static string Remade(LocoClip clip)
    {
        var m = BodyMeasures.OfClips;
        Vector3[] p = new Vector3[LocoClip.BoneCount], q = new Vector3[LocoClip.BoneCount];
        var sum = new float[Parts.Length];
        for (int f = 0; f < clip.Frames; f++)
        {
            clip.Pose(f / clip.Rate, p, 1f);
            m.Compose(m.Decompose(p), q);
            for (int k = 0; k < Parts.Length; k++)
            {
                foreach (var bone in Parts[k].bones)
                {
                    sum[k] += (p[(int)bone] - q[(int)bone]).sqrMagnitude / Parts[k].bones.Length;
                }
            }
        }

        return string.Join("  ", Parts.Select((part, k) => string.Format(CultureInfo.InvariantCulture, "{0} {1,4:0.0}", part.name, Mathf.Sqrt(sum[k] / clip.Frames) * Metre * 100f)));
    }

    private static void RoundTrips()
    {
        Console.WriteLine("\na clip's points against its own tracks put together again (cm): what of the body the tracks do not hold");
        foreach (string name in new[] { "idle", "walk_fwd", "walk_back", "sns_idle_b", "sns_walk", "sns_walk_back", "cap_ginga", "walk_fwd_swagger" })
        {
            var clip = LocoLibrary.Get(name);
            if (clip != null)
            {
                Console.WriteLine($"  {name,-18}: {Remade(clip)}   (phase {(clip.HasPhase ? "yes" : "no")}, travels {clip.Direction:0}° off its facing, {clip.Speed * Metre:0.00} m/s)");
            }
        }


        var plain = StyleProfile.Get("plain");
        if (plain == null)
        {
            Console.WriteLine("\nno profile file: run 'style write' first.");
            return;
        }

        Console.WriteLine("\nthe round trip (T0): a style's recording against the plain clip of the same job, and against that clip with the style's manner laid on (cm apart at the same moments of the stride, root mean square)");
        var trips = new (string style, Job job, string plainClip, string recorded)[]
        {
            ("guard", Job.WalkForward, "walk_fwd", "sns_walk"), ("guard", Job.WalkBack, "walk_back", "sns_walk_back"),
            ("guard", Job.StepLeft, "walk_left_step", "sns_step_left"), ("guard", Job.StepRight, "walk_right_step", "sns_step_right"),
            ("guard", Job.RunForward, "run_fwd", "sns_run"),
            ("sword", Job.WalkForward, "walk_fwd", "sw_walk"), ("sword", Job.WalkBack, "walk_back", "sw_walk_back"),
            ("swagger", Job.WalkForward, "walk_fwd", "walk_fwd_swagger"),
        };
        foreach (var (style, job, plainName, recordedName) in trips)
        {
            var profile = StyleProfile.Get(style);
            LocoClip from = LocoLibrary.Get(plainName), recorded = LocoLibrary.Get(recordedName);
            if (profile == null || from == null || recorded == null || !from.HasPhase || !recorded.HasPhase || !plain.For(job, out var basis) || !profile.For(job, out var manner))
            {
                Console.WriteLine($"  {style} {job}: not there ({plainName}, {recordedName})");
                continue;
            }

            Console.WriteLine($"  {style} {job}: {recordedName} against");
            Console.WriteLine($"    {plainName} as it is             : {Apart(from, recorded)}");
            var carried = StyleManner.Over(from, basis, manner, $"trip_{style}_{job}", 1f, 0f);
            Console.WriteLine($"    its carriage laid on           : {Apart(carried, recorded)}");
            var rebuilt = StyleManner.Over(from, basis, manner, $"trip_{style}_{job}", 1f, 1f);
            Console.WriteLine($"    its carriage and its motion    : {Apart(rebuilt, recorded)}");
            if (job != Job.Stand && profile.Has(Job.Stand) && plain.For(Job.Stand, out var plainStand) && profile.For(Job.Stand, out var stand))
            {
                // (Held out: the carriage it stands with, never measured on this job, laid on the moving clip.)
                var held = StyleManner.Over(from, basis, Shifted(basis, plainStand, stand), $"trip_{style}_{job}_stand", 1f, 0f);
                Console.WriteLine($"    its standing carriage's change : {Apart(held, recorded)}");
            }

            Console.WriteLine(Row("    recorded", Measure(recorded)));
            Console.WriteLine(Row("    plain", Measure(from)));
            Console.WriteLine(Row("    rebuilt", Measure(rebuilt)));
        }
    }

    /// <summary>A job's carriage as the plain clip's own with the change the style makes to another job's (standing) put on it.</summary>
    private static Carriage Shifted(in Carriage basis, in Carriage plainOther, in Carriage styleOther)
    {
        var shifted = basis;
        shifted.Mid = StyleManner.Lay(basis.Mid, plainOther, styleOther, 1f, 0f);
        shifted.Spine = styleOther.Spine;
        return shifted;
    }
}
