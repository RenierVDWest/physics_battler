using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using PhysicsBattler.Locomotion;
using UnityEngine;

/// <summary>
/// The matched walk on a bare walker (the plan "Walking that feels like a real person", step 2): no body, no fight,
/// only the search, the recording moving the root and the pose it gives, sent round a course of wishes (stand, walk,
/// stop, turn, back, sideways, run). Prints how well he went where he was sent, how often he jumps between
/// recordings, how far a foot that is down slides, and how his speed and pose jump; writes a film that
/// Tools/Headless/gaitfilm.py and filmstrip.py read.
///
///   dotnet PhysicsBattler.Tests.dll match [film.json] [set]      set: relaxed (default) or a prefix of clip names
/// </summary>
public static class MatchReport
{
    private const float Leg = 0.845f;

    private static readonly string[] Relaxed =
    {
        "idle", "walk_fwd", "walk_back", "walk_left_cross", "walk_right_cross", "walk_left_step", "walk_right_step",
        "run_fwd", "run_left", "run_right", "sidestep_left", "sidestep_right", "start_walk", "start_walk_m",
        "turn_to_run", "turn_to_run_m", "stop_walk", "stop_walk_m", "turn180_left", "turn180_right", "turn90_left",
        "turn90_right", "turn180_run", "turn180_run_m", "turn180_walk", "turn180_walk_m", "walkturn90_left", "walkturn90_right",
    };

    private readonly struct Wish
    {
        public readonly float Until, Speed, Heading, Facing;
        public readonly string What;

        public Wish(float until, string what, float speed, float heading, float facing)
        {
            Until = until; What = what; Speed = speed; Heading = heading; Facing = facing;
        }
    }

    private static readonly LocoBone[] FilmBones =
    {
        LocoBone.Hips, LocoBone.Spine, LocoBone.Spine2, LocoBone.Head, LocoBone.LeftArm, LocoBone.LeftForeArm, LocoBone.LeftHand,
        LocoBone.RightArm, LocoBone.RightForeArm, LocoBone.RightHand, LocoBone.LeftUpLeg, LocoBone.LeftLeg, LocoBone.LeftFoot,
        LocoBone.RightUpLeg, LocoBone.RightLeg, LocoBone.RightFoot, LocoBone.HeadTop, LocoBone.LeftHandMiddle, LocoBone.RightHandMiddle,
        LocoBone.LeftToeEnd, LocoBone.RightToeEnd,
    };

