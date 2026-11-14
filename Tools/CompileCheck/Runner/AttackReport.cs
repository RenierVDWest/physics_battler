// The attack maker's checks (the plan "Aimed attacks"): each strike clip taken apart into a score and made again as
// it is, against the clip itself; and made for targets, where the made blade lands against where it was asked.
using System;
using System.Globalization;
using System.Linq;
using PhysicsBattler.Locomotion;
using UnityEngine;

internal static class AttackReport
{
    public static int Run()
    {
        LocoLibrary.Load();
        var m = BodyMeasures.OfClips;
        var hold = SwordHold.Medium;
        float leg = Locomotor.ClipLeg * 100f;
        Console.WriteLine($"the recorded body: thigh {m.ThighLen[1] * leg:0.0} cm, shin {m.ShinLen[1] * leg:0.0}, upper arm {m.UpperLen[1] * leg:0.0}, forearm {m.ForeLen[1] * leg:0.0}, collar bone {m.ClavLen[1] * leg:0.0}");
        var strikes = LocoLibrary.Clips.Values.Where(c => c.Role == "strike" && c.ContactFrame >= 0 && c.Name.StartsWith("sns_")).OrderBy(c => c.Name).ToList();
        Console.WriteLine("\nthe root's travel, as the walk steps it (each step's motion in the root's own frame, turned by the facing so far), against the clip's own track, by the landing (leg lengths: right, forward):");
        foreach (var clip in strikes)
        {
            clip.Root(0f, out Vector2 at0, out float yaw0);
            float facing = 0f;
            Vector3 place = Vector3.zero;
            float until = clip.ContactFrame / clip.Rate;
            for (float t = 0f; t < until - 1e-4f; t += 1f / 60f)
            {
                clip.RootMotion(t, Mathf.Min(until, t + 1f / 60f), out var moved, out float turned);
                place += Quaternion.Euler(0f, facing, 0f) * new Vector3(moved.x, 0f, moved.y);
                facing += turned;
            }

            clip.Root(until, out Vector2 at1, out float yaw1);
            Vector3 truly = Quaternion.Euler(0f, -yaw0, 0f) * new Vector3(at1.x - at0.x, 0f, at1.y - at0.y);
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "  {0,-20} stepped {1,5:0.00} {2,5:0.00} | the track {3,5:0.00} {4,5:0.00} | turned {5,4:0}° | apart {6:0.00}", clip.Name, place.x, place.z, truly.x, truly.z, Mathf.DeltaAngle(yaw0, yaw1), (place - truly).magnitude));
        }

        Console.WriteLine("\nmade again as it is, against the clip (cm): the points' mean and worst; marks (s): the drive, the landing, carried through; the blade's height over the cut");
        var a = new Vector3[LocoClip.BoneCount];
        var b = new Vector3[LocoClip.BoneCount];
        foreach (var clip in strikes)
        {
            var score = AttackScore.Of(clip);
            var made = AttackMaker.Make(score, AttackStyle.Plain, default, hold);
            float sum = 0f, worst = 0f;
            int n = 0;
            for (int f = 0; f < clip.Frames; f++)
            {
                float t = f / clip.Rate;
                clip.Pose(t, a, 1f);
                made.Clip.Pose(t, b, 1f);
                for (int i = 0; i < a.Length; i++)
                {
                    float d = Vector3.Distance(a[i], b[i]) * leg;
                    sum += d;
                    worst = Mathf.Max(worst, d);
                    n++;
                }
            }

            clip.RootMotion(0f, clip.Duration, out var moved, out float turned);
            made.Clip.RootMotion(0f, made.Clip.Duration, out var movedMade, out float turnedMade);
            float fastest = score.StrikeSpeed.Max(), low = float.MaxValue, high = float.MinValue;
            for (int f = score.Drive; f <= score.Follow; f++)
            {
                if (score.StrikeSpeed[f] >= AttackMaker.FastEnough * fastest)
                {
                    low = Mathf.Min(low, score.Strike[f].y);
                    high = Mathf.Max(high, score.Strike[f].y);
                }
            }

            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "  {0,-20} mean {1,5:0.00} worst {2,5:0.0} | root {3:0.00} m and {4:0}° against {5:0.00} m and {6:0}° | drive {7:0.00} landing {8:0.00} carried {9:0.00} of {10:0.00} | blade fast from {11:0.00} to {12:0.00} m up, {13:0.0} m/s at its fastest",
                clip.Name, sum / Mathf.Max(1, n), worst, movedMade.magnitude * leg / 100f, turnedMade, moved.magnitude * leg / 100f, turned,
                score.Drive / score.Rate, score.Contact / score.Rate, score.Follow / score.Rate, score.Duration, low * leg / 100f, high * leg / 100f, fastest * leg / 100f));
        }

        Console.WriteLine("\nthe blade as this hold has it in the recorded hand, at the landing (the frame the clip began in): its way, how far off the arm's line (shoulder to grip) it points, the grip's height (m)");
        foreach (var clip in strikes)
        {
            var score = AttackScore.Of(clip);
            foreach (int f in new[] { score.Drive, score.Contact, score.Follow })
            {
                AttackScore.SwordOf(score.Frames[f], m, hold, false, out Vector3 sh, out _, out Vector3 grip, out Vector3 way);
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "  {0,-20} at {1:0.00} s: way {2,5:0.00} {3,5:0.00} {4,5:0.00}, {5,3:0}° off the arm's line, grip {6:0.00} up, {7:0.00} m from the shoulder", clip.Name, f / clip.Rate, way.x, way.y, way.z, Vector3.Angle(way, grip - sh), grip.y * 0.845f, (grip - sh).magnitude * 0.845f));
            }
        }

        Console.WriteLine("\nmade for a target (its own frame, m: right, up, forward): the turn, the travel, the hand's move, how far short; and the made clip's own blade at its landing against the target (cm)");
        var targets = new[] { new Vector3(0f, 1.2f, 1.5f), new Vector3(0.15f, 1.6f, 1.6f), new Vector3(-0.2f, 0.9f, 1.4f), new Vector3(0.1f, 1.45f, 1.9f), new Vector3(0f, 0.5f, 1.5f), new Vector3(0.1f, 0.1f, 1.4f) };
        foreach (var clip in strikes)
        {
            var score = AttackScore.Of(clip);
            if (!score.Plain)
            {
                continue;
            }

            foreach (var target in targets)
            {
                var aim = new AttackAim { Aimed = true, Target = target / 0.845f };
                var made = AttackMaker.Make(score, AttackStyle.Plain, aim, hold);
                made.Blade(made.ContactAt, 1f, out Vector3 grip, out Vector3 tip);
                Vector3 strike = grip + (tip - grip) * made.Hold.Strike;
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "  {0,-14} at {1,5:0.00} {2,5:0.00} {3,5:0.00}: lands {4:0.00} s in ({5:0.00} s of the recording), turned {6,4:0}°, travel x{7:0.00}, hips {12,3:0} cm down, lean {13,3:0}°, wrist {14,2:0}°, hand {8,3:0} cm, short {9,3:0} cm, cost {15:0.00} | the clip's blade {10,5:0.0} cm from the target, the plan's {11,5:0.0}",
                    clip.Name, target.x, target.y, target.z, made.ContactAt, made.ScoreTime, made.Turn, made.Lunge, made.HandMoved * 84.5f, made.Short * 84.5f,
                    Vector3.Distance(strike, aim.Target) * 84.5f, Vector3.Distance(made.Lands, aim.Target) * 84.5f, made.Drop * 84.5f, made.Lean, made.Steer, made.Cost));
            }
        }

        {
            // (How long making takes: an attack is made as it begins, in the frame it begins in.)
            var forehand = AttackScore.Of(LocoLibrary.Clips["sns_forehand"]);
            var aim = new AttackAim { Aimed = true, Target = new Vector3(0f, 1.2f, 1.5f) / 0.845f };
            AttackMaker.Make(forehand, AttackStyle.Plain, aim, hold);
            var watch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < 50; i++)
            {
                aim.Target.x = 0.01f * i;
                AttackMaker.Make(forehand, AttackStyle.Plain, aim, hold);
            }

            Console.WriteLine($"\nmaking one aimed attack takes {watch.Elapsed.TotalMilliseconds / 50:0.00} ms here");
        }

        string dump = Environment.GetEnvironmentVariable("ATTACK_DUMP");
        if (!string.IsNullOrEmpty(dump))
        {
            Dump(dump, hold);
        }

        return 0;
    }

    /// <summary>
    /// Made attacks written out as points for the film strips (Tools/Mocap/Attacks/strip.py): each frame's 33 points,
    /// its sword's grip and tip, in the frame the attack began in (metres on a Medium's leg).
    /// </summary>
    private static void Dump(string path, SwordHold hold)
    {
        const float leg = 0.845f;
        var forehand = AttackScore.Of(LocoLibrary.Clips["sns_forehand"]);
        var rising = AttackScore.Of(LocoLibrary.Clips["sns_rising"]);
        var thrust = AttackScore.Of(LocoLibrary.Clips["sns_thrust"]);
        AttackAim At(float x, float y, float z) => new AttackAim { Aimed = true, Target = new Vector3(x, y, z) / leg };
        AttackStyle Preset(string name) => AttackStyle.Presets.First(p => p.name == name).style;
        var cases = new (string name, AttackScore score, AttackStyle style, AttackAim aim)[]
        {
            ("forehand as recorded", forehand, AttackStyle.Plain, default),
            ("forehand at the head, 1.5 m", forehand, AttackStyle.Plain, At(0f, 1.6f, 1.5f)),
            ("forehand at the belly, 1.5 m", forehand, AttackStyle.Plain, At(0f, 1.05f, 1.5f)),
            ("forehand at the left shin, 1.5 m", forehand, AttackStyle.Plain, At(0.12f, 0.3f, 1.5f)),
            ("forehand at the belly, Heavy style", forehand, Preset("Heavy"), At(0f, 1.05f, 1.5f)),
            ("forehand at the belly, Quick style", forehand, Preset("Quick"), At(0f, 1.05f, 1.5f)),
            ("forehand at the belly, Brawler style", forehand, Preset("Brawler"), At(0f, 1.05f, 1.5f)),
            ("forehand at the belly, Whip style", forehand, Preset("Whip"), At(0f, 1.05f, 1.5f)),
            ("rising at the head, Heavy style", rising, Preset("Heavy"), At(0f, 1.6f, 1.5f)),
            ("thrust on the forehand's legs, at the chest, Heavy style", thrust.OnLegsOf(forehand), Preset("Heavy"), At(0f, 1.3f, 1.7f)),
            ("forehand at the belly, Low style", forehand, Preset("Low"), At(0f, 1.05f, 1.5f)),
            ("rising as recorded", rising, AttackStyle.Plain, default),
            ("rising at the head, 1.5 m", rising, AttackStyle.Plain, At(0f, 1.6f, 1.5f)),
            ("rising at the right thigh, 1.5 m", rising, AttackStyle.Plain, At(-0.12f, 0.65f, 1.5f)),
            ("thrust as recorded", thrust, AttackStyle.Plain, default),
            ("thrust at the chest, 1.3 m", thrust, AttackStyle.Plain, At(0f, 1.3f, 1.3f)),
            ("thrust on the forehand's legs, at the chest, 1.7 m", thrust.OnLegsOf(forehand), AttackStyle.Plain, At(0f, 1.3f, 1.7f)),
            ("thrust on the forehand's legs, at the head, 1.7 m", thrust.OnLegsOf(forehand), AttackStyle.Plain, At(0f, 1.6f, 1.7f)),
            ("forehand on the thrust's legs (standing), at the chest, 1.1 m", forehand.OnLegsOf(thrust), AttackStyle.Plain, At(0f, 1.3f, 1.1f)),
            ("rising on the forehand's legs, at the head, 1.5 m", rising.OnLegsOf(forehand), AttackStyle.Plain, At(0f, 1.6f, 1.5f)),
        };
        var text = new System.Text.StringBuilder("[");
        var pose = new Vector3[LocoClip.BoneCount];
        string N(float v) => v.ToString("0.###", CultureInfo.InvariantCulture);
        for (int c = 0; c < cases.Length; c++)
        {
            var made = AttackMaker.Make(cases[c].score, cases[c].style, cases[c].aim, hold);
            var clip = made.Clip;
            text.Append(c > 0 ? ",\n" : "\n").Append("{\"name\":\"").Append(cases[c].name).Append("\",\"rate\":").Append(N(clip.Rate)).Append(",\"contact\":").Append(clip.ContactFrame)
                .Append(",\"aimed\":").Append(cases[c].aim.Aimed ? "true" : "false")
                .Append(",\"target\":[").Append(N(cases[c].aim.Target.x * leg)).Append(',').Append(N(cases[c].aim.Target.y * leg)).Append(',').Append(N(cases[c].aim.Target.z * leg)).Append(']')
                .Append(",\"fit\":\"").Append(string.Format(CultureInfo.InvariantCulture, "lands {0:0.00} s in, turned {1:0}, travel x{2:0.00}, hips {3:0} cm down, lean {4:0}, wrist {5:0}, hand {6:0} cm, short {7:0} cm", made.ContactAt, made.Turn, made.Lunge, made.Drop * 84.5f, made.Lean, made.Steer, made.HandMoved * 84.5f, made.Short * 84.5f)).Append("\",\"frames\":[");
            for (int f = 0; f < clip.Frames; f++)
            {
                float t = f / clip.Rate;
                clip.Pose(t, pose, leg);
                clip.RootMotion(0f, t, out Vector2 moved, out float turned);
                var turn = Quaternion.Euler(0f, turned, 0f);
                made.Blade(t, hold.Length, out Vector3 grip, out Vector3 tip);
                text.Append(f > 0 ? "," : "").Append("{\"p\":[");
                for (int i = 0; i < pose.Length; i++)
                {
                    Vector3 flat = turn * new Vector3(pose[i].x, 0f, pose[i].z) + new Vector3(moved.x, 0f, moved.y) * leg;
                    text.Append(i > 0 ? "," : "").Append(N(flat.x)).Append(',').Append(N(pose[i].y)).Append(',').Append(N(flat.z));
                }

                text.Append("],\"s\":[").Append(N(grip.x * leg)).Append(',').Append(N(grip.y * leg)).Append(',').Append(N(grip.z * leg)).Append(',').Append(N(tip.x * leg)).Append(',').Append(N(tip.y * leg)).Append(',').Append(N(tip.z * leg))
                    .Append("],\"d\":[").Append((int)clip.Contact(0, t)).Append(',').Append((int)clip.Contact(1, t)).Append("]}");
            }

            text.Append("]}");
        }

        text.Append("\n]\n");
        System.IO.File.WriteAllText(path, text.ToString());
        Console.WriteLine($"\n{cases.Length} made attacks written to {path}");
    }
}
