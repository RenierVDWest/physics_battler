// `dummyhits <dir> [body] [clips]`: the combat plan's step 4 headless. Each strike clip is played from the guard,
// aimed at a dummy (a second body, kinematic here: the stub has no physics solver) standing at the strike's
// reach, at its chest and at its head, with its shield down and then up. For each it prints what the blade
// met, how the blow ended (cut through, stopped and backed out, blocked) and how long it took to end.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using PhysicsBattler.Combat;
using PhysicsBattler.Locomotion;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

internal static class DummyHitReport
{
    public static int Run(string dir, string body, string which)
    {
        Directory.CreateDirectory(dir);
        body ??= "Medium";
        LocoLibrary.Load();
        var ci = CultureInfo.InvariantCulture;
        string prefix = body == "Large" ? "lg_" : "sns_";
        var names = LocoLibrary.Clips.Where(p => p.Value.Role == "strike" && (which == null ? p.Key.StartsWith(prefix) : which.Split(',').Contains(p.Key))).Select(p => p.Key).OrderBy(n => n).ToList();
        int landed = 0, blocked = 0, missed = 0, tries = 0;
        foreach (string name in names)
        {
            var clip = LocoLibrary.Get(name);
            foreach (var stance in new[] { LabDummy.Stances.Open, LabDummy.Stances.Block })
            {
                foreach (var part in new[] { PartId.Chest, PartId.Head })   // (the middle and the high target)
                {
                    var (anim, loco, poser, root) = Walker(body);
                    float size = (anim.RestLocalPosition(PartId.ShinL).magnitude + anim.RestLocalPosition(PartId.FootL).magnitude) / 0.845f;
                    var weapon = WeaponDefinition.CreateDefault(body == "Large" ? WeaponKind.Club : WeaponKind.Sword);
                    poser.WeaponLength = weapon.length * size;
                    var hits = new StrikeHits(loco, poser, anim) { Weapon = weapon, Scale = size, Mass = 70f * size * size * size };

                    // The dummy (a Medium) where the strike reaches, facing the walker.
                    float reach = 1.6f;
                    if (StrikeReach.TryGet(body, name, out var grip, out var way))
                    {
                        Vector3 sweet = grip + way * (poser.WeaponLength * poser.SweetSpot);
                        reach = new Vector2(sweet.x, sweet.z).magnitude;
                    }

                    var dummyRig = RigFile.Parse(File.ReadAllText(Path.Combine(Application.dataPath, "_Battler/Data/Rigs/medium_rig.json")));
                    var dprofile = UnitProfile.CreateDefault();
                    dprofile.body = "Medium";
                    var droot = UnitBuilder.Build(dummyRig, dprofile, "Dummy", _ => null, 1);
                    var dummy = new LabDummy(droot, droot.GetComponent<UnitRig>(), dprofile, new Vector3(0f, 0f, reach), 180f, 0f, "sns_", physical: false);
                    // (Its shield: a board on its left hand's slot, as a shield's collider is.)
                    var shield = new GameObject("Dummy shield");
                    shield.transform.SetParent(dummy.Rig.SlotWeaponL, false);
                    shield.transform.localRotation = UnitController.ShieldGrip;
                    var board = shield.AddComponent<BoxCollider>();
                    board.size = new Vector3(0.6f, 0.6f, 0.05f);
                    board.isTrigger = true;
                    dummy.ShieldColliders.Add(board);
                    dummy.Stance = stance;
                    const float dt = 1f / 60f;
                    for (int i = 0; i < 60; i++)
                    {
                        loco.Step(dt, Vector3.zero, 0f);
                        poser.Pose(dt);
                        dummy.Step(dt, loco.Position, false);
                    }

                    int height = part == PartId.Head ? 2 : 1;
                    Vector3 target = dummy.TargetAt(height);
                    dummy.BlockAt = target;
                    for (int i = 0; i < 20; i++)
                    {
                        loco.Step(dt, Vector3.zero, 0f);
                        poser.Pose(dt);
                        dummy.Step(dt, loco.Position + Vector3.up * 1.3f * size, false);
                    }

                    target = dummy.TargetAt(height);
                    dummy.BlockAt = target;
                    poser.PlayStrike(clip, target);
                    var film = new LocoFilm(dt, 2) { BladeLength = poser.WeaponLength };
                    var dummyFilm = new LocoFilm(dt, 2) { ShieldRadius = 0.3f };
                    float started = 0f, ended = -1f, mostLean = 0f;
                    string what = "missed";
                    hits.Landed += text => what = text.Substring(text.IndexOf(':') + 2);
                    for (int i = 0; i < 400; i++)
                    {
                        bool frozen = hits.HitStop > 0f;
                        if (!frozen)
                        {
                            loco.Step(dt, Vector3.zero, loco.Facing);
                            poser.Pose(dt);
                        }

                        hits.Step(dt, dummy);
                        dummy.Step(dt, loco.Position + Vector3.up * 1.3f * size, frozen);
                        film.Add(i * dt, name, loco, poser, anim);
                        mostLean = Mathf.Max(mostLean, dummy.Lean);
                        dummyFilm.Add(i * dt, "dummy", dummy.Loco, dummy.Poser, dummy.Anim);
                        if (loco.Current != Locomotor.Mode.OneShot && ended < 0f)
                        {
                            ended = i * dt;
                        }

                        if (ended >= 0f && i * dt > ended + 1f)
                        {
                            break;
                        }
                    }

                    File.WriteAllText(Path.Combine(dir, $"{name}-{stance}-{part}.json".ToLowerInvariant()), film.ToString());
                    File.WriteAllText(Path.Combine(dir, $"{name}-{stance}-{part}.dummy".ToLowerInvariant()), dummyFilm.ToString());

                    tries++;
                    if (what == "missed") missed++;
                    else if (what.StartsWith("blocked")) blocked++;
                    else landed++;
                    Console.WriteLine(string.Format(ci, "{0,-20} at the {1,-5} {2}: {3}; the blow ended after {4:0.00} s (the clip is {5:0.00} s); the dummy leaned {6:0} degrees (mass {7:0} kg)",
                        name, part.ToString().ToLowerInvariant(), stance == LabDummy.Stances.Open ? "open " : "block", what, ended, clip.Duration, mostLean * Mathf.Rad2Deg, dummy.Mass));
                    UnityEngine.Object.DestroyImmediate(root);
                    UnityEngine.Object.DestroyImmediate(droot);
                }
            }
        }

        Console.WriteLine($"All: {landed} landed, {blocked} blocked, {missed} missed of {tries}");
        return 0;
    }