    public static int Run(string[] args)
    {
        var inv = System.Globalization.CultureInfo.InvariantCulture;
        string filmPath = args.Length >= 2 ? args[1] : null;
        string set = args.Length >= 3 ? args[2] : "relaxed";
        var clips = set == "relaxed"
            ? Relaxed.Select(LocoLibrary.Get).Where(c => c != null).ToList()
            : LocoLibrary.Clips.Values.Where(c => c.Name.StartsWith(set, StringComparison.Ordinal)).ToList();
        if (clips.Count == 0)
        {
            Console.WriteLine("MATCH no recordings for the set " + set);
            return 1;
        }

        var built = Stopwatch.StartNew();
        var footage = new MatchFootage(clips);
        built.Stop();
        float minutes = clips.Sum(c => c.Duration) / 60f;
        Console.WriteLine(string.Format(inv, "MATCH footage: {0} recordings, {1:0.0} minutes, {2} moments of {3} numbers; made ready in {4} ms",
            clips.Count, minutes, footage.Count, MatchFootage.Size, built.ElapsedMilliseconds));

        // The course of wishes: (until, what, speed m/s, heading degrees, facing degrees). Heading and facing are turns to the right from +z.
        float walk = set == "relaxed" ? 1.5f : EnvF("MATCH_WALK", 1.3f), run = EnvF("MATCH_RUN", set == "relaxed" ? 4.2f : 3.2f), back = EnvF("MATCH_BACK", 1.1f), side = EnvF("MATCH_SIDE", 0.6f);
        var course = new List<Wish>
        {
            new Wish(1f, "stand", 0f, 0f, 0f),
            new Wish(5f, "walk forward", walk, 0f, 0f),
            new Wish(7f, "stand", 0f, 0f, 0f),
            new Wish(10f, "walk forward", walk, 0f, 0f),
            new Wish(14f, "turn right and walk", walk, 90f, 90f),
            new Wish(16f, "stand", 0f, 90f, 90f),
            new Wish(19f, "walk backward", back, 270f, 90f),
            new Wish(21f, "stand", 0f, 90f, 90f),
            new Wish(24f, "step to his left", side, 0f, 90f),
            new Wish(26f, "stand", 0f, 90f, 90f),
            new Wish(30f, "run", run, 90f, 90f),
            new Wish(33f, "stand", 0f, 90f, 90f),
            new Wish(37f, "turn about and walk", walk, 270f, 270f),
            new Wish(39f, "stand", 0f, 270f, 270f),
        };

        var start = clips.FirstOrDefault(c => c.Role == "idle" || c.Name.Contains("idle") || c.Name.EndsWith("_ID", StringComparison.Ordinal)) ?? clips[0];
        var mm = new MotionMatch(footage, Leg, start, Vector2.zero, 0f);
        const float dt = 1f / 60f;
        var film = new List<string>();
        var search = new Stopwatch();
        var playing = new Dictionary<string, float>();
        Vector2 wished = Vector2.zero, lastVel = Vector2.zero;
        var lastPose = new Vector3[LocoClip.BoneCount];
        var lastSpeed = new Vector3[LocoClip.BoneCount];
        var footWas = new Vector3[2];
        var slid = new float[2];
        var slides = new List<float>();
        var slideSpeeds = new List<float>();
        int speedJumps = 0, poseJumps = 0, steps = 0;
        float worstSpeedJump = 0f, worstPoseJump = 0f;
        int seg = 0;
        Vector2 segStart = Vector2.zero, segWishStart = Vector2.zero;
        float segTime = 0f, segMoving = 0f;
        var down = new bool[2];
        for (float t = 0f; seg < course.Count; t += dt)
        {
            var w = course[seg];
            float hr = w.Heading * Mathf.Deg2Rad;
            Vector2 want = new Vector2(Mathf.Sin(hr), Mathf.Cos(hr)) * w.Speed;
            int before = mm.Searches;
            search.Start();
            mm.Step(dt, want, w.Facing);
            search.Stop();
            wished += want * dt;
            steps++;
            playing[mm.Clip.Name] = playing.TryGetValue(mm.Clip.Name, out float had) ? had + dt : dt;

            // His root's speed and his pose from step to step: a jump is a change no body could make in a sixtieth of a second.
            float dv = (mm.Velocity - lastVel).magnitude;
            if (steps > 2)
            {
                worstSpeedJump = Mathf.Max(worstSpeedJump, dv);
                if (dv > 0.5f)
                {
                    speedJumps++;
                }
            }

            lastVel = mm.Velocity;
            float worst = 0f;
            for (int i = 0; i < LocoClip.BoneCount; i++)
            {
                Vector3 sp = (mm.Pose[i] - lastPose[i]) / dt;
                if (steps > 3)
                {
                    worst = Mathf.Max(worst, (sp - lastSpeed[i]).magnitude);
                }

                lastSpeed[i] = sp;
                lastPose[i] = mm.Pose[i];
            }

            worstPoseJump = Mathf.Max(worstPoseJump, worst);
            if (worst > 3f)
            {
                poseJumps++;
            }

            // A foot the recording has down: how fast it goes over the ground, and how far it slides while down.
            for (int f = 0; f < 2; f++)
            {
                Vector3 at = mm.World(f == 0 ? LocoBone.LeftFoot : LocoBone.RightFoot);
                bool isDown = mm.Down(f);
                if (isDown && down[f])
                {
                    float d = new Vector2(at.x - footWas[f].x, at.z - footWas[f].z).magnitude;
                    slid[f] += d;
                    slideSpeeds.Add(d / dt);
                }
                else if (!isDown && down[f])
                {
                    slides.Add(slid[f]);
                    slid[f] = 0f;
                }

                down[f] = isDown;
                footWas[f] = at;
            }

            segTime += dt;
            if (mm.Velocity.magnitude > 0.2f)
            {
                segMoving += dt;
            }

            if (filmPath != null)
            {
                var line = new System.Text.StringBuilder();
                line.Append("{\"blend\":\"").Append(mm.Clip.Name).Append(":1.00;\",\"ph\":0,\"spd\":").Append(mm.Velocity.magnitude.ToString("0.00", inv))
                    .Append(",\"want\":").Append(w.Speed.ToString("0.00", inv)).Append(",\"cad\":1.00,\"ss\":1.00,\"clipt\":").Append(mm.Time.ToString("0.000", inv))
                    .Append(",\"wish\":\"").Append(w.What).Append("\",\"t\":").Append(t.ToString("0.000", inv)).Append(",\"u\":\"Matched Medium\",\"doing\":\"")
                    .Append(mm.Velocity.magnitude > 0.2f ? "Cycle" : "Idle").Append("\",\"p\":[");
                foreach (var b in FilmBones)
                {
                    Vector3 at = mm.World(b);
                    line.Append(at.x.ToString("0.000", inv)).Append(',').Append(at.y.ToString("0.000", inv)).Append(',').Append(at.z.ToString("0.000", inv)).Append(',');
                }

                line.Length--;
                line.Append("],\"d\":[").Append(mm.Down(0) ? 1 : 0).Append(',').Append(mm.Down(1) ? 1 : 0).Append("],\"root\":[")
                    .Append(mm.Place.x.ToString("0.000", inv)).Append(',').Append(mm.Place.y.ToString("0.000", inv)).Append(',').Append(mm.Facing.ToString("0.0", inv)).Append("]}");
                film.Add(line.ToString());
            }

            if (t + dt >= w.Until)
            {
                Vector2 went = mm.Place - segStart, asked = wished - segWishStart;
                float off = Mathf.DeltaAngle(mm.Facing, w.Facing);
                Console.WriteLine(string.Format(inv, "MATCHLEG {0,-20} asked {1:0.00} m at {2:0.00} m/s; went {3:0.00} m, {4:0.00} m/s while moving ({5:0.0} s of {6:0.0}); ended {7:0.00} m from where the wish would be; facing {8:0} deg off; playing {9}",
                    w.What, asked.magnitude, w.Speed, went.magnitude, segMoving > 0.05f ? PathLength(film, segTime, dt) / Mathf.Max(0.05f, segMoving) : 0f, segMoving, segTime, (went - asked).magnitude, off, mm.Clip.Name));
                seg++;
                segStart = mm.Place;
                segWishStart = wished;
                segTime = 0f;
                segMoving = 0f;
            }
        }

        float total = steps * dt;
        slides.Sort();
        slideSpeeds.Sort();
        Console.WriteLine(string.Format(inv, "MATCH {0:0} s: {1} jumps between recordings ({2:0.0} a second), {3} searches at {4:0.000} ms each ({5:0.000} ms a step on average)",
            total, mm.Jumps, mm.Jumps / total, mm.Searches, search.Elapsed.TotalMilliseconds / Mathf.Max(1, mm.Searches), search.Elapsed.TotalMilliseconds / steps));
        Console.WriteLine(string.Format(inv, "MATCH his root's speed jumped over 0.5 m/s in one step {0} times (worst {1:0.00} m/s); a point of his pose changed speed by over 3 m/s in one step {2} times (worst {3:0.0} m/s)",
            speedJumps, worstSpeedJump, poseJumps, worstPoseJump));
        if (slides.Count > 0)
        {
            Console.WriteLine(string.Format(inv, "MATCH a foot the recording has down slid {0:0.0} cm a footfall (median; the worst tenth over {1:0.0} cm; {2} footfalls), going {3:0.00} m/s over the ground (median) while down",
                100f * slides[slides.Count / 2], 100f * slides[Mathf.Min(slides.Count - 1, slides.Count * 9 / 10)], slides.Count, slideSpeeds[slideSpeeds.Count / 2]));
        }

        Console.WriteLine("MATCH played: " + string.Join(", ", playing.OrderByDescending(p => p.Value).Take(14).Select(p => string.Format(inv, "{0} {1:0.0} s", p.Key, p.Value))));
        if (filmPath != null)
        {
            File.WriteAllText(filmPath, "[\n" + string.Join(",\n", film) + "\n]\n");
            Console.WriteLine("MATCH film: " + filmPath + " (" + film.Count + " steps)");
        }

        return 0;
    }

    private static float EnvF(string name, float fallback) =>
        float.TryParse(Environment.GetEnvironmentVariable(name), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float v) ? v : fallback;

    // (The length of the path his root took over the last stretch of the film, for his speed while moving. Zero with no film.)
    private static float PathLength(List<string> film, float seconds, float dt)
    {
        int n = Mathf.Min(film.Count, Mathf.RoundToInt(seconds / dt));
        if (n < 2)
        {
            return 0f;
        }

        float len = 0f;
        Vector2? was = null;
        for (int i = film.Count - n; i < film.Count; i++)
        {
            string s = film[i];
            int at = s.LastIndexOf("\"root\":[", StringComparison.Ordinal) + 8;
            string[] parts = s.Substring(at, s.Length - at - 2).Split(',');
            var p = new Vector2(float.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture), float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture));
            if (was.HasValue)
            {
                len += (p - was.Value).magnitude;
            }

            was = p;
        }

        return len;
    }
}
