// `locolab [dir] [seconds]`: the locomotion lab headless. A Medium walks the lab's course (the plan's
// test course) on the new locomotion; the measures are printed, and every other step's pose is written
// to <dir>/locolab.json for Tools/CompileCheck/locofilm.py to draw (a film from above and from the side).
using System;
using System.Globalization;
using System.IO;
using System.Text;
using PhysicsBattler.Locomotion;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

internal static class LocoLabReport
{
    public static int Run(string dir, float seconds)
    {
        Directory.CreateDirectory(dir);
        string body = Environment.GetEnvironmentVariable("LOCO_BODY") ?? "Medium";
        var rig = RigFile.Parse(File.ReadAllText(Path.Combine(Application.dataPath, $"_Battler/Data/Rigs/{body.ToLowerInvariant()}_rig.json")));
        var profile = UnitProfile.CreateDefault();
        profile.body = body;
        var root = UnitBuilder.Build(rig, profile, "Lab " + body, _ => null, 0);
        var unitRig = root.GetComponent<UnitRig>();
        var anim = new AnimRig(root.transform, unitRig);
        float leg = anim.RestLocalPosition(PartId.ShinL).magnitude + anim.RestLocalPosition(PartId.FootL).magnitude;
        LocoLibrary.Load();
        var loco = new Locomotor(leg);
        if (!loco.Ready)
        {
            Console.WriteLine("locolab: the clip library is missing clips");
            return 1;
        }

        var poser = new LocoBody(anim, loco);
        poser.SoleFrom(unitRig);
        float bladeLength = 0f, shieldRadius = 0f;
        if (Environment.GetEnvironmentVariable("LOCO_ARMS") == "sword")
        {
            // A weapon in the right hand and a shield in the left, gripped as the game grips them.
            anim.SlotWeaponR.localRotation = UnitController.MainGrip;
            anim.SlotWeaponL.localRotation = UnitController.ShieldGripNow;
            poser.CarryR = LocoBody.Carry.Weapon;
            poser.CarryL = LocoBody.Carry.Shield;
            poser.BladeR = UnitController.MainGrip * Vector3.forward;
            bladeLength = 0.9f * leg / 0.845f;
            shieldRadius = 0.3f * leg / 0.845f;
        }
        string legName = "";
        if (Environment.GetEnvironmentVariable("LOCO_TRACE") != null)
        {
            LocoBody.Trace = text => Console.WriteLine($"    {legName}: {text}");
        }
        // (LOCO_MATCHED=1: out of his guard he walks by matched footage, the plan "Walking that feels like a real person", step 3.)
        Locomotor.Matched = Environment.GetEnvironmentVariable("LOCO_MATCHED") == "1";
        float MatchLever(string name, float otherwise) => float.TryParse(Environment.GetEnvironmentVariable(name), NumberStyles.Float, CultureInfo.InvariantCulture, out float v) ? v : otherwise;
        MotionMatch.PullsAtMost = MatchLever("MATCH_PULL", MotionMatch.PullsAtMost);
        MotionMatch.TurnsAtMost = MatchLever("MATCH_TURN", MotionMatch.TurnsAtMost);
        MotionMatch.TempoAtMost = MatchLever("MATCH_TEMPO", MotionMatch.TempoAtMost);
        MotionMatch.JumpsIfBetterBy = MatchLever("MATCH_BETTER", MotionMatch.JumpsIfBetterBy);
        MotionMatch.PlantedFeetStay = Environment.GetEnvironmentVariable("MATCH_PLANTED") != "0";
        var measures = new LocoMeasures(anim, loco, poser);
        string courseName = Environment.GetEnvironmentVariable("LOCO_COURSE");
        var course = courseName == "strafing" ? LocoCourse.Strafing() : courseName == "guard" ? LocoCourse.GuardTest() : LocoCourse.Lab();
        // (The guard: "up,down,..." times in seconds, or the guard test's own: up at 1.5 s, down as it walks on.)
        string guardTimes = Environment.GetEnvironmentVariable("LOCO_GUARD") ?? (courseName == "guard" ? "1.5,19.5" : null);
        if (guardTimes != null)
        {
            loco.SetGuard(LocoLibrary.CombatPrefix(body));
        }
        string line = Environment.GetEnvironmentVariable("LOCO_LINE");   // "speed,facing": a straight line along +z at that speed, facing that way
        if (line != null)
        {
            var parts = line.Split(',');
            float sp = float.Parse(parts[0], CultureInfo.InvariantCulture), yaw = float.Parse(parts[1], CultureInfo.InvariantCulture);
            course = new LocoCourse { Start = new Vector3(0f, 0f, -5f), StartYaw = yaw };
            course.Hold(0.5f).To(0f, 5f, sp, $"line at {sp} m/s facing {yaw}", true, LocoCourse.Face.Fixed, yaw).Hold(0.5f);
        }
        course.Repeat = false;
        course.SpeedScale = Mathf.Sqrt(leg / 0.845f);
        if (Locomotor.Matched && guardTimes == null && Environment.GetEnvironmentVariable("LOCO_LINE") == null)
        {
            // (As the lab does: on the matched walk the course is asked at the footage's paces.)
            course.SpeedScale *= LocomotionLab.MatchedCourse;
        }

        // (LOCO_PACE=0.57: the old walk asked the same paces, to compare the two like for like.)
        course.SpeedScale *= MatchLever("LOCO_PACE", 1f);
        course.LegStarted += (i, name) => { legName = name ?? ""; Console.WriteLine($"  {i,2}: {name}"); };
        loco.Place(course.Start, course.StartYaw);
        course.Reset();
        const float dt = 0.02f;
        var matchedPlayed = new System.Collections.Generic.Dictionary<string, float>();
        var matchedJumps = new System.Collections.Generic.Dictionary<string, int>();
        var matchedTime = new System.Collections.Generic.Dictionary<string, float>();
        int lastJumps = 0;
        var film = new LocoFilm(dt) { BladeLength = bladeLength, ShieldRadius = shieldRadius };
        int steps = (int)(seconds / dt);
        string lastMode = "";
        for (int i = 0; i < steps && course.Current != null; i++)
        {
            course.Steer(dt, loco.Position, loco.Facing, loco.Velocity, out var want, out float facing);
            loco.StopAt = course.StopPoint;
            if (guardTimes != null)
            {
                int toggles = 0;
                foreach (var g in guardTimes.Split(','))
                {
                    if (i * dt >= float.Parse(g, CultureInfo.InvariantCulture))
                    {
                        toggles++;
                    }
                }

                loco.GuardWanted = toggles % 2 == 1;
            }
            foreach (var shove in (Environment.GetEnvironmentVariable("LOCO_SHOVE") ?? "").Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                // "time,yaw,m/s;...": shoves at those times.
                var p = shove.Split(',');
                float at = float.Parse(p[0], CultureInfo.InvariantCulture);
                if (Mathf.Abs(i * dt - at) < 0.5f * dt)
                {
                    loco.Push(Quaternion.Euler(0f, float.Parse(p[1], CultureInfo.InvariantCulture), 0f) * Vector3.forward * float.Parse(p[2], CultureInfo.InvariantCulture));
                    Console.WriteLine($"    shoved at {at} s");
                }
            }

            loco.Step(dt, want, facing);
            if (loco.Match != null)
            {
                string key = legName + "|" + loco.Match.Clip.Name;
                matchedPlayed.TryGetValue(key, out float had);
                matchedPlayed[key] = had + dt;
                matchedJumps.TryGetValue(legName, out int jumped);
                matchedJumps[legName] = jumped + (loco.Match.Jumps - lastJumps);
                lastJumps = loco.Match.Jumps;
                matchedTime.TryGetValue(legName, out float t0);
                matchedTime[legName] = t0 + dt;
            }

            poser.Pose(dt);
            measures.Sample(dt, i * dt, legName);
            string modeNow = loco.Current + (loco.Shot != null ? " " + loco.Shot.Name : "");
            if (modeNow != lastMode && Environment.GetEnvironmentVariable("LOCO_MODES") != null)
            {
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "    {0:0.00} s {1}: {2} (speed {3:0.00}, want {4:0.00}, facing {5:0}, pos {6:0.00},{7:0.00})",
                    i * dt, legName, modeNow, loco.Velocity.magnitude, want.magnitude, loco.Facing, loco.Position.x, loco.Position.z));
            }

