// `footwork <dir> [seconds]`: two clip fighters steer themselves against each other with no blows (the plan "Bringing the
// lab into the fight", step 3): each walks in to its striking distance, holds it, circles, backs off when crowded.
// FOOT_A / FOOT_B: probe | gather | press | shell (each one's tempo, default probe); FOOT_GAP: the start apart (m, 3).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using PhysicsBattler.Combat;
using PhysicsBattler.Locomotion;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

internal static class FootworkReport
{
    public static int Run(string dir, float seconds)
    {
        Directory.CreateDirectory(dir);
        LocoLibrary.Load();
        var ci = CultureInfo.InvariantCulture;
        float gap = float.TryParse(Environment.GetEnvironmentVariable("FOOT_GAP"), NumberStyles.Float, ci, out float g) ? g : 3f;
        var a = SparFighter("A", Vector3.zero, 0f);
        var b = SparFighter("B", new Vector3(0f, 0f, gap), 180f);
        var feetA = new Footwork(a, 0) { Mode = Tempo(Environment.GetEnvironmentVariable("FOOT_A")) };
        var feetB = new Footwork(b, 1) { Mode = Tempo(Environment.GetEnvironmentVariable("FOOT_B")) };
        var measuresA = new LocoMeasures(a.Anim, a.Loco, a.Poser);
        var measuresB = new LocoMeasures(b.Anim, b.Loco, b.Poser);
        var filmA = new LocoFilm(1f / 60f, 2) { BladeLength = a.Poser.WeaponLength, ShieldRadius = 0.3f };
        var filmB = new LocoFilm(1f / 60f, 2) { BladeLength = b.Poser.WeaponLength, ShieldRadius = 0.3f };
        const float dt = 1f / 60f;
        float nearest = 99f, farthest = 0f, sum = 0f, circled = 0f, lastBearing = float.NaN, circlingFor = 0f, worst = 0f;
        int samples = 0, turnsA = 0, turnsB = 0;
        string worstWhat = "", lastA = "", lastB = "";
        string wasShotA = "", wasShotB = "";
        var lastPos = new Dictionary<(ClipFighter, PartId), Vector3>();
        float settled = -1f;
        for (int i = 0; i < Mathf.RoundToInt(seconds / dt); i++)
        {
            float t = i * dt;
            float keep = ClipFighter.FightingDistance(a.Size, b.Size);
            a.KeepDistance = b.KeepDistance = keep;
            feetA.Steer(b, feetB, dt, out var wantA, out float yawA);
            feetB.Steer(a, feetA, dt, out var wantB, out float yawB);
            a.Move(dt, wantA, yawA, b.Loco.Position, false);
            b.Move(dt, wantB, yawB, a.Loco.Position, false);
            ClipFighter.KeepApart(a, b);
            a.Apply(dt);
            b.Apply(dt);
            measuresA.Sample(dt, t, feetA.Doing);
            measuresB.Sample(dt, t, feetB.Doing);
            filmA.Add(t, feetA.Doing, a.Loco, a.Poser, a.Anim);
            filmB.Add(t, feetB.Doing, b.Loco, b.Poser, b.Anim);
            if (feetA.Doing != lastA || feetB.Doing != lastB)
            {
                Vector3 d0 = b.Loco.Position - a.Loco.Position;
                Console.WriteLine(string.Format(ci, "{0,6:0.00} s  {1:0.00} m apart (A wants {2:0.00}, B {3:0.00})  A: {4}  B: {5}", t, new Vector2(d0.x, d0.z).magnitude, feetA.Want, feetB.Want, feetA.Doing, feetB.Doing));
                lastA = feetA.Doing;
                lastB = feetB.Doing;
            }

            string shotA = a.Loco.Current == Locomotor.Mode.OneShot && a.Loco.Shot != null ? a.Loco.Shot.Name : "";
            string shotB = b.Loco.Current == Locomotor.Mode.OneShot && b.Loco.Shot != null ? b.Loco.Shot.Name : "";
            if (shotA != wasShotA && shotA.Contains("turn")) turnsA++;
            if (shotB != wasShotB && shotB.Contains("turn")) turnsB++;
            wasShotA = shotA;
            wasShotB = shotB;
            Vector3 d = b.Loco.Position - a.Loco.Position;
            d.y = 0f;
            float apart = d.magnitude;
            if (settled < 0f && Mathf.Abs(feetA.Off) < 0.3f && Mathf.Abs(feetB.Off) < 0.3f)
            {
                settled = t;
            }

            if (settled >= 0f)
            {
                nearest = Mathf.Min(nearest, apart);
                farthest = Mathf.Max(farthest, apart);
                sum += apart;
                samples++;
                float bearing = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
                if (!float.IsNaN(lastBearing))
                {
                    circled += Mathf.Abs(Mathf.DeltaAngle(lastBearing, bearing));
                }

                lastBearing = bearing;
                if (Mathf.Abs(feetA.Circling) > 0f || Mathf.Abs(feetB.Circling) > 0f)
                {
                    circlingFor += dt;
                }
            }

            foreach (var f in new[] { a, b })
            {
                foreach (var id in new[] { PartId.HandR, PartId.HandL, PartId.Head, PartId.FootL, PartId.FootR, PartId.Pelvis })
                {
                    Vector3 now = f.Anim.Part(id).position;
                    if (lastPos.TryGetValue((f, id), out var was))
                    {
                        float v = (now - was).magnitude / dt;
                        if (v > worst)
                        {
                            worst = v;
                            worstWhat = $"{(f == a ? "A" : "B")}'s {id} at {t:0.00} s ({f.Loco.Current} {f.Loco.Shot?.Name})";
                        }
                    }

                    lastPos[(f, id)] = now;
                }
            }
        }

        File.WriteAllText(Path.Combine(dir, "a.json"), filmA.ToString());
        File.WriteAllText(Path.Combine(dir, "b.json"), filmB.ToString());
        foreach (var (name, m) in new[] { ("A", measuresA), ("B", measuresB) })
        {
            Console.WriteLine($"{name}'s feet: " + m.Report().Split('\n')[0].Trim());
            foreach (var note in m.Notes)
            {
                if (note.Contains("while planted"))
                {
                    Console.WriteLine("    " + note);
                }
            }
        }

        Console.WriteLine(string.Format(ci, "Tempo A {0}, B {1}. Settled at {2:0.0} s; after it {3:0.00} to {4:0.00} m apart, {5:0.00} on average (the fighting distance {6:0.00}); circled {7:0} degrees round each other in {8:0.0} s of circling; turns: A {9}, B {10}; fastest part {11:0.0} m/s ({12})",
            feetA.Mode, feetB.Mode, settled, nearest, farthest, samples > 0 ? sum / samples : 0f, ClipFighter.FightingDistance(a.Size, b.Size), circled, circlingFor, turnsA, turnsB, worst, worstWhat));
        return 0;
    }

