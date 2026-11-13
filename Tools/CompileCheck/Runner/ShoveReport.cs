// `shoves`: one fighter standing in its guard is shoved, at several speeds and from four sides, and left to come to
// rest (the plan "Grounded legs", step 4: steps where they are needed). For each: how many steps its feet took, how long
// until it stood again, how far its body went, whether it walked, whether both feet were ever off the ground, how far the
// leg furthest from its foot was stretched, and how far its feet stood from the stance's when it had stopped.
// SHOVE_TO=1: a shove is stepped in the stance only up to a walk's speed (as before step 4). SHOVE_STANCE=0: every shove is walked off.
using System;
using System.Globalization;
using System.IO;
using PhysicsBattler.Combat;
using PhysicsBattler.Locomotion;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

internal static class ShoveReport
{
    public static int Run()
    {
        LocoLibrary.Load();
        var ci = CultureInfo.InvariantCulture;
        if (Environment.GetEnvironmentVariable("SHOVE_STANCE") == "0")
        {
            Locomotor.ShovesInStance = false;
        }

        var f = Fighter();
        if (float.TryParse(Environment.GetEnvironmentVariable("SHOVE_TO"), NumberStyles.Float, ci, out float shoveTo))
        {
            Locomotor.ShoveSteppedTo = shoveTo;
        }

        const float dt = 1f / 60f;
        void Step()
        {
            f.Move(dt, Vector3.zero, f.Loco.Facing, f.Loco.Position + Quaternion.Euler(0f, f.Loco.Facing, 0f) * Vector3.forward * 3f, false);
            f.Apply(dt);
        }

        // (Into its guard, and settled in it.)
        for (int i = 0; i < 240; i++)
        {
            Step();
        }

        Console.WriteLine($"a Medium in its guard, shoved (shoves stepped in the stance {(Locomotor.ShovesInStance ? $"up to {Locomotor.ShoveSteppedTo:0.0} m/s" : "off")}); it stands {f.Loco.Current}");
        string[] sides = { "back", "forward", "to its left", "to its right" };
        Vector3[] ways = { Vector3.back, Vector3.forward, Vector3.left, Vector3.right };
        foreach (float speed in new[] { 0.3f, 0.6f, 0.9f, 1.3f, 2f })
        {
            for (int s = 0; s < 4; s++)
            {
                Vector3 from = f.Loco.Position;
                f.Loco.Push(Quaternion.Euler(0f, f.Loco.Facing, 0f) * ways[s] * speed);
                int steps = 0, hops = 0, walked = 0, frames = 0, rested = 0;
                float stretched = 0f, took = -1f;
                bool[] was = { f.Poser.Feet[0].Locked, f.Poser.Feet[1].Locked };
                for (int i = 0; i < 300; i++)
                {
                    Step();
                    frames++;
                    for (int k = 0; k < 2; k++)
                    {
                        bool locked = f.Poser.Feet[k].Locked;
                        steps += locked && !was[k] ? 1 : 0;
                        was[k] = locked;
                        Vector3 hip = f.Anim.Part(k == 0 ? PartId.ThighL : PartId.ThighR).position, foot = f.Anim.Part(k == 0 ? PartId.FootL : PartId.FootR).position;
                        stretched = Mathf.Max(stretched, (foot - hip).magnitude / f.Loco.Leg);
                    }

                    hops += !f.Poser.Feet[0].Locked && !f.Poser.Feet[1].Locked ? 1 : 0;
                    walked += f.Loco.Current == Locomotor.Mode.Cycle ? 1 : 0;
                    bool still = f.Poser.Feet[0].Locked && f.Poser.Feet[1].Locked && f.Loco.Velocity.magnitude < 0.03f && f.Loco.Current == Locomotor.Mode.Idle;
                    rested = still ? rested + 1 : 0;
                    if (rested >= 30 && took < 0f)
                    {
                        took = (frames - 30) * dt;
                        break;
                    }
                }

                Console.WriteLine(string.Format(ci, "  {0:0.0} m/s {1,-12}: {2} steps, at rest after {3}, gone {4:0} cm; walking {5:0.00} s, both feet off the ground {6:0.00} s, a leg at most {7:0}% of its length, its feet {8:0} cm from the stance's at the end",
                    speed, sides[s], steps, took < 0f ? "over 5 s" : took.ToString("0.00", ci) + " s", 100f * (f.Loco.Position - from).magnitude, walked * dt, hops * dt, 100f * stretched, 100f * f.Loco.StanceOff));
                for (int i = 0; i < 120; i++)
                {
                    Step();
                }
            }
        }

        return 0;
    }

    private static ClipFighter Fighter()
    {
        var rigFile = RigFile.Parse(File.ReadAllText(Path.Combine(Application.dataPath, "_Battler/Data/Rigs/medium_rig.json")));
        var profile = UnitProfile.CreateDefault();
        profile.body = "Medium";
        var root = UnitBuilder.Build(rigFile, profile, "A", _ => null, 0);
        var f = new ClipFighter(root, root.GetComponent<UnitRig>(), profile, Vector3.zero, 0f, 0f, "sns_", physical: false);
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
