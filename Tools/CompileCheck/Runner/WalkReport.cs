// `walks [dir] [body]`: walks each body (its own style, so its captured walk) forward, back, to the
// side and to a stop, and writes its animated pose per step to CSV (as `rhythms` does) for
// Tools/Mocap/compare.py --walk; prints the steps, cadence, slides and reach.
using System;
using System.Globalization;
using System.IO;
using System.Text;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using PhysicsBattler.Tests;
using PhysicsBattler.Units;
using UnityEngine;

internal static class WalkReport
{
    public static int Run(string dir, string onlyBody)
    {
        if (dir != null) Directory.CreateDirectory(dir);
        var ci = CultureInfo.InvariantCulture;
        foreach (string body in new[] { "Large", "Medium", "Small" })
        {
            if (onlyBody != null && body != onlyBody) continue;
            var (unit, opponent) = RhythmTests.Pair(body);
            opponent.Place(Vector3.forward * 30f, 180f);
            unit.Opponent = null;
            unit.Rhythm.Auto = false;
            var both = new[] { unit, opponent };
            unit.FacePoint(unit.Root.position + Vector3.forward * 100f);
            RhythmTests.Step(both, 40);
            var csv = new StringBuilder("t,phase,weight,leg");
            foreach (var id in PartInfo.All) csv.Append($",{id}.x,{id}.y,{id}.z");
            csv.Append(",grip.x,grip.y,grip.z,tip.x,tip.y,tip.z,root.x,root.y,root.z").AppendLine();
            float speed = unit.WalkSpeed, leg = unit.Body.LegLength;
            float t = 0f, overReach = 0f, slide = 0f;
            int steps0 = unit.Stance.StepsTaken;
            var planted = new Vector3[2];
            var wasPlanted = new bool[2];
            var wasToe = new bool[2];
            (Vector3 dir, float seconds)[] legs = { (Vector3.forward, 3f), (Vector3.back, 1.5f), (Vector3.right, 1.5f), (Vector3.zero, 1.5f), (Vector3.up, 3f), (Vector3.zero, 1f) };
            float pelvisDrop = 0f;
            Vector3 heading = Vector3.forward;
            foreach (var (way, seconds) in legs)
            {
                for (float s = 0f; s < seconds; s += 1f / 60f)
                {
                    if (way == Vector3.up)
                    {
                        // Walking round in a circle, turning to face the way it goes (back onto a tile).
                        heading = Quaternion.Euler(0f, 90f / 60f, 0f) * heading;
                        unit.FacePoint(unit.Root.position + heading);
                        unit.Root.position += heading * (speed / 60f);
                        if (s + 1f / 60f >= seconds) unit.FacePoint(unit.Root.position + heading * 100f);
                    }
                    else
                    {
                        unit.Root.position += unit.Root.rotation * way * (speed / 60f);
                    }

                    RhythmTests.Step(both);
                    if (unit.Pose.PelvisDrop > pelvisDrop + 0.05f && Environment.GetEnvironmentVariable("WALK_DEBUG") != null)
                    {
                        for (int f = 0; f < 2; f++)
                        {
                            var th = unit.Anim.Part(f == 0 ? PartId.ThighL : PartId.ThighR).position;
                            var an = unit.Stance.Position(f);
                            Vector3 flat = an - unit.Root.position; flat.y = 0f;
                            Console.WriteLine($"  t {t:0.00} foot {f} stepping {unit.Stance.Stepping(f)} at {unit.Stance.StepProgress(f):0.00}, foot from root {flat.magnitude / leg:0.00} legs ({unit.Root.InverseTransformDirection(flat) / leg}), walk {unit.Gait.Weight:0.00} phase {unit.Gait.Phase:0.00} hold {unit.Stance.Hold} drop {unit.Pose.PelvisDrop:0.00}");
                        }
                    }

                    pelvisDrop = Mathf.Max(pelvisDrop, unit.Pose.PelvisDrop);
                    t += 1f / 60f;
                    for (int f = 0; f < 2; f++)
                    {
                        // (The new walk: a foot is planted while its lock holds, measured where the posed
                        // foot meets the ground, heel or toe; the old: while the stance doesn't step it.)
                        var walkBody = unit.WalkBody;
                        bool newWalk = walkBody != null && unit.WalkWeight > 0.99f;
                        bool p = newWalk ? walkBody.Feet[f].Locked : !unit.Stance.Stepping(f);
                        Vector3 at = newWalk ? walkBody.ContactPoint(f) : unit.Stance.Position(f);
                        at.y = 0f;
                        bool toe = newWalk && walkBody.Feet[f].OnToe;
                        if (p && wasPlanted[f] && toe == wasToe[f]) slide = Mathf.Max(slide, Vector3.Distance(planted[f], at));
                        if (p && (!wasPlanted[f] || toe != wasToe[f])) planted[f] = at;
                        wasPlanted[f] = p;
                        wasToe[f] = toe;
                        var thigh = unit.Anim.Part(f == 0 ? PartId.ThighL : PartId.ThighR).position;
                        var foot = unit.Anim.Part(f == 0 ? PartId.FootL : PartId.FootR).position;
                        overReach = Mathf.Max(overReach, Vector3.Distance(thigh, foot) / leg);
                    }
                    csv.Append(t.ToString("0.000", ci)).Append(',').Append(unit.Gait.Phase.ToString("0.000", ci)).Append(',').Append(unit.Gait.Weight.ToString("0.00", ci)).Append(',').Append(leg.ToString("0.000", ci));
                    foreach (var id in PartInfo.All) Add(csv, unit.Anim.Part(id).position, ci);
                    var slot = unit.Anim.SlotWeaponR;
                    Add(csv, slot.position, ci);
                    Add(csv, slot.position + slot.rotation * (Vector3.forward * unit.Loadout.Main.length * unit.Loadout.Scale), ci);
                    Add(csv, unit.Root.position, ci);
                    csv.AppendLine();
                }
                Console.WriteLine($"{body} walking {(way == Vector3.zero ? "to a stop" : way == Vector3.up ? "round in a circle" : way.ToString())}: hips dropped at most {pelvisDrop * 100f:0} cm, walk weight {(unit.Walk != null ? unit.WalkWeight : unit.Gait.Weight):0.00}{(unit.Walk != null ? $" ({unit.Walk.Current}, {unit.WalkBody.Feet[0].Landings + unit.WalkBody.Feet[1].Landings} landings)" : "")}, cycle {unit.Gait.CycleTime:0.00} s, {unit.Stance.StepsTaken - steps0} steps so far");
                pelvisDrop = 0f;
            }
            Vector3 home = unit.Stance.HomeWorld(Stance.Left, unit.Root.position, unit.FacingYaw);
            Console.WriteLine($"{body}: worst planted-foot slide {slide * 100f:0.0} cm, hip to ankle at most {overReach:0.00} legs, left foot {Vector3.Distance(home, unit.Stance.Position(Stance.Left)) * 100f:0} cm from home after stopping");
            if (dir != null) File.WriteAllText(Path.Combine(dir, $"{body.ToLowerInvariant()}_walk.csv"), csv.ToString());
            RhythmTests.Destroy(both);
        }
        return 0;
    }

    private static void Add(StringBuilder csv, Vector3 p, CultureInfo ci)
    {
        csv.Append(',').Append(p.x.ToString("0.000", ci)).Append(',').Append(p.y.ToString("0.000", ci)).Append(',').Append(p.z.ToString("0.000", ci));
    }
}
