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
        ("capoeira", "Capoeira, empty hands: its carriage in the ginga (four takes on the spot) and in the ginga travelling forward and back.", new[]
        {
            (Job.Stand, new[] { "cap_ginga", "cap_ginga_b", "cap_ginga_c", "cap_ginga_d" }),
            (Job.WalkForward, new[] { "cap_ginga_fwd" }), (Job.WalkBack, new[] { "cap_ginga_back" }),
        }),
    };

    // The ruler clips: recordings that are measured and never played (Tools/Mocap/Style/style_bake.py), beside the game's own.
    private static Dictionary<string, LocoClip> ruler;

    private static LocoClip Clip(string name)
    {
        var clip = LocoLibrary.Get(name);
        if (clip != null)
        {
            return clip;
        }

        if (ruler == null)
        {
            ruler = new Dictionary<string, LocoClip>();
            string path = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Tools", "Mocap", "Style", "RulerClips.json"));
            if (File.Exists(path))
            {
                foreach (var d in JsonUtility.FromJson<LocoClipFile>(File.ReadAllText(path)).clips)
                {
                    ruler[d.name] = new LocoClip(d);
                }
            }
        }

        return ruler.TryGetValue(name, out clip) ? clip : null;
    }

    public static int Run(string[] args)
    {
        LocoLibrary.Load();
        if (args.Contains("write"))
        {
            Write();
            StyleProfile.Load();
        }

        if (args.Contains("steps"))
        {
            Steps();
            return 0;
        }

        if (args.Contains("made"))
        {
            Made();
            return 0;
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
                var clips = names.Select(Clip).Where(c => c != null).ToArray();
                if (clips.Length > 0)
                {
                    profile.Set(job, Carriage.Of(clips), string.Join(", ", clips.Select(c => c.Name)));
                }
            }

            if (style == "capoeira")
            {
                // The ginga's two sides: the pose it rests in with the right foot back, and with the left (both feet down, one well behind the other).
                var gingas = new[] { "cap_ginga", "cap_ginga_b", "cap_ginga_c", "cap_ginga_d" }.Select(Clip).Where(c => c != null).ToArray();
                var q = new Vector3[LocoClip.BoneCount];
                bool Rests(LocoClip clip, int f, bool rightBack)
                {
                    float time = f / clip.Rate;
                    if ((clip.Contact(0, time) & LocoContact.Down) == 0 || (clip.Contact(1, time) & LocoContact.Down) == 0)
                    {
                        return false;
                    }

                    clip.Pose(time, q, 1f);
                    float behind = q[(int)LocoBone.LeftFoot].z - q[(int)LocoBone.RightFoot].z;
                    return rightBack ? behind > 0.45f : behind < -0.45f;
                }

                if (gingas.Length > 0)
                {
                    profile.SetPose("GingaRightBack", Carriage.Of(gingas, (clip, f) => Rests(clip, f, true)), string.Join(", ", gingas.Select(c => c.Name)));
                    profile.SetPose("GingaLeftBack", Carriage.Of(gingas, (clip, f) => Rests(clip, f, false)), string.Join(", ", gingas.Select(c => c.Name)));
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
            WriteCarriages(sb, s.carriages);
            if (s.poses != null && s.poses.Length > 0)
            {
                sb.Append("      ],\n      \"poses\": [\n");
                WriteCarriages(sb, s.poses);
            }

            sb.Append("      ]\n    }").Append(i < styles.Count - 1 ? "," : "").Append("\n");
        }

        sb.Append("  ]\n}\n");
        string path = Path.Combine(Application.dataPath, StyleProfile.AssetPath);
        File.WriteAllText(path, sb.ToString());
        Console.WriteLine($"written: {path} ({styles.Count} styles, {styles.Sum(s => s.carriages.Length)} carriages)\n");
    }

    private static void WriteCarriages(StringBuilder sb, StyleCarriageData[] carriages)
    {
            for (int j = 0; j < carriages.Length; j++)
            {
                var c = carriages[j];
                sb.Append("        {\n");
                sb.Append("          \"job\": ").Append(Str(c.job)).Append(", \"from\": ").Append(Str(c.from)).Append(",\n");
                sb.Append("          \"hips\": ").Append(Arr(c.hips, 3)).Append(", \"hipsTurn\": ").Append(Arr(c.hipsTurn, 1)).Append(", \"chestTurn\": ").Append(Arr(c.chestTurn, 1)).Append(", \"headTurn\": ").Append(Arr(c.headTurn, 1)).Append(",\n");
                sb.Append("          \"collarL\": ").Append(Arr(c.collarL, 3)).Append(", \"upperArmL\": ").Append(Arr(c.upperArmL, 3)).Append(", \"forearmL\": ").Append(Arr(c.forearmL, 3)).Append(", \"handL\": ").Append(Arr(c.handL, 3)).Append(",\n");
                sb.Append("          \"collarR\": ").Append(Arr(c.collarR, 3)).Append(", \"upperArmR\": ").Append(Arr(c.upperArmR, 3)).Append(", \"forearmR\": ").Append(Arr(c.forearmR, 3)).Append(", \"handR\": ").Append(Arr(c.handR, 3)).Append(",\n");
                sb.Append("          \"spine\": ").Append(Arr(c.spine, 3)).Append(",\n");
                sb.Append("          \"ankleL\": ").Append(Arr(c.ankleL, 3)).Append(", \"ankleR\": ").Append(Arr(c.ankleR, 3)).Append(",\n");
                sb.Append("          \"hipsMove\": ").Append(Arr(c.hipsMove, 4)).Append(", \"hipsTurnMove\": ").Append(Arr(c.hipsTurnMove, 2)).Append(", \"chestTurnMove\": ").Append(Arr(c.chestTurnMove, 2)).Append(", \"headTurnMove\": ").Append(Arr(c.headTurnMove, 2)).Append(", \"armsMove\": ").Append(Arr(c.armsMove, 2)).Append(", \"handsMove\": ").Append(Arr(c.handsMove, 2)).Append("\n");
                sb.Append("        }").Append(j < carriages.Length - 1 ? "," : "").Append("\n");
            }

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
                    var clip = Clip(name);
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

    // ---- The long steps of a recording (the plan's step S3): what a step is, in numbers ----------------------

    private static void Steps()
    {
        Console.WriteLine("the long steps in the recordings (metres, seconds, degrees; a foot's swing from its leaving the ground to its landing; all in the frame the clip began in):");
        foreach (string name in new[] { "cap_ginga", "cap_ginga_b", "cap_ginga_c", "cap_ginga_d", "cap_ginga_fwd", "cap_ginga_back" })
        {
            var clip = Clip(name);
            if (clip == null)
            {
                continue;
            }

            int n = clip.Frames;
            var all = new Vector3[n][];
            for (int f = 0; f < n; f++)
            {
                all[f] = new Vector3[LocoClip.BoneCount];
                BodyMeasures.World(clip, f, all[f]);
            }

            float Yaw(int f, LocoBone a, LocoBone b) => YawOf(all[f][(int)b] - all[f][(int)a]);
            Console.WriteLine($"  {name} ({clip.Duration:0.00} s):");
            if (Environment.GetEnvironmentVariable("STYLE_PLACES") == "1")
            {
                // (Where everything is at each tenth of a second: the feet, the hips, how the hips and the chest are turned.)
                for (int f = 0; f < n; f += 3)
                {
                    Vector3 l = all[f][(int)LocoBone.LeftFoot], r = all[f][(int)LocoBone.RightFoot], h = all[f][(int)LocoBone.Hips];
                    bool dl = (clip.Contact(0, f / clip.Rate) & LocoContact.Down) != 0, dr = (clip.Contact(1, f / clip.Rate) & LocoContact.Down) != 0;
                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "      {0,5:0.00} L {1}({2,5:0.00},{3,5:0.00}) R {4}({5,5:0.00},{6,5:0.00}) hips ({7,5:0.00},{8,5:0.00}) {9:0.00} up, turned {10,4:0}°, chest {11,4:0}°, head {12,4:0}°",
                        f / clip.Rate, dl ? "down" : " up ", l.x * Metre, l.z * Metre, dr ? "down" : " up ", r.x * Metre, r.z * Metre, h.x * Metre, h.z * Metre, h.y * Metre,
                        Yaw(f, LocoBone.LeftUpLeg, LocoBone.RightUpLeg), Yaw(f, LocoBone.LeftArm, LocoBone.RightArm), Yaw(f, LocoBone.LeftEye, LocoBone.RightEye)));
                }
            }

            for (int foot = 0; foot < 2; foot++)
            {
                LocoBone ankle = foot == 0 ? LocoBone.LeftFoot : LocoBone.RightFoot, other = foot == 0 ? LocoBone.RightFoot : LocoBone.LeftFoot, toe = foot == 0 ? LocoBone.LeftToeBase : LocoBone.RightToeBase, otherToe = foot == 0 ? LocoBone.RightToeBase : LocoBone.LeftToeBase;
                bool Down(int f) => (clip.Contact(foot, f / clip.Rate) & LocoContact.Down) != 0;
                int start = -1;
                for (int f = 1; f < n; f++)
                {
                    if (!Down(f) && Down(f - 1))
                    {
                        start = f - 1;
                    }
                    else if (Down(f) && !Down(f - 1) && start >= 0)
                    {
                        int a = start, b = f;
                        Vector3 from = all[a][(int)ankle], to = all[b][(int)ankle];
                        Vector3 line = new Vector3(to.x - from.x, 0f, to.z - from.z);
                        float far = line.magnitude;
                        if (far * Metre < 0.25f)
                        {
                            start = -1;
                            continue;
                        }

                        Vector3 along = line / far, side = new Vector3(along.z, 0f, -along.x);
                        float peak = 0f, peakAt = 0f, bulge = 0f;
                        float ground = Mathf.Min(from.y, to.y);
                        for (int k = a; k <= b; k++)
                        {
                            float u = (k - a) / (float)(b - a);
                            Vector3 q = all[k][(int)ankle];
                            if (q.y - ground > peak) { peak = q.y - ground; peakAt = u; }
                            float off = Vector3.Dot(q - from, side);
                            if (Mathf.Abs(off) > Mathf.Abs(bulge)) bulge = off;
                        }

                        // The hips: how far they go over the swing, and how much of it by a quarter, half and three quarters of it; how far before and after.
                        Vector3 h0 = all[a][(int)LocoBone.Hips], h1 = all[b][(int)LocoBone.Hips];
                        Vector3 hl = new Vector3(h1.x - h0.x, 0f, h1.z - h0.z);
                        float hfar = hl.magnitude;
                        float Share(float u)
                        {
                            int k = Mathf.Clamp(Mathf.RoundToInt(a + u * (b - a)), 0, n - 1);
                            Vector3 d = all[k][(int)LocoBone.Hips] - h0;
                            return hfar > 1e-4f ? Vector3.Dot(new Vector3(d.x, 0f, d.z), hl) / (hfar * hfar) : 0f;
                        }

                        int lead = Mathf.Max(0, a - Mathf.RoundToInt(0.25f * clip.Rate)), trail = Mathf.Min(n - 1, b + Mathf.RoundToInt(0.25f * clip.Rate));
                        Vector3 before = all[a][(int)LocoBone.Hips] - all[lead][(int)LocoBone.Hips], after = all[trail][(int)LocoBone.Hips] - all[b][(int)LocoBone.Hips];
                        float low = float.MaxValue;
                        for (int k = a; k <= b; k++) low = Mathf.Min(low, all[k][(int)LocoBone.Hips].y);
                        // The planted foot: how far it slides and turns.
                        Vector3 p0 = all[a][(int)other], p1 = all[b][(int)other];
                        float pivot = Mathf.DeltaAngle(Yaw(a, other, otherToe), Yaw(b, other, otherToe));
                        // Where the swinging foot is against the planted one, along and across where the body faces at the landing.
                        float faces = Yaw(b, LocoBone.LeftArm, LocoBone.RightArm) ;
                        var toFacing = Quaternion.Euler(0f, -(faces), 0f);
                        Vector3 relFrom = toFacing * (from - p0), relTo = toFacing * (to - p1);
                        Console.WriteLine(string.Format(CultureInfo.InvariantCulture,
                            "    {0} {1,5:0.00}-{2,5:0.00} s: {3:0.00} s, goes {4:0.00} m, lifts {5:0.00} at {6:0.00} of it, bows {7,5:0.00} to its side | against the planted foot, from ({8,5:0.00} across, {9,5:0.00} ahead) to ({10,5:0.00}, {11,5:0.00}) | hips go {12:0.00} m ({13:0.00} {14:0.00} {15:0.00} of it by a quarter, half, three quarters; {16:0.00} m in the quarter second before, {17:0.00} after), {18:0.00} up at the start, {19:0.00} lowest, {20:0.00} at the end | hips turn {21,4:0}°, chest {22,4:0}°, the swinging foot {23,4:0}°, the planted foot {24,4:0}° and slides {25:0.00} m",
                            foot == 0 ? "L" : "R", a / clip.Rate, b / clip.Rate, (b - a) / clip.Rate, far * Metre, peak * Metre, peakAt, bulge * Metre,
                            relFrom.x * Metre, relFrom.z * Metre, relTo.x * Metre, relTo.z * Metre,
                            hfar * Metre, Share(0.25f), Share(0.5f), Share(0.75f), new Vector2(before.x, before.z).magnitude * Metre, new Vector2(after.x, after.z).magnitude * Metre,
                            h0.y * Metre, low * Metre, h1.y * Metre,
                            Mathf.DeltaAngle(Yaw(a, LocoBone.LeftUpLeg, LocoBone.RightUpLeg), Yaw(b, LocoBone.LeftUpLeg, LocoBone.RightUpLeg)), Mathf.DeltaAngle(Yaw(a, LocoBone.LeftArm, LocoBone.RightArm), Yaw(b, LocoBone.LeftArm, LocoBone.RightArm)),
                            Mathf.DeltaAngle(Yaw(a, ankle, toe), Yaw(b, ankle, toe)), pivot, new Vector2(p1.x - p0.x, p1.z - p0.z).magnitude * Metre));
                        start = -1;
                    }
                }
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

    /// <summary>
    /// The standard movement made of its parts against the recordings it is made of (`style made`; the fight rhythm
    /// plan's step A): every recording of the plain, guard and sword styles, at every frame and half way between,
    /// the body put together from its parts (<see cref="MoveScore"/>, levers at rest) against the recording's points;
    /// and what each lever does to one walk.
    /// </summary>
    private static void Made()
    {
        Console.WriteLine("the standard movement made of its parts against its recordings (levers at rest; mm, the furthest any point is at any moment):");
        var seen = new HashSet<string>();
        var a = new Vector3[LocoClip.BoneCount];
        var b = new Vector3[LocoClip.BoneCount];
        float worstOfAll = 0f;
        foreach (var (style, _, jobs) in Sources)
        {
            if (style != "plain" && style != "guard" && style != "sword")
            {
                continue;
            }

            foreach (var (job, names) in jobs)
            {
                foreach (string name in names)
                {
                    var clip = LocoLibrary.Get(name);
                    if (clip == null || !seen.Add(name))
                    {
                        continue;
                    }

                    var score = MoveScore.Of(clip);
                    float worst = 0f, sum = 0f;
                    int at = 0, count = 0;
                    LocoBone bone = LocoBone.Hips;
                    for (int h = 0; h < 2 * clip.Frames; h++)
                    {
                        float time = 0.5f * h / clip.Rate;
                        clip.Pose(time, a, 1f);
                        score.Pose(time, b, 1f, MoveLevers.Plain);
                        for (int i = 0; i < a.Length; i++)
                        {
                            float d = (a[i] - b[i]).magnitude * Locomotor.ClipLeg * 1000f;
                            sum += d;
                            count++;
                            if (d > worst)
                            {
                                worst = d;
                                at = h;
                                bone = (LocoBone)i;
                            }
                        }
                    }

                    worstOfAll = Mathf.Max(worstOfAll, worst);
                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "  {0,-9} {1,-12} {2,-18}: {3,5:0.00} at most ({4}, at {5:0.00} s), {6:0.000} usual; {7} frames", style, job, name, worst, bone, 0.5f * at / clip.Rate, sum / Mathf.Max(1, count), clip.Frames));
                }
            }
        }

        Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "  the furthest of all: {0:0.00} mm", worstOfAll));

        var walk = LocoLibrary.Get("sns_walk");
        if (walk == null)
        {
            return;
        }

        Console.WriteLine("what each lever does to the guard's walk (sns_walk; over one cycle, cm and degrees):");
        var tries = new (string name, MoveLevers levers)[]
        {
            ("at rest", MoveLevers.Plain),
            ("bounce 2", With(l => l.Bounce = 2f)), ("bounce 0", With(l => l.Bounce = 0f)),
            ("sway 2", With(l => l.Sway = 2f)), ("hip turn 2", With(l => l.HipTurn = 2f)), ("chest turn 2", With(l => l.ChestTurn = 2f)),
            ("arm swing 2", With(l => l.ArmSwing = 2f)), ("step lift 2", With(l => l.StepLift = 2f)),
            ("lean 15", With(l => l.Lean = 15f)), ("crouch 10 cm", With(l => l.Crouch = 0.10f / Locomotor.ClipLeg)),
        };
        var m = MoveScore.Of(walk);
        foreach (var (name, levers) in tries)
        {
            Vector3 lo = Vector3.one * 1e9f, hi = Vector3.one * -1e9f, handLo = lo, handHi = hi;
            float yawLo = 1e9f, yawHi = -1e9f, chestLo = 1e9f, chestHi = -1e9f, lift = 0f, floor = 1e9f, lean = 0f, knee = 0f, feet = 0f;
            int n = walk.Frames;
            for (int f = 0; f < n; f++)
            {
                var t = m.Parts(f / walk.Rate, levers, out var spine);
                BodyMeasures.OfClips.Compose(t, b, spine);
                walk.Pose(f / walk.Rate, a, 1f);
                lo = Vector3.Min(lo, t.Pelvis); hi = Vector3.Max(hi, t.Pelvis);
                Vector3 hand = b[(int)LocoBone.LeftHand] - b[(int)LocoBone.Spine2];
                handLo = Vector3.Min(handLo, hand); handHi = Vector3.Max(handHi, hand);
                yawLo = Mathf.Min(yawLo, t.PelvisTurn.y); yawHi = Mathf.Max(yawHi, t.PelvisTurn.y);
                chestLo = Mathf.Min(chestLo, t.ChestTurn.y); chestHi = Mathf.Max(chestHi, t.ChestTurn.y);
                floor = Mathf.Min(floor, b[(int)LocoBone.LeftFoot].y); lift = Mathf.Max(lift, b[(int)LocoBone.LeftFoot].y);
                Vector3 up = b[(int)LocoBone.Head] - b[(int)LocoBone.Hips];
                lean += Mathf.Atan2(up.z, up.y) * Mathf.Rad2Deg / n;
                Vector3 thigh = b[(int)LocoBone.LeftLeg] - b[(int)LocoBone.LeftUpLeg], shin = b[(int)LocoBone.LeftFoot] - b[(int)LocoBone.LeftLeg];
                knee += Vector3.Angle(thigh, shin) / n;
                feet = Mathf.Max(feet, (b[(int)LocoBone.LeftFoot] - a[(int)LocoBone.LeftFoot]).magnitude);
                feet = Mathf.Max(feet, new Vector2(b[(int)LocoBone.RightFoot].x - a[(int)LocoBone.RightFoot].x, b[(int)LocoBone.RightFoot].z - a[(int)LocoBone.RightFoot].z).magnitude);
            }

            float cm = 100f * Locomotor.ClipLeg;
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture,
                "  {0,-13}: hips go {1,4:0.0} up and down, {2,4:0.0} side to side, turn {3,4:0.0} deg; chest turns {4,4:0.0} deg; left hand goes {5,4:0.0} on the chest; left ankle rises {6,4:0.0}; hips to head leans {7,4:0.0} deg; left knee bent {8,4:0.0} deg; hips {9,5:0.0} up; ankles off the recording's {10:0.0}",
                name, (hi.y - lo.y) * cm, (hi.x - lo.x) * cm, yawHi - yawLo, chestHi - chestLo, (handHi - handLo).magnitude * cm, (lift - floor) * cm, lean, knee, 0.5f * (hi.y + lo.y) * cm, feet * cm));
        }
    }

    private static MoveLevers With(Action<MoveLeversBox> set)
    {
        var box = new MoveLeversBox();
        set(box);
        return new MoveLevers { Bounce = box.Bounce, Sway = box.Sway, HipTurn = box.HipTurn, ChestTurn = box.ChestTurn, ArmSwing = box.ArmSwing, StepLift = box.StepLift, Lean = box.Lean, Crouch = box.Crouch };
    }

    private sealed class MoveLeversBox
    {
        public float Bounce = 1f, Sway = 1f, HipTurn = 1f, ChestTurn = 1f, ArmSwing = 1f, StepLift = 1f, Lean, Crouch;
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
        var whole = MannerDials.Whole;
        whole.Motion = 0f;
        shifted.Mid = StyleManner.Lay(basis.Mid, plainOther, styleOther, whole);
        shifted.Spine = styleOther.Spine;
        return shifted;
    }
}
