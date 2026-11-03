// `strikes <dir> [body] [clips]`: the combat plan's step 3 headless. Each strike clip is played from the guard,
// aimed at target points at three distances and three heights in front of the body; for each it prints the plan
// (the turn, the lunge's stretch, the miss the pull had to make up) and how close the weapon's blade came to the
// target around the contact moment. A film of each first aim is written to <dir>/<clip>.json.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using PhysicsBattler.Locomotion;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

internal static class StrikeAimReport
{
    public static int Run(string dir, string body, string which)
    {
        Directory.CreateDirectory(dir);
        body ??= "Medium";
        bool measure = which == "--measure";
        if (measure)
        {
            which = null;
        }

        var rig = RigFile.Parse(File.ReadAllText(Path.Combine(Application.dataPath, $"_Battler/Data/Rigs/{body.ToLowerInvariant()}_rig.json")));
        var profile = UnitProfile.CreateDefault();
        profile.body = body;
        LocoLibrary.Load();
        string prefix = LocoLibrary.CombatPrefix(body);
        var names = LocoLibrary.Clips.Where(p => p.Value.Role == "strike" && (which == null ? p.Key.StartsWith(prefix) && LocoLibrary.StrikeFor(body, p.Key) : which.Split(',').Contains(p.Key))).Select(p => p.Key).OrderBy(n => n).ToList();
        var ci = CultureInfo.InvariantCulture;
        var allMisses = new List<float>();
        if (measure)
        {
            return Measure(rig, profile, body, prefix, names);
        }

        foreach (string name in names)
        {
            var clip = LocoLibrary.Get(name);
            var misses = new List<float>();
            var plans = new List<string>();
            bool filmed = false;
            // (The targets: a grid in front at three distances and heights; or, with AIM_REACH, about each
            // strike's own reach as measured: 20 degrees either side, 0.25 m nearer and further, 0.2 m lower and higher.)
            bool aroundReach = Environment.GetEnvironmentVariable("AIM_REACH") != null;
            StrikeReach.TryGet(body, name, out var reachGrip, out var reachWay);
            Vector3 reach = reachGrip + reachWay * (0.9f * Size(rig) * 0.7f);
            var targets = new List<(float d, float h, float b)>();
            if (aroundReach)
            {
                float d0 = new Vector2(reach.x, reach.z).magnitude, b0 = Mathf.Atan2(reach.x, reach.z) * Mathf.Rad2Deg;
                foreach (float db in new[] { -20f, 0f, 20f })
                {
                    foreach (float dd in new[] { -0.25f, 0f, 0.25f })
                    {
                        foreach (float dh in new[] { -0.2f, 0f, 0.2f })
                        {
                            targets.Add((d0 + dd, reach.y + dh, b0 + db));
                        }
                    }
                }
            }
            else
            {
                foreach (float d in new[] { 1.2f, 1.6f, 2.0f })
                {
                    foreach (float h in new[] { 0.9f, 1.3f, 1.6f })
                    {
                        targets.Add((d, h, 0f));
                    }
                }
            }

            foreach (var (distance, height, bearing) in targets)
            {
                {
                    var root = UnitBuilder.Build(rig, profile, "Aim " + body, _ => null, 0);
                    var unitRig = root.GetComponent<UnitRig>();
                    var anim = new AnimRig(root.transform, unitRig);
                    float leg = anim.RestLocalPosition(PartId.ShinL).magnitude + anim.RestLocalPosition(PartId.FootL).magnitude;
                    float size = leg / 0.845f;
                    var loco = new Locomotor(leg);
                    loco.SetGuard(prefix);
                    loco.GuardWanted = true;
                    var poser = new LocoBody(anim, loco) { WeaponLength = 0.9f * size, TwoHanded = LocoLibrary.TwoHanded(body) };
                    poser.SoleFrom(unitRig);
                    anim.SlotWeaponR.localRotation = UnitController.MainGrip;
                    poser.BladeR = UnitController.MainGrip * Vector3.forward;
                    const float dt = 1f / 60f;
                    loco.Place(Vector3.zero, 0f);
                    for (int i = 0; i < 90; i++)
                    {
                        loco.Step(dt, Vector3.zero, 0f);
                        poser.Pose(dt);
                    }

                    Vector3 target = aroundReach
                        ? Quaternion.Euler(0f, bearing, 0f) * new Vector3(0f, 0f, distance) + Vector3.up * height
                        : new Vector3(0f, height * size, distance * size);
                    poser.BodyKey = body;
                    poser.PlayStrike(clip, target);
                    var film = filmed ? null : new LocoFilm(dt, 2) { BladeLength = poser.WeaponLength };
                    float contact = clip.ContactFrame / clip.Rate, best = float.MaxValue;
                    float fastestRoot = 0f;
                    for (int i = 0; i < 400 && loco.Current == Locomotor.Mode.OneShot; i++)
                    {
                        loco.Step(dt, Vector3.zero, loco.Facing);
                        poser.Pose(dt);
                        fastestRoot = Mathf.Max(fastestRoot, new Vector2(loco.Velocity.x, loco.Velocity.z).magnitude);
                        film?.Add(i * dt, name, loco, poser, anim);
                        if (Environment.GetEnvironmentVariable("AIM_CHECK") != null && Mathf.Abs(loco.ShotTime - contact) < 0.5f * dt)
                        {
                            var s0 = anim.SlotWeaponR;
                            Vector3 sweet = s0.position + s0.rotation * Vector3.forward * (poser.WeaponLength * poser.SweetSpot);
                            Console.WriteLine(string.Format(ci, "    {0} {1:0.0}/{2:0.0}: at contact the sweet spot is {3} (predicted {4}, target {5}); root {6}, facing {7:0}",
                                name, distance, height, sweet, poser.AimPredicted, target, loco.Position, loco.Facing));
                        }

                        if (Mathf.Abs(loco.ShotTime - contact) <= 0.1f)
                        {
                            var slot = anim.SlotWeaponR;
                            Vector3 a = slot.position, b = slot.position + slot.rotation * Vector3.forward * poser.WeaponLength;
                            Vector3 ab = b - a;
                            float t = Mathf.Clamp01(Vector3.Dot(target - a, ab) / Mathf.Max(1e-6f, ab.sqrMagnitude));
                            best = Mathf.Min(best, (a + ab * t - target).magnitude);
                        }
                    }

                    // (After it: back to the guard and turned 60 degrees toward the next target, as the lab
                    // does; the fastest a hand or forearm moves through it all is told.)
                    if (Environment.GetEnvironmentVariable("AIM_AFTER") != null)
                    {
                        float fastest = 0f;
                        string where = "";
                        var parts = new[] { PartId.HandL, PartId.HandR, PartId.ForearmL, PartId.ForearmR };
                        var last = parts.Select(p => anim.Part(p).position).ToArray();
                        float turnTo = loco.Facing + 60f;
                        for (int i = 0; i < 90; i++)
                        {
                            loco.Step(dt, Vector3.zero, turnTo);
                            poser.Pose(dt);
                            film?.Add((400 + i) * dt, name + " after", loco, poser, anim);
                            for (int k = 0; k < parts.Length; k++)
                            {
                                Vector3 now = anim.Part(parts[k]).position;
                                float v = (now - last[k]).magnitude / dt;
                                if (v > fastest)
                                {
                                    fastest = v;
                                    where = $"{parts[k]} at {i * dt:0.00} s after ({loco.Current} {loco.Shot?.Name})";
                                }

                                last[k] = now;
                            }
                        }

                        Console.WriteLine(string.Format(ci, "    after {0}: the arms moved at most {1:0.0} m/s ({2})", name, fastest, where));
                    }

                    if (film != null)
                    {
                        File.WriteAllText(Path.Combine(dir, name + ".json"), film.ToString());
                        filmed = true;
                    }

                    misses.Add(best);
                    plans.Add(string.Format(ci, "{6:+0;-0}deg {0:0.00}m/{1:0.00}m: turn {2:+0;-0} lunge x{3:0.00} pull {4:0.00} -> miss {5:0.00}; the root at most {7:0.0} m/s", distance, height, poser.AimTurn, poser.AimLunge, poser.AimMissBeforePull, best, bearing, fastestRoot));
                    UnityEngine.Object.DestroyImmediate(root);
                }
            }

            misses.Sort();
            allMisses.AddRange(misses);
            Console.WriteLine(string.Format(ci, "{0,-20} ({1}): the blade passed within {2:0.00} m of its target (median), {3:0.00} m at worst", name, clip.Kind, misses[misses.Count / 2], misses[misses.Count - 1]));
            if (Environment.GetEnvironmentVariable("AIM_DETAIL") != null)
            {
                foreach (var p in plans)
                {
                    Console.WriteLine("    " + p);
                }
            }
        }

        allMisses.Sort();
        if (allMisses.Count > 0)
        {
            Console.WriteLine(string.Format(ci, "All: within {0:0.00} m (median), {1:0.00} m (90%), {2:0.00} m at worst; {3} aims within 5 cm of {4}",
                allMisses[allMisses.Count / 2], allMisses[(int)(allMisses.Count * 0.9f)], allMisses[allMisses.Count - 1], allMisses.Count(m => m <= 0.05f), allMisses.Count));
        }

        return 0;
    }