            lastMode = modeNow;
            if (Environment.GetEnvironmentVariable("LOCO_FOOT") != null && i * dt > 2f && i * dt < 3.5f)
            {
                var fl = poser.Feet[0];
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "t {0:0.00} phase {1:0.000} down {2} heel {3:0.0} toe {4:0.0} locked {5} toe? {6} posed {7:0.000},{8:0.000} lock {9:0.000},{10:0.000} root z {11:0.000} speed {12:0.00} stride x{13:0.00}",
                    i * dt, loco.Phase, loco.Down[0], loco.Heel[0], loco.Toe[0], fl.Locked, fl.OnToe, fl.Posed.x, fl.Posed.z, fl.Ankle.x, fl.Ankle.z, loco.Position.z, loco.Velocity.magnitude, loco.StrideScale));
            }
            film.Add(i * dt, legName, loco, poser, anim);
        }

        File.WriteAllText(Path.Combine(dir, "locolab.json"), film.ToString());
        Console.WriteLine(measures.Report());
        foreach (var leg1 in matchedTime.Keys)
        {
            var played = new System.Collections.Generic.List<string>();
            foreach (var kv in matchedPlayed)
            {
                if (kv.Key.StartsWith(leg1 + "|", StringComparison.Ordinal) && kv.Value >= 0.3f)
                {
                    played.Add(string.Format(CultureInfo.InvariantCulture, "{0} {1:0.0} s", kv.Key.Substring(leg1.Length + 1), kv.Value));
                }
            }

            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "MATCHED {0,-28} {1,5:0.0} s, {2,3} jumps ({3:0.0} a second): {4}", leg1, matchedTime[leg1], matchedJumps[leg1],
                matchedJumps[leg1] / Mathf.Max(0.1f, matchedTime[leg1]), string.Join(", ", played)));
        }

        return 0;
    }
}
