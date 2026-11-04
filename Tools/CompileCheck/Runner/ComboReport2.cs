// `combos <dir> [body]`: the combat plan's step 5 headless. The follow-on cost of every pair of the body's strikes
// (where the first is left and the second taken up), the combo each strike starts, and each combo played at a
// kinematic dummy at its first strike's reach: what each strike did, and how fast the hands and feet moved about
// each link. Films of both bodies are written for each combo.
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

internal static class StrikeComboReport
{
    public static int Run(string dir, string body)
    {
        Directory.CreateDirectory(dir);
        body ??= "Medium";
        LocoLibrary.Load();
        var ci = CultureInfo.InvariantCulture;
        string prefix = LocoLibrary.CombatPrefix(body);
        var strikes = LocoLibrary.Clips.Where(p => p.Value.Role == "strike" && p.Key.StartsWith(prefix) && LocoLibrary.StrikeFor(body, p.Key))
            .OrderBy(p => p.Key).Select(p => p.Value).ToList();
        Console.WriteLine("Follow-on costs (rows: from; columns: to):");
        Console.WriteLine("                     " + string.Join(" ", strikes.Select(s => s.Name.Replace(prefix, "").PadLeft(9).Substring(0, 9))));
        foreach (var a in strikes)
        {
            Console.WriteLine(a.Name.Replace(prefix, "").PadRight(20) + " " + string.Join(" ", strikes.Select(b => a == b ? "        -" : StrikeCombos.Link(a, b).Cost.ToString("0.00", ci).PadLeft(9))));
        }

        if (Environment.GetEnvironmentVariable("COMBO_CURVE") is string pair && pair.Contains(">"))
        {
            // The cost against where the first is left, each place's best entry into the second.
            var ab = pair.Split('>');
            var a = LocoLibrary.Get(ab[0]);
            var b = LocoLibrary.Get(ab[1]);
            foreach (var (at, entry, cost) in StrikeCombos.Curve(a, b))
            {
                Console.WriteLine(string.Format(ci, "    left at {0:0.00} s (contact {1:0.00}, end {2:0.00}): best entry {3:0.00} s, cost {4:0.00}", at, a.ContactFrame / a.Rate, a.Duration, entry, cost));
            }
        }

        int combos = 0, landed = 0, blocked = 0, missed = 0, strikesPlayed = 0;
        float fastest = 0f;
        string fastestWhere = "";
        foreach (var first in strikes)
        {
            var chain = StrikeCombos.Chain(first, strikes, 3);
            Console.WriteLine($"{first.Name}: " + (chain.Count == 0 ? "no follow-on" : string.Join("; ", chain.Select(l => l.ToString()))));
            if (chain.Count == 0)
            {
                continue;
            }

            // Played at the dummy.
            var (anim, loco, poser, root, size) = DummyHitReportWalker(body);
            var weapon = WeaponDefinition.CreateDefault(body == "Large" ? WeaponKind.Club : WeaponKind.Sword);
            poser.WeaponLength = weapon.length * size;
            var hits = new StrikeHits(loco, poser, anim) { Weapon = weapon, Scale = size, Mass = 70f * size * size * size };
            float reach = 1.6f;
            if (StrikeReach.TryGet(body, first.Name, out var grip, out var way))
            {
                Vector3 sweet = grip + way * (poser.WeaponLength * poser.SweetSpot);
                reach = new Vector2(sweet.x, sweet.z).magnitude;
            }

            var dummyRig = RigFile.Parse(File.ReadAllText(Path.Combine(Application.dataPath, "_Battler/Data/Rigs/medium_rig.json")));
            var dprofile = UnitProfile.CreateDefault();
            dprofile.body = "Medium";
            var droot = UnitBuilder.Build(dummyRig, dprofile, "Dummy", _ => null, 1);
            var dummy = new LabDummy(droot, droot.GetComponent<UnitRig>(), dprofile, new Vector3(0f, 0f, reach), 180f, 0f, "sns_", physical: false) { Stance = LabDummy.Stances.Open };
            const float dt = 1f / 60f;
            for (int i = 0; i < 80; i++)
            {
                loco.Step(dt, Vector3.zero, 0f);
                poser.Pose(dt);
                dummy.Step(dt, loco.Position + Vector3.up * 1.3f * size, false);
            }

            var player = new ComboPlayer(loco, poser)
            {
                Strikes = Environment.GetEnvironmentVariable("COMBO_FIXED") != null ? null : strikes,
                TargetVelocity = () => dummy.Loco.Velocity,
                // (Each strike at the dummy's part nearest its own reach's height.)
                TargetFor = clip =>
                {
                    float h = 1.3f * size;
                    if (StrikeReach.TryGet(body, clip.Name, out var g, out var w))
                    {
                        h = (g + w * (poser.WeaponLength * poser.SweetSpot)).y;
                    }

                    int best = 0;
                    for (int k = 1; k < 3; k++)
                    {
                        if (Mathf.Abs(dummy.TargetAt(k).y - h) < Mathf.Abs(dummy.TargetAt(best).y - h))
                        {
                            best = k;
                        }
                    }

                    return dummy.TargetAt(best);
                },
            };
            var results = new List<string>();
            float clock = 0f;
            var hitTimes = new List<float>();
            hits.Landed += text => { hitTimes.Add(clock); results.Add(text.Substring(0, text.IndexOf(" (", StringComparison.Ordinal) > 0 ? text.IndexOf(" (", StringComparison.Ordinal) : text.Length)); };
            var linkTimes = new List<float>();
            var playedNames = new List<string>();
            bool wasPressing = false;
            player.Started += (clip, link) =>
            {
                playedNames.Add((wasPressing ? "(pressed in) " : "") + clip.Name.Replace(prefix, "") + string.Format(ci, " [turn {0:+0;-0}]", poser.AimTurn));
                if (link != null) { linkTimes.Add(loco.ShotTime); }
                if (link != null && Environment.GetEnvironmentVariable("COMBO_WHY") != null) { Console.WriteLine("      chose " + clip.Name + " of: " + player.LastChoice); }
            };
            var film = new LocoFilm(dt, 2) { BladeLength = poser.WeaponLength };
            var dummyFilm = new LocoFilm(dt, 2) { ShieldRadius = 0.3f };
            player.Start(first, chain);
            var parts = new[] { PartId.HandR, PartId.HandL, PartId.ForearmR, PartId.ForearmL, PartId.FootL, PartId.FootR };
            var last = parts.Select(p => anim.Part(p).position).ToArray();
            int switches = loco.Switches;
            float sinceSwitch = 99f;
            for (int i = 0; i < 900; i++)
            {
                clock = i * dt;
                bool frozen = hits.HitStop > 0f;
                if (!frozen)
                {
                    // (Pressing after the dummy, the combo walks the walker in.)
                    loco.Step(dt, player.Pressing ? player.Want : Vector3.zero, player.Pressing ? player.WantFacing : loco.Facing);
                    player.Step(dt);
                    poser.Pose(dt);
                }

                if (player.Pressing && !wasPressing && Environment.GetEnvironmentVariable("COMBO_WHY") != null) { Console.WriteLine("      pressing: " + player.Ended); }
                if (Environment.GetEnvironmentVariable("COMBO_FEET") != null && (player.Pressing || loco.Current == Locomotor.Mode.Idle))
                {
                    Console.WriteLine(string.Format(ci, "      {0:0.00} {1} off {2:0.00} settled {3} held {4}{5} facing {6:0}", i * dt, loco.Current, loco.StanceOff, loco.StanceSettled, loco.FootHeld(0) ? "L" : "-", loco.FootHeld(1) ? "R" : "-", loco.Facing));
                }

                wasPressing = player.Pressing;
                hits.Step(dt, dummy);
                dummy.Step(dt, loco.Position + Vector3.up * 1.3f * size, frozen);
                film.Add(i * dt, string.Join(">", new[] { first.Name }.Concat(chain.Select(l => l.To.Name))), loco, poser, anim);
                dummyFilm.Add(i * dt, "dummy", dummy.Loco, dummy.Poser, dummy.Anim);
                if (loco.Switches != switches)
                {
                    switches = loco.Switches;
                    sinceSwitch = 0f;
                }

                sinceSwitch += dt;
                for (int k = 0; k < parts.Length; k++)
                {
                    Vector3 now = anim.Part(parts[k]).position;
                    float v = (now - last[k]).magnitude / dt;
                    // (About each switch between strikes: the strikes' own swings are fast anyway.)
                    if (sinceSwitch < 0.3f && v > fastest)
                    {
                        fastest = v;
                        fastestWhere = $"{parts[k]} {sinceSwitch:0.00} s after a switch in the combo from {first.Name} ({loco.Shot?.Name})";
                    }

                    last[k] = now;
                }

                if (!player.Playing && loco.Current != Locomotor.Mode.OneShot)
                {
                    break;
                }
            }

            combos++;
            strikesPlayed += playedNames.Count;
            foreach (var r in results)
            {
                if (r.Contains("blocked")) blocked++;
                else landed++;
            }

            missed += playedNames.Count - results.Count;
            Console.WriteLine("    played: " + string.Join(" > ", playedNames) + (player.Ended.Length > 0 ? $" (ended: {player.Ended})" : ""));
            Console.WriteLine("    " + (results.Count == 0 ? "nothing landed" : string.Join("; ", results)));
            if (hitTimes.Count > 1)
            {
                Console.WriteLine("    between blows: " + string.Join(", ", hitTimes.Zip(hitTimes.Skip(1), (a, b) => (b - a).ToString("0.00", ci) + " s")));
            }
            File.WriteAllText(Path.Combine(dir, first.Name + ".json"), film.ToString());
            File.WriteAllText(Path.Combine(dir, first.Name + ".dummy"), dummyFilm.ToString());
            UnityEngine.Object.DestroyImmediate(root);
            UnityEngine.Object.DestroyImmediate(droot);
        }

        Console.WriteLine(string.Format(ci, "All: {0} combos, {1} strikes: {2} landed, {3} blocked, {4} missed; about the switches the hands and feet moved at most {5:0.0} m/s ({6})",
            combos, strikesPlayed, landed, blocked, missed, fastest, fastestWhere));
        return 0;
    }