    /// <summary>
    /// Plays each strike unaimed (no turn, no stretch, no pull) from the guard at the origin facing +z, and
    /// writes where the weapon's sweet spot is at its contact moment into StrikeReach.json (other bodies'
    /// entries kept).
    /// </summary>
    private static int Measure(RigFile rig, UnitProfile profile, string body, string prefix, List<string> names)
    {
        var ci = CultureInfo.InvariantCulture;
        string path = Path.Combine(Application.dataPath, StrikeReach.AssetPath);
        var entries = new List<StrikeReach.Entry>();
        if (File.Exists(path))
        {
            var old = JsonUtility.FromJson<StrikeReach.File_>(File.ReadAllText(path));
            entries.AddRange((old.entries ?? Array.Empty<StrikeReach.Entry>()).Where(e => e.body != body));
        }

        foreach (string name in names)
        {
            var clip = LocoLibrary.Get(name);
            var root = UnitBuilder.Build(rig, profile, "Measure " + body, _ => null, 0);
            var unitRig = root.GetComponent<UnitRig>();
            var anim = new AnimRig(root.transform, unitRig);
            float leg = anim.RestLocalPosition(PartId.ShinL).magnitude + anim.RestLocalPosition(PartId.FootL).magnitude;
            float size = leg / 0.845f;
            var loco = new Locomotor(leg);
            loco.SetGuard(prefix);
            loco.GuardWanted = true;
            var poser = new LocoBody(anim, loco) { WeaponLength = 0.9f * size, TwoHanded = LocoLibrary.TwoHanded(body), BodyKey = body };
            poser.SoleFrom(unitRig);
            anim.SlotWeaponR.localRotation = UnitController.MainGrip;
            poser.BladeR = UnitController.MainGrip * Vector3.forward;
            const float dt = 1f / 60f;
            loco.Place(Vector3.zero, 0f);
            for (int i = 0; i < 90; i++)
            {
                loco.Step(dt, Vector3.zero, 0f);
                poser.Pose(dt);
            }

            Vector3 start = new Vector3(loco.Position.x, 0f, loco.Position.z);
            var turn0 = Quaternion.Inverse(Quaternion.Euler(0f, loco.Facing, 0f));
            float contact = clip.ContactFrame / clip.Rate;
            poser.PlayStrike(clip, poser.NominalTarget(), plan: false);
            Vector3? at = null;
            float before = 0f;
            Vector3 last = Vector3.zero, lastWay = Vector3.forward, atWay = Vector3.forward;
            for (int i = 0; i < 400 && loco.Current == Locomotor.Mode.OneShot; i++)
            {
                loco.Step(dt, Vector3.zero, loco.Facing);
                poser.Pose(dt);
                var s0 = anim.SlotWeaponR;
                Vector3 local = turn0 * (s0.position - start);
                Vector3 way = turn0 * (s0.rotation * Vector3.forward);
                float t = loco.ShotTime;
                if (at == null && t >= contact)
                {
                    // (Between the step before contact and this one.)
                    float u = t - before > 1e-5f ? Mathf.Clamp01((contact - before) / (t - before)) : 1f;
                    at = Vector3.Lerp(last, local, u);
                    atWay = Vector3.Slerp(lastWay, way, u).normalized;
                }

                before = t;
                last = local;
                lastWay = way;
            }

            UnityEngine.Object.DestroyImmediate(root);
            Vector3 p = at ?? last, w = at != null ? atWay : lastWay;
            Vector3 sweetAt = p + w * (poser.WeaponLength * poser.SweetSpot);
            entries.Add(new StrikeReach.Entry { body = body, clip = name, x = p.x, y = p.y, z = p.z, bx = w.x, by = w.y, bz = w.z });
            Console.WriteLine(string.Format(ci, "{0,-20} grip at contact {1:0.00} {2:0.00} {3:0.00}, blade {4:0.00} {5:0.00} {6:0.00}; sweet spot {7:0.00} {8:0.00} {9:0.00}",
                name, p.x, p.y, p.z, w.x, w.y, w.z, sweetAt.x, sweetAt.y, sweetAt.z));
        }

        var file = new StrikeReach.File_
        {
            about = "Where each strike's weapon grip is at its contact moment, and the way its blade points (bx, by, bz), played from the guard at a target ahead at chest height without turning, stretching or pulling (metres, against the start: x right, y up, z forward). Written by the headless runner: strikes <dir> <body> --measure.",
            entries = entries.OrderBy(e => e.body).ThenBy(e => e.clip).ToArray(),
        };
        File.WriteAllText(path, JsonUtility.ToJson(file, true));
        StrikeReach.Forget();
        Console.WriteLine($"{path}: {entries.Count} entries");
        return 0;
    }

    private static float Size(RigFile rig)
    {
        var root = UnitBuilder.Build(rig, UnitProfile.CreateDefault(), "Size", _ => null, 0);
        var anim = new AnimRig(root.transform, root.GetComponent<UnitRig>());
        float leg = anim.RestLocalPosition(PartId.ShinL).magnitude + anim.RestLocalPosition(PartId.FootL).magnitude;
        UnityEngine.Object.DestroyImmediate(root);
        return leg / 0.845f;
    }
}