    private static (AnimRig, Locomotor, LocoBody, GameObject) Walker(string body)
    {
        var rig = RigFile.Parse(File.ReadAllText(Path.Combine(Application.dataPath, $"_Battler/Data/Rigs/{body.ToLowerInvariant()}_rig.json")));
        var profile = UnitProfile.CreateDefault();
        profile.body = body;
        var root = UnitBuilder.Build(rig, profile, "Striker " + body, _ => null, 0);
        var unitRig = root.GetComponent<UnitRig>();
        var anim = new AnimRig(root.transform, unitRig);
        float leg = anim.RestLocalPosition(PartId.ShinL).magnitude + anim.RestLocalPosition(PartId.FootL).magnitude;
        var loco = new Locomotor(leg);
        loco.SetGuard(body == "Large" ? "lg_" : "sns_");
        loco.GuardWanted = true;
        var poser = new LocoBody(anim, loco) { BodyKey = body, TwoHanded = body == "Large" };
        poser.SoleFrom(unitRig);
        anim.SlotWeaponR.localRotation = UnitController.MainGrip;
        poser.BladeR = UnitController.MainGrip * Vector3.forward;
        poser.CarryR = LocoBody.Carry.Weapon;
        loco.Place(Vector3.zero, 0f);
        // (The striker's own colliders are not in the dummy's: the sweep only counts the dummy's.)
        return (anim, loco, poser, root);
    }
}