    private static (AnimRig, Locomotor, LocoBody, GameObject, float) DummyHitReportWalker(string body)
    {
        var rig = RigFile.Parse(File.ReadAllText(Path.Combine(Application.dataPath, $"_Battler/Data/Rigs/{body.ToLowerInvariant()}_rig.json")));
        var profile = UnitProfile.CreateDefault();
        profile.body = body;
        var root = UnitBuilder.Build(rig, profile, "Striker " + body, _ => null, 0);
        var unitRig = root.GetComponent<UnitRig>();
        var anim = new AnimRig(root.transform, unitRig);
        float leg = anim.RestLocalPosition(PartId.ShinL).magnitude + anim.RestLocalPosition(PartId.FootL).magnitude;
        var loco = new Locomotor(leg);
        loco.SetGuard(LocoLibrary.CombatPrefix(body));
        loco.GuardWanted = true;
        var poser = new LocoBody(anim, loco) { BodyKey = body, TwoHanded = LocoLibrary.TwoHanded(body) };
        poser.SoleFrom(unitRig);
        anim.SlotWeaponR.localRotation = UnitController.MainGrip;
        poser.BladeR = UnitController.MainGrip * Vector3.forward;
        poser.CarryR = LocoBody.Carry.Weapon;
        loco.Place(Vector3.zero, 0f);
        return (anim, loco, poser, root, leg / 0.845f);
    }
}