    private static Footwork.Tempo Tempo(string name) =>
        name == "gather" ? Footwork.Tempo.Gather : name == "press" ? Footwork.Tempo.Press : name == "shell" ? Footwork.Tempo.Shell : Footwork.Tempo.Probe;

    /// <summary>A Medium with a sword and shield (as spar makes it), its strikes known (for its reach), in its guard.</summary>
    private static ClipFighter SparFighter(string name, Vector3 at, float facing)
    {
        var rigFile = RigFile.Parse(File.ReadAllText(Path.Combine(Application.dataPath, "_Battler/Data/Rigs/medium_rig.json")));
        var profile = UnitProfile.CreateDefault();
        profile.body = "Medium";
        var root = UnitBuilder.Build(rigFile, profile, name, _ => null, name == "A" ? 0 : 1);
        var f = new ClipFighter(root, root.GetComponent<UnitRig>(), profile, at, facing, 0f, "sns_", physical: false);
        f.Poser.BodyKey = "Medium";
        f.Rig.SlotWeaponR.localRotation = UnitController.MainGrip;
        f.Anim.SlotWeaponR.localRotation = UnitController.MainGrip;
        f.Poser.CarryR = LocoBody.Carry.Weapon;
        f.Poser.BladeR = UnitController.MainGrip * Vector3.forward;
        var sword = WeaponDefinition.CreateDefault(WeaponKind.Sword);
        f.Poser.WeaponLength = sword.length * f.Size;
        f.Rig.SlotWeaponL.localRotation = UnitController.ShieldGrip;
        f.Anim.SlotWeaponL.localRotation = UnitController.ShieldGrip;
        f.Poser.CarryL = LocoBody.Carry.Shield;
        f.Stance = ClipFighter.Stances.Shield;
        f.EnableStrikes(sword);
        return f;
    }
}
