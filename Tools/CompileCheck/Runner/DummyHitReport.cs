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
        // (LEG_FALLS=1: the shelved leg sweeps on.)
        ClipFighter.LegFalls = Environment.GetEnvironmentVariable("LEG_FALLS") != null;
        var ci = CultureInfo.InvariantCulture;
        var allShieldMeets = new List<StrikeHits.ShieldMeet>();
        var steadies = new List<ShieldSteadiness>();
        if (Environment.GetEnvironmentVariable("SHIELD_PLAN") != null) ClipFighter.ShieldTrace = line => Console.WriteLine("    the shield: " + line);
        int allInside = 0;
        string prefix = LocoLibrary.CombatPrefix(body);
        var names = LocoLibrary.Clips.Where(p => p.Value.Role == "strike" && (which == null ? p.Key.StartsWith(prefix) && LocoLibrary.StrikeFor(body, p.Key) : which.Split(',').Contains(p.Key))).Select(p => p.Key).OrderBy(n => n).ToList();
        if (Environment.GetEnvironmentVariable("PARRY_CROSSOUT") != null)
        {
            SwordParry.CrossOut = float.Parse(Environment.GetEnvironmentVariable("PARRY_CROSSOUT"), CultureInfo.InvariantCulture);
        }

        if (Environment.GetEnvironmentVariable("PARRY_ARMSPEED") != null)
        {
            SwordParry.UpperArmSpeed = float.Parse(Environment.GetEnvironmentVariable("PARRY_ARMSPEED"), CultureInfo.InvariantCulture);
        }

        if (Environment.GetEnvironmentVariable("PARRY_HEADFRONT") != null)
        {
            SwordParry.HeadFront = float.Parse(Environment.GetEnvironmentVariable("PARRY_HEADFRONT"), CultureInfo.InvariantCulture);
        }

        SwordParry.NoClear = Environment.GetEnvironmentVariable("PARRY_NOCLEAR") != null;
        if (Environment.GetEnvironmentVariable("PARRY_TRACE") != null)
        {
            SwordParry.Trace = line => Console.WriteLine("        " + line);
        }

        int landed = 0, blocked = 0, missed = 0, tries = 0, swordTries = 0, swordParried = 0;
        float worstWrist = 0f, worstJump = 0f, worstHand = 0f, closest = 99f, worstSword = 99f;
        int allTouches = 0;
        foreach (string name in names)
        {
            var clip = LocoLibrary.Get(name);
            string onlyStance = Environment.GetEnvironmentVariable("DUMMY_STANCE");
            foreach (var stance in new[] { ClipFighter.Stances.Open, ClipFighter.Stances.Shield, ClipFighter.Stances.Sword })
            {
                if (onlyStance != null && !string.Equals(onlyStance, stance.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                foreach (var part in new[] { PartId.Pelvis, PartId.Chest, PartId.Head })   // (the low, the middle and the high target)
                {
                    var (anim, loco, poser, root, walker) = Walker(body);
                    float size = (anim.RestLocalPosition(PartId.ShinL).magnitude + anim.RestLocalPosition(PartId.FootL).magnitude) / 0.845f;
                    // (DUMMY_WEAPON=club | sword: the attacker's weapon; by default a Large's club, any other's sword.)
                    string weaponArg = Environment.GetEnvironmentVariable("DUMMY_WEAPON");
                    var weapon = WeaponDefinition.CreateDefault(weaponArg == "club" ? WeaponKind.Club : weaponArg == "sword" ? WeaponKind.Sword : body == "Large" ? WeaponKind.Club : WeaponKind.Sword);
                    poser.WeaponLength = weapon.length * size;
                    var hits = walker.EnableStrikes(weapon);
                    var steady = new ShieldSteadiness();
                    steadies.Add(steady);
                    hits.Mass = 70f * size * size * size;

                    // The dummy (a Medium) where the strike reaches, facing the walker.
                    float reach = 1.6f;
                    if (StrikeReach.TryGet(body, name, out var grip, out var way))
                    {
                        Vector3 sweet = grip + way * (poser.WeaponLength * poser.SweetSpot);
                        reach = new Vector2(sweet.x, sweet.z).magnitude;
                    }

                    // (From the standing distance, as the lab sets them up.)
                    float ownReach = reach;
                    reach = Mathf.Max(reach, Environment.GetEnvironmentVariable("STAND") != null ? float.Parse(Environment.GetEnvironmentVariable("STAND"), CultureInfo.InvariantCulture) * size : ClipFighter.StandingDistance(size, 1f));

                    var dummyRig = RigFile.Parse(File.ReadAllText(Path.Combine(Application.dataPath, "_Battler/Data/Rigs/medium_rig.json")));
                    var dprofile = UnitProfile.CreateDefault();
                    dprofile.body = "Medium";
                    var droot = UnitBuilder.Build(dummyRig, dprofile, "Dummy", _ => null, 1);
                    var dummy = new ClipFighter(droot, droot.GetComponent<UnitRig>(), dprofile, new Vector3(0f, 0f, reach), 180f, 0f, "sns_", physical: false);
                    if (float.TryParse(Environment.GetEnvironmentVariable("PARRY_SKILL"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float parrySkill))
                    {
                        dummy.ParrySkill = parrySkill;
                    }

                    if (float.TryParse(Environment.GetEnvironmentVariable("SHIELD_SKILL"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float shieldSkill))
                    {
                        dummy.ShieldSkill = shieldSkill;
                    }
                    // (Its sword, as the lab holds it.)
                    dummy.Rig.SlotWeaponR.localRotation = UnitController.MainGrip;
                    dummy.Anim.SlotWeaponR.localRotation = UnitController.MainGrip;
                    dummy.Poser.CarryR = LocoBody.Carry.Weapon;
                    dummy.Poser.BladeR = UnitController.MainGrip * Vector3.forward;
                    var dummySword = WeaponDefinition.CreateDefault(WeaponKind.Sword);
                    dummy.Poser.WeaponLength = dummySword.length * dummy.Size;
                    if (stance == ClipFighter.Stances.Sword)
                    {
                        dummy.Parry = new SwordParry(dummy.Anim, dummy.Loco, dummy.Rig, dummy.Size, dummySword.length * dummy.Size)
                        {
                            Opponent = new OpponentShape(anim, size, poser.CarryL == LocoBody.Carry.Shield ? anim.SlotWeaponL : null, 0.27f) { WeaponSlot = anim.SlotWeaponR, WeaponLength = poser.WeaponLength },
                        };
                    }

                    // (Its shield: a board on its left hand's slot, as a shield's collider is.)
                    var shield = new GameObject("Dummy shield");
                    // (Held as the lab holds it: the slots turned by the shield's grip, the board's face the slot's forward.)
                    dummy.Rig.SlotWeaponL.localRotation = UnitController.ShieldGrip;
                    dummy.Anim.SlotWeaponL.localRotation = UnitController.ShieldGrip;
                    shield.transform.SetParent(dummy.Rig.SlotWeaponL, false);
                    var board = shield.AddComponent<BoxCollider>();
                    board.size = new Vector3(0.522f, 0.522f, 0.05f);   // (P_Weapon_Shield's)
                    board.center = new Vector3(0f, 0f, 0.075f);
                    board.isTrigger = true;
                    if (stance == ClipFighter.Stances.Shield)
                    {
                        dummy.ShieldColliders.Add(board);
                    }
                    else
                    {
                        board.enabled = false;
                    }
                    dummy.Stance = stance;
                    const float dt = 1f / 60f;
                    for (int i = 0; i < 60; i++)
                    {
                        walker.Move(dt, Vector3.zero, 0f, dummy.Loco.Position, false);
                        dummy.Step(dt, loco.Position, false);
                    }

                    int height = part == PartId.Head ? 2 : part == PartId.Chest ? 1 : 0;
                    // (DUMMY_LOW=1: the low blows at its knees instead of its hips, to take its legs out.)
                    if (height == 0 && Environment.GetEnvironmentVariable("DUMMY_LOW") != null)
                    {
                        height = -1;
                    }
                    Vector3 target = dummy.TargetAt(height);
                    dummy.BlockAt = target;
                    for (int i = 0; i < 20; i++)
                    {
                        walker.Move(dt, Vector3.zero, 0f, dummy.Loco.Position, false);
                        dummy.Step(dt, loco.Position + Vector3.up * 1.3f * size, false);
                    }

                    target = dummy.TargetAt(height);
                    // (A strike that can't reach from the standing distance is stood in for, as the lab does: only as
                    // near as it needs, never nearer than the fighting distance.)
                    {
                        float least = ClipFighter.FightingDistance(size, dummy.Size) + 0.1f * size;
                        poser.KeepFrom = dummy.Loco.Position;
                        poser.KeepDistance = ClipFighter.FightingDistance(size, dummy.Size);
                        // (Else from its own reach, as near as the fighting distance lets it.)
                        float zOwn = Mathf.Max(0f, reach - Mathf.Max(ownReach, least)), z = zOwn;
                        for (float zz = 0f; zz < zOwn; zz += 0.05f * size)
                        {
                            if (poser.MissAfterPull(clip, target, 0f, new Vector3(0f, 0f, zz), 0f) <= LocomotionLab.LandsNear * size)
                            {
                                z = zz;
                                break;
                            }
                        }

                        if (z > 0f)
                        {
                            if (Environment.GetEnvironmentVariable("STEP_TRACE") != null) Console.WriteLine(string.Format(ci, "    stepped in {0:0.00} m from {1:0.00} m (miss from there {2:0.00} m)", z, reach, poser.MissAfterPull(clip, target, 0f, Vector3.zero, 0f)));
                            loco.Place(new Vector3(0f, 0f, z), 0f);
                            for (int i = 0; i < 20; i++)
                            {
                                walker.Move(dt, Vector3.zero, 0f, dummy.Loco.Position, false);
                                dummy.Step(dt, loco.Position + Vector3.up * 1.3f * size, false);
                            }

                            target = dummy.TargetAt(height);
                        }
                    }

                    dummy.BlockAt = target;
                    Vector3 dummyStart = dummy.Loco.Position;
                    poser.KeepFrom = dummy.Loco.Position;
                    poser.KeepDistance = ClipFighter.FightingDistance(size, dummy.Size);
                    poser.PlayStrike(clip, target);
                    var film = new LocoFilm(dt, 2) { BladeLength = poser.WeaponLength };
                    var dummyFilm = new LocoFilm(dt, 2) { ShieldRadius = stance == ClipFighter.Stances.Shield ? 0.3f : 0f, BladeLength = stance == ClipFighter.Stances.Sword ? dummySword.length * dummy.Size : 0f };
                    float started = 0f, ended = -1f, mostLean = 0f, mostMoved = 0f, nearest = 99f, handNearest = 99f, swordNearest = 99f, guardNearest = 99f, guardWhen = 0f, mostBladeTurn = 0f; Vector3 lastBladeWay = Vector3.zero, lastBladeDelta = Vector3.zero; int buzz = 0;
                    string what = "missed";
                    bool predicted = false; Vector3[] predTip = null, predStrike = null; float predFrom = 0f;
                    hits.Landed += text => what = text.Substring(text.IndexOf(':') + 2);
                    for (int i = 0; i < 400; i++)
                    {
                        float keepAt = ClipFighter.FightingDistance(size, dummy.Size);
                        poser.KeepFrom = dummy.Loco.Position;
                        poser.KeepDistance = keepAt;
                        dummy.KeepDistance = keepAt;
                        dummy.StandDistance = ClipFighter.StandingDistance(size, dummy.Size);
                        bool frozen = hits.HitStop > 0f;
                        walker.Move(dt, Vector3.zero, loco.Facing, dummy.Loco.Position, frozen);

                        if (Environment.GetEnvironmentVariable("PRED_TRACE") != null && loco.Current == Locomotor.Mode.OneShot && loco.Shot != null)
                        {
                            var back = Quaternion.Inverse(Quaternion.Euler(0f, dummy.Loco.Facing, 0f));
                            Vector3 L(Vector3 w) => back * (w - dummy.Loco.Position);
                            string V3(Vector3 v) => string.Format(ci, "({0:0.00},{1:0.00},{2:0.00})", v.x, v.y, v.z);
                            float contactAt = loco.Shot.ContactFrame / loco.Shot.Rate;
                            if (hits.Read(out var rd) && rd.ToContact <= 0.33f && !predicted)
                            {
                                predicted = true;
                                predTip = rd.TipPath; predStrike = rd.Path;
                                predFrom = loco.ShotTime;
                                Console.WriteLine("    predicted at shot " + loco.ShotTime.ToString("0.00", ci) + ": strike " + string.Join(" ", rd.Path.Select(q => V3(L(q)))));
                            }

                            if (predicted && hits.Blade(loco.Shot, out _, out var tipNow, out var strikeNow) && loco.ShotTime <= contactAt + 0.01f)
                            {
                                Console.WriteLine("    shot " + loco.ShotTime.ToString("0.00", ci) + " (contact " + contactAt.ToString("0.00", ci) + ") strike " + V3(L(strikeNow)) + " tip " + V3(L(tipNow)));
                            }
                        }

                        hits.Step(dt, dummy);
                        // (The dummy watches the blow coming, as the lab has it.)
                        dummy.Watch(hits);

                        dummy.Step(dt, loco.Position + Vector3.up * 1.3f * size, frozen);
                        steady.Step(dummy);
                        if (Environment.GetEnvironmentVariable("SHIELD_STEPS") != null && dummy.IncomingBlade.HasValue && dummy.ShieldBoard(out var sbc, out var sbn, out _))
                        {
                            var bk = Quaternion.Inverse(Quaternion.Euler(0f, dummy.Loco.Facing, 0f));
                            string V3(Vector3 v) => string.Format(ci, "({0:0.00},{1:0.00},{2:0.00})", v.x, v.y, v.z);
                            Console.WriteLine(string.Format(ci, "    {0:0.00} board {1} n {2} | posed hand {3} want {4} face {5} | replans {6} hitstop {7:0.00}", i * dt,
                                V3(bk * (sbc - dummy.Loco.Position)), V3(bk * sbn), V3(bk * (dummy.Anim.Part(PartId.HandL).position - dummy.Loco.Position)),
                                V3(bk * (dummy.DebugWant - dummy.Loco.Position)), V3(bk * dummy.DebugFace), dummy.Replans, hits.HitStop));
                        }
                        if (Environment.GetEnvironmentVariable("SHIELD_TRACE") != null && dummy.IncomingBlade.HasValue)
                        {
                            Vector3 sc = board.transform.position, n = board.transform.forward;
                            Vector3 bl = dummy.IncomingBlade.Value;
                            Console.WriteLine(string.Format(ci, "  t {0:0.00} shot {1:0.00} blade {2:0.00},{3:0.00},{4:0.00} shield {5:0.00},{6:0.00},{7:0.00} gap {8:0.00} face-to-blade {9:0}",
                                i * dt, loco.ShotTime, bl.x, bl.y, bl.z, sc.x, sc.y, sc.z, Vector3.Distance(bl, sc), Vector3.Angle(n, bl - sc)));
                        }
                        film.Add(i * dt, name, loco, poser, anim);
                        mostLean = Mathf.Max(mostLean, dummy.Lean);
                        nearest = Mathf.Min(nearest, new Vector2(dummy.Loco.Position.x - loco.Position.x, dummy.Loco.Position.z - loco.Position.z).magnitude);
                        Vector3 handW = anim.Part(PartId.HandR).position;
                        handNearest = Mathf.Min(handNearest, new Vector2(dummy.Loco.Position.x - handW.x, dummy.Loco.Position.z - handW.z).magnitude);
                        mostMoved = Mathf.Max(mostMoved, new Vector2(dummy.Loco.Position.x - dummyStart.x, dummy.Loco.Position.z - dummyStart.z).magnitude);
                        dummyFilm.Add(i * dt, "dummy", dummy.Loco, dummy.Poser, dummy.Anim);
                        if (dummy.Parry != null && dummy.Parry.Opponent != null)
                        {
                            dummy.Parry.BladeNow(out var dg, out var dt2);
                            Vector3 bw = (dt2 - dg).normalized;
                            if (lastBladeWay != Vector3.zero && dummy.Parry.Current < 0)
                            {
                                float turnStep = Vector3.Angle(bw, lastBladeWay);
                                mostBladeTurn = Mathf.Max(mostBladeTurn, turnStep);
                                if (turnStep > 20f && Environment.GetEnvironmentVariable("PARRY_TRACE") != null) Console.WriteLine(string.Format(ci, "    the guard's blade turned {0:0} deg at {1:0.00} s (hit stop {2:0.00})", turnStep, i * dt, hits.HitStop));
                            }
                            if (lastBladeWay != Vector3.zero)
                            {
                                Vector3 delta = bw - lastBladeWay;
                                if (dummy.Parry.Current < 0 && lastBladeDelta.magnitude > 0.008f && delta.magnitude > 0.008f && Vector3.Dot(delta, lastBladeDelta) < 0f)
                                {
                                    buzz++;
                                    if (Environment.GetEnvironmentVariable("BUZZ_TRACE") != null) Console.WriteLine(string.Format(ci, "    the guard's blade turned back at {0:0.00} s ({1:0.0} then {2:0.0} deg; hit stop {3:0.00})", i * dt, Mathf.Asin(Mathf.Min(1f, lastBladeDelta.magnitude)) * Mathf.Rad2Deg, Mathf.Asin(Mathf.Min(1f, delta.magnitude)) * Mathf.Rad2Deg, hits.HitStop));
                                }
                                lastBladeDelta = delta;
                            }

                            lastBladeWay = bw;
                            float cl = dummy.Parry.Opponent.Clearance(dg, dt2);
                            swordNearest = Mathf.Min(swordNearest, cl);
                            if (dummy.Parry.Current < 0 && cl < guardNearest) { guardNearest = cl; guardWhen = i * dt; }
                        }
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
                    else if (what.StartsWith("blocked") || what.StartsWith("parried")) blocked++;
                    else landed++;
                    Console.WriteLine(string.Format(ci, "{0,-20} at the {1,-6} {2,-6}: {3}; the dummy leaned {4:0} degrees, {5}, and was moved {6:0.00} m{7}{8}",
                        name, part == PartId.Pelvis ? "hips" : part.ToString().ToLowerInvariant(), stance.ToString().ToLowerInvariant(), what, mostLean * Mathf.Rad2Deg, dummy.LastReaction, mostMoved,
                        dummy.Falls + dummy.Stumbles > 0 ? $" [{(dummy.Falls > 0 ? "fell" : "stumbled")}; feet down {(dummy.Poser.Feet[0].Locked ? "L" : "-")}{(dummy.Poser.Feet[1].Locked ? "R" : "-")}]" : $" [stood; feet down {(dummy.Poser.Feet[0].Locked ? "L" : "-")}{(dummy.Poser.Feet[1].Locked ? "R" : "-")}, mass {dummy.Mass:0} kg]",
                        dummy.LastBend > 0f && what != "missed" && !what.StartsWith("blocked") && !what.StartsWith("parried") ? string.Format(ci, "; bent {0:0.00} m at its peak", dummy.LastBend) : ""));
                    Console.WriteLine(string.Format(ci, "      the bodies came within {0:0.00} m (root to root; the fighting distance {1:0.00} m), the weapon hand within {5:0.00} m of its middle; the lunge x{2:0.00}; touches {3}{4}",
                        nearest, ClipFighter.FightingDistance(size, dummy.Size), poser.AimLunge, hits.Touches, hits.Deflects > 0 ? $"; glanced off the sword {hits.Deflects}" : "", handNearest));
                    allTouches += hits.Touches;
                    closest = Mathf.Min(closest, nearest);
                    if (dummy.Parry != null)
                    {
                        var pr = dummy.Parry;
                        Console.WriteLine(string.Format(ci, "      parries made: {0}; the arm: wrist short at most {1:0} deg, the elbow's biggest jump {2:0} deg a step, the hand at most {3:0.0} m/s, the shoulder past {4:0} deg, into the torso {5:0.00} m",
                            pr.Made.Length > 0 ? pr.Made : "none", pr.MostWristShort, pr.MostSwivelJump, pr.MostHandSpeed, pr.MostShoulderPast, pr.MostIntoTorso));
                        if (dummy.Parry.Pushed > 0) Console.WriteLine("      the dummy's sword was pushed out of the attacker " + dummy.Parry.Pushed + " times (5 deg each)");
                        Console.WriteLine(string.Format(ci, "      the dummy's sword came within {0:0.00} m of the attacker's body{1} (below 0: into it); in its guard {2:0.00} m (at {3:0.00} s), the guard's blade turning at most {4:0} deg a step; its blade turned back on itself {5} times", swordNearest, dummy.Parry.Opponent != null && dummy.Parry.Opponent.HasShield ? " and shield" : "", guardNearest, guardWhen, mostBladeTurn, buzz));
                        worstSword = Mathf.Min(worstSword, swordNearest);
                        swordTries++;
                        if (what.StartsWith("parried")) swordParried++;
                        worstWrist = Mathf.Max(worstWrist, pr.MostWristShort);
                        worstJump = Mathf.Max(worstJump, pr.MostSwivelJump);
                        worstHand = Mathf.Max(worstHand, pr.MostHandSpeed);
                    }
                    allShieldMeets.AddRange(hits.ShieldMeets);
                    allInside += hits.InsideOutside;
                    UnityEngine.Object.DestroyImmediate(root);
                    UnityEngine.Object.DestroyImmediate(droot);
                }
            }
        }

        ShieldMeetSummary.Print(allShieldMeets);
        foreach (var st in steadies) st.Close();
        ShieldSteadiness.Print(steadies);
        Console.WriteLine($"Outside its blows the blade was in a body or a shield {allInside} steps");
        Console.WriteLine($"All: {landed} landed, {blocked} blocked or parried, {missed} missed of {tries}; the bodies came within {closest:0.00} m; {allTouches} touches outside the blows");
        if (swordTries > 0)
        {
            Console.WriteLine(string.Format(ci, "The sword: parried {0} of {1}; the arm's worst: wrist short {2:0} deg, elbow jump {3:0} deg a step, hand {4:0.0} m/s",
                swordParried, swordTries, worstWrist, worstJump, worstHand));
        }
        return 0;
    }

    /// <summary>The striker: a clip fighter (not physical; it doesn't defend: nothing strikes it), in its guard at the origin facing +z.</summary>
    internal static (AnimRig, Locomotor, LocoBody, GameObject, ClipFighter) Walker(string body)
    {
        var rig = RigFile.Parse(File.ReadAllText(Path.Combine(Application.dataPath, $"_Battler/Data/Rigs/{body.ToLowerInvariant()}_rig.json")));
        var profile = UnitProfile.CreateDefault();
        profile.body = body;
        var root = UnitBuilder.Build(rig, profile, "Striker " + body, _ => null, 0);
        var unitRig = root.GetComponent<UnitRig>();
        var walker = new ClipFighter(root, unitRig, profile, Vector3.zero, 0f, 0f, LocoLibrary.CombatPrefix(body), physical: false) { Defends = false };
        var anim = walker.Anim;
        var loco = walker.Loco;
        var poser = walker.Poser;
        poser.BodyKey = body;
        poser.TwoHanded = LocoLibrary.TwoHanded(body);
        anim.SlotWeaponR.localRotation = UnitController.MainGrip;
        poser.BladeR = UnitController.MainGrip * Vector3.forward;
        poser.CarryR = LocoBody.Carry.Weapon;
        if (!LocoLibrary.TwoHanded(body) && Environment.GetEnvironmentVariable("NO_SHIELD") == null)
        {
            // (Its shield, as the lab's sword-and-shield loadout holds it.)
            anim.SlotWeaponL.localRotation = UnitController.ShieldGrip;
            unitRig.SlotWeaponL.localRotation = UnitController.ShieldGrip;
            poser.CarryL = LocoBody.Carry.Shield;
        }

        loco.Place(Vector3.zero, 0f);
        // (The striker's own colliders are not in the dummy's: the sweep only counts the dummy's.)
        return (anim, loco, poser, root, walker);
    }
}
