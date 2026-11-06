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
    private static readonly List<float> predOff = new List<float>();
    private static float feetWorst;
    private static string feetWorstWhat = "";
    private static int retiresAll, groundAll;
    private static readonly List<float> readWeights = new List<float>();

    public static int Run(string dir, string body)
    {
        Directory.CreateDirectory(dir);
        body ??= "Medium";
        LocoLibrary.Load();
        var ci = CultureInfo.InvariantCulture;
        var allShieldMeets = new List<StrikeHits.ShieldMeet>();
        if (Environment.GetEnvironmentVariable("GROUND_TRACE") != null) ClipFighter.GroundTrace = line => Console.WriteLine("      " + line);
        if (Environment.GetEnvironmentVariable("SHIELD_PLAN") != null) ClipFighter.ShieldTrace = line => Console.WriteLine("      the shield: " + line);
        var steadies = new List<ShieldSteadiness>();
        int allInside = 0, allDeep = 0, longestDeep = 0, armThrough = 0; float armThroughMost = 0f;
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
        bool cores = Environment.GetEnvironmentVariable("COMBO_OLD") == null;
        var beats = new List<float>();
        var spans = new List<float>();
        // (Cores: each picked strike's combos as good as its best, up to COMBO_VARIANTS of them (3), as the lab goes round them.)
        int variants = int.TryParse(Environment.GetEnvironmentVariable("COMBO_VARIANTS"), out int nv) ? nv : 3;
        var jobs = new List<(LocoClip picked, List<StrikeLink> chain)>();
        foreach (var picked in strikes)
        {
            if (!cores)
            {
                jobs.Add((picked, StrikeCombos.Chain(picked, strikes, 3)));
                continue;
            }

            foreach (var c in StrikeCombos.CoreChains(picked, strikes).Take(variants))
            {
                jobs.Add((picked, c));
            }
        }

        var follows = new List<float>();
        foreach (var (picked, chain) in jobs)
        {
            var first = chain.Count > 0 ? chain[0].From : picked;
            if (cores && first != picked)
            {
                Console.WriteLine($"{picked.Name} (picked) ends its combo:");
            }

            Console.WriteLine($"{first.Name}: " + (chain.Count == 0 ? "no follow-on" : string.Join("; ", chain.Select(l => l.ToString()))));
            // Played at the dummy (a strike with no quick follow-on too: the combo goes on from the guard).
            var (anim, loco, poser, root, walker) = DummyHitReport.Walker(body);
            float size = walker.Size;
            var weapon = WeaponDefinition.CreateDefault(body == "Large" ? WeaponKind.Club : WeaponKind.Sword);
            poser.WeaponLength = weapon.length * size;
            var hits = walker.EnableStrikes(weapon);
            var steady = new ShieldSteadiness();
            steadies.Add(steady);
            hits.Mass = 70f * size * size * size;
            ComboPlayer playerRef = null;
            hits.Finishing = () => playerRef != null && playerRef.LastBlow;
            hits.Comboing = () => playerRef != null && playerRef.Playing && playerRef.Cores;
            float reach = 1.6f;
            if (StrikeReach.TryGet(body, first.Name, out var grip, out var way))
            {
                Vector3 sweet = grip + way * (poser.WeaponLength * poser.SweetSpot);
                reach = new Vector2(sweet.x, sweet.z).magnitude;
            }

            // (From the standing distance, as the lab sets them up.)
            reach = Mathf.Max(reach, ClipFighter.StandingDistance(size, 1f));
            var dummyRig = RigFile.Parse(File.ReadAllText(Path.Combine(Application.dataPath, "_Battler/Data/Rigs/medium_rig.json")));
            var dprofile = UnitProfile.CreateDefault();
            dprofile.body = "Medium";
            var droot = UnitBuilder.Build(dummyRig, dprofile, "Dummy", _ => null, 1);
            bool swordDummy = Environment.GetEnvironmentVariable("COMBO_STANCE") == "sword";
            ClipFighter.GivesGround = Environment.GetEnvironmentVariable("NO_RETIRE") == null;
            if (Environment.GetEnvironmentVariable("NO_GROUND_STEP") != null)
            {
                // (Only the late arm's step back, as before the weight plan's step 5.)
                CombatFeel.GiveGroundFrom = 1e6f;
                CombatFeel.GiveGroundDeep = 0f;
            }
            if (Environment.GetEnvironmentVariable("PARRY_TRACE") != null)
            {
                float traceFrom = 0f;
                SwordParry.Trace = line => Console.WriteLine("        parry: " + line);
                _ = traceFrom;
            }
            // (COMBO_PLACE="x,z": the dummy that far to the side and further off (metres), facing the walker; the lab's
            // combos start from wherever the walker was driven to.)
            Vector3 dummyAt = new Vector3(0f, 0f, reach);
            var placeArg = Environment.GetEnvironmentVariable("COMBO_PLACE");
            if (placeArg != null)
            {
                var xz = placeArg.Split(',');
                dummyAt += new Vector3(float.Parse(xz[0], System.Globalization.CultureInfo.InvariantCulture), 0f, float.Parse(xz[1], System.Globalization.CultureInfo.InvariantCulture));
            }

            float dummyFacing = Mathf.Atan2(-dummyAt.x, -dummyAt.z) * Mathf.Rad2Deg;
            var dummy = new ClipFighter(droot, droot.GetComponent<UnitRig>(), dprofile, dummyAt, dummyFacing, 0f, "sns_", physical: false);
            if (swordDummy)
            {
                // (Its sword, as the lab holds it, parrying.)
                dummy.Rig.SlotWeaponR.localRotation = UnitController.MainGrip;
                dummy.Anim.SlotWeaponR.localRotation = UnitController.MainGrip;
                dummy.Poser.CarryR = LocoBody.Carry.Weapon;
                dummy.Poser.BladeR = UnitController.MainGrip * Vector3.forward;
                var dummySword = WeaponDefinition.CreateDefault(WeaponKind.Sword);
                dummy.Poser.WeaponLength = dummySword.length * dummy.Size;
                dummy.Parry = new SwordParry(dummy.Anim, dummy.Loco, dummy.Rig, dummy.Size, dummySword.length * dummy.Size)
                {
                    Opponent = new OpponentShape(anim, size, poser.CarryL == LocoBody.Carry.Shield ? anim.SlotWeaponL : null, 0.27f) { WeaponSlot = anim.SlotWeaponR, WeaponLength = poser.WeaponLength },
                };
                dummy.Stance = ClipFighter.Stances.Sword;
                if (float.TryParse(Environment.GetEnvironmentVariable("PARRY_SKILL"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float parrySkill))
                {
                    dummy.ParrySkill = parrySkill;
                }

                if (float.TryParse(Environment.GetEnvironmentVariable("SHIELD_SKILL"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float shieldSkill))
                {
                    dummy.ShieldSkill = shieldSkill;
                }
            }
            else if (Environment.GetEnvironmentVariable("COMBO_STANCE") == "shield")
            {
                // (Its shield, as dummyhits gives it: a board on its left hand's slot.)
                var board = new GameObject("Dummy shield");
                dummy.Rig.SlotWeaponL.localRotation = UnitController.ShieldGrip;
                dummy.Anim.SlotWeaponL.localRotation = UnitController.ShieldGrip;
                board.transform.SetParent(dummy.Rig.SlotWeaponL, false);
                var box = board.AddComponent<BoxCollider>();
                box.size = new Vector3(0.522f, 0.522f, 0.05f);
                box.center = new Vector3(0f, 0f, 0.075f);
                box.isTrigger = true;
                dummy.ShieldColliders.Add(box);
                dummy.Stance = ClipFighter.Stances.Shield;
                if (float.TryParse(Environment.GetEnvironmentVariable("SHIELD_SKILL"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float shieldSkill))
                {
                    dummy.ShieldSkill = shieldSkill;
                }
            }
            else
            {
                dummy.Stance = ClipFighter.Stances.Open;
            }
            const float dt = 1f / 60f;
            for (int i = 0; i < 80; i++)
            {
                walker.Move(dt, Vector3.zero, 0f, dummy.Loco.Position, false);
                dummy.Step(dt, loco.Position + Vector3.up * 1.3f * size, false);
            }

            var player = new ComboPlayer(loco, poser)
            {
                Cores = cores,
                Keep = picked,
                Strikes = Environment.GetEnvironmentVariable("COMBO_FIXED") != null ? null : strikes,
                TargetVelocity = () => dummy.Loco.Velocity,
                // (Each strike at the dummy's part nearest its own reach's height.)
                TargetFor = clip =>
                {
                    // (COMBO_TARGET=legs: every blow at the knees, the low blows the parry must meet too.)
                    if (Environment.GetEnvironmentVariable("COMBO_TARGET") == "legs")
                    {
                        return dummy.TargetAt(-1);
                    }

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
            playerRef = player;
            walker.Combo = player;
            var results = new List<string>();
            float clock = 0f;
            var hitTimes = new List<float>();
            var linkTimes = new List<float>();
            var playedNames = new List<string>();
            // (One result a blow: a blade that cuts through goes on to strike more parts, which is still the one blow.)
            int lastLandedBlow = -1;
            hits.Landed += text =>
            {
                if (Environment.GetEnvironmentVariable("SHIELD_PLAN") != null) Console.WriteLine(string.Format(ci, "      {0:0.00} s: {1}", clock, text));
                if (playedNames.Count == lastLandedBlow)
                {
                    return;
                }

                lastLandedBlow = playedNames.Count;
                hitTimes.Add(clock);
                results.Add(text.Substring(0, text.IndexOf(" (", StringComparison.Ordinal) > 0 ? text.IndexOf(" (", StringComparison.Ordinal) : text.Length));
            };
            bool wasPressing = false;
            Vector3 aimAt = Vector3.zero;
            float aimNearest = float.MaxValue;
            var passed = new List<string>();
            player.Started += (clip, link) =>
            {
                if (playedNames.Count > 0)
                {
                    passed.Add((aimNearest * 100f).ToString("0", ci) + " cm");
                }

                aimAt = poser.AimTarget;
                aimNearest = float.MaxValue;
                if (Environment.GetEnvironmentVariable("COMBO_AIM") != null)
                {
                    Console.WriteLine(string.Format(ci, "      {0} from {1:0.00} s: planned to miss by {2:0.00} m before the pull, turn {3:+0;-0}, lunge x{4:0.00}, predicted at {5}, the target {6}, the dummy {7:0.00} m away",
                        clip.Name, loco.ShotTime, poser.AimMissBeforePull, poser.AimTurn, poser.AimLunge, poser.AimPredicted, poser.AimTarget,
                        new Vector2(dummy.Loco.Position.x - loco.Position.x, dummy.Loco.Position.z - loco.Position.z).magnitude));
                }
                playedNames.Add((wasPressing ? "(pressed in) " : "") + clip.Name.Replace(prefix, "") + string.Format(ci, " [turn {0:+0;-0}{1}]", poser.AimTurn,
                    player.LastStepIn > 0.01f ? string.Format(ci, ", stepped in {0:0.00} m", player.LastStepIn) : ""));
                if (link != null) { linkTimes.Add(loco.ShotTime); follows.Add(Mathf.Abs(poser.AimTurn)); }
                if (link != null && Environment.GetEnvironmentVariable("COMBO_WHY") != null) { Console.WriteLine("      chose " + clip.Name + " of: " + player.LastChoice); }
            };
            var film = new LocoFilm(dt, 2) { BladeLength = poser.WeaponLength };
            var dummyFilm = new LocoFilm(dt, 2) { ShieldRadius = 0.3f };
            var dummyFeet = new LocoMeasures(dummy.Anim, dummy.Loco, dummy.Poser);
            player.Start(first, chain);
            var parts = new[] { PartId.HandR, PartId.HandL, PartId.ForearmR, PartId.ForearmL, PartId.FootL, PartId.FootR };
            var last = parts.Select(p => anim.Part(p).position).ToArray();
            // (The attacker's root and chest, step to step: a physical body can't follow a pose that jumps.)
            Vector3 lastRoot = loco.Position, lastChest = anim.Part(PartId.Chest).position;
            float fastestRoot = 0f, fastestChest = 0f;
            string fastestRootAt = "", fastestChestAt = "";
            int switches = loco.Switches;
            float sinceSwitch = 99f;
            string lastState = "";
            bool predTrace = Environment.GetEnvironmentVariable("PRED_TRACE") != null;
            var firstPred = new Dictionary<int, (Vector3, Vector3, float, Vector3, float)>();
            var lastPred = new Dictionary<int, (Vector3, Vector3, float, Vector3, float)>();
            int nowSerial = -1, weighedSerial = -1;
            var predClip = new Dictionary<int, LocoClip>();
            for (int i = 0; i < 900; i++)
            {
                clock = i * dt;
                float keepAt = ClipFighter.FightingDistance(size, dummy.Size);
                poser.KeepFrom = dummy.Loco.Position;
                poser.KeepDistance = keepAt;
                dummy.KeepDistance = keepAt;
                dummy.StandDistance = ClipFighter.StandingDistance(size, dummy.Size);
                bool frozen = hits.HitStop > 0f;
                if (Environment.GetEnvironmentVariable("COMBO_TIMELINE") != null)
                {
                    string state = $"{loco.Current} {(loco.Shot != null ? loco.Shot.Name.Replace(prefix, "") : "-")}{(frozen ? " hit stop" : "")}{(loco.ShotSpeed < 0f ? " backing out" : "")}{(player.Pressing ? " pressing" : "")}";
                    if (state != lastState)
                    {
                        Console.WriteLine(string.Format(ci, "      {0:0.00} s: {1} (at {2:0.00} s of it)", i * dt, state, loco.ShotTime));
                        lastState = state;
                    }
                }
                // (Pressing after the dummy, the combo walks the walker in.)
                walker.Move(dt, player.Pressing ? player.Want : Vector3.zero, player.Pressing ? player.WantFacing : loco.Facing, dummy.Loco.Position, frozen);

                if (player.Pressing && !wasPressing && Environment.GetEnvironmentVariable("COMBO_WHY") != null) { Console.WriteLine("      pressing: " + player.Ended); }
                if (Environment.GetEnvironmentVariable("COMBO_FEET") != null && (player.Pressing || loco.Current == Locomotor.Mode.Idle))
                {
                    Console.WriteLine(string.Format(ci, "      {0:0.00} {1} off {2:0.00} settled {3} held {4}{5} facing {6:0}", i * dt, loco.Current, loco.StanceOff, loco.StanceSettled, loco.FootHeld(0) ? "L" : "-", loco.FootHeld(1) ? "R" : "-", loco.Facing));
                }

                if (wasPressing && !player.Pressing && !player.Playing && Environment.GetEnvironmentVariable("COMBO_WHY") != null)
                {
                    var d = dummy.Loco.Position - loco.Position;
                    Console.WriteLine(string.Format(ci, "      ended pressing {0:0.00} m from the dummy (root to root), facing off {1:0}; each strike would miss by: {2}", new Vector2(d.x, d.z).magnitude,
                        Mathf.DeltaAngle(loco.Facing, Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg),
                        string.Join(", ", strikes.Select(c => c.Name.Replace(prefix, "") + " " + poser.MissAfterPull(c, player.TargetFor(c)).ToString("0.00", ci)))));
                }

                wasPressing = player.Pressing;
                if (loco.Shot != null && hits.Blade(loco.Shot, out var bg, out var btip, out _))
                {
                    StrikeHits.SegmentsNearest(bg, btip, aimAt, aimAt + Vector3.up * 1e-3f, out _, out _, out _, out _);
                    Vector3 ab = btip - bg;
                    float u = Mathf.Clamp01(Vector3.Dot(aimAt - bg, ab) / Mathf.Max(1e-6f, ab.sqrMagnitude));
                    aimNearest = Mathf.Min(aimNearest, (bg + ab * u - aimAt).magnitude);
                }

                if (Environment.GetEnvironmentVariable("COMBO_HAND") is string hw && first.Name.Contains(hw.Split(':')[0]))
                {
                    var range = hw.Split(':')[1].Split('-');
                    if (i * dt >= float.Parse(range[0], ci) && i * dt <= float.Parse(range[1], ci))
                    {
                        Vector3 clipHand = loco.Position + Quaternion.Euler(0f, loco.Facing, 0f) * loco.Points[(int)LocoBone.RightHand];
                        Console.WriteLine(string.Format(ci, "      {0:0.000} {1} {2:0.00} {3} hand {4} clip's hand {5} pull weight {6:0.00} aim lands {7}", i * dt, loco.Shot?.Name, loco.ShotTime, frozen ? "frozen" : "", anim.Part(PartId.HandR).position, clipHand, poser.AimWeightNow, poser.AimLands));
                    }
                }

                if (Environment.GetEnvironmentVariable("COMBO_AIM") != null && loco.Shot != null && loco.Shot.ContactFrame >= 0)
                {
                    float c = loco.Shot.ContactFrame / loco.Shot.Rate;
                    if (loco.ShotTime >= c && loco.ShotTime - loco.ShotRate * dt < c)
                    {
                        Vector3 nowAt = player.TargetFor(loco.Shot);
                        hits.Blade(loco.Shot, out var cg, out var ct, out _);
                        Console.WriteLine(string.Format(ci, "      {0} at contact: aimed at {1}, the target now {2} ({3:0.00} m off), the tip at {4}, the dummy {5:0.00} m away (root to root), live {6}",
                            loco.Shot.Name, aimAt, nowAt, (nowAt - aimAt).magnitude, ct,
                            new Vector2(dummy.Loco.Position.x - loco.Position.x, dummy.Loco.Position.z - loco.Position.z).magnitude, hits.LiveBefore));
                    }
                }

                if (hits.Read(out var wrd) && wrd.ToContact <= 0.25f && wrd.Serial != weighedSerial)
                {
                    weighedSerial = wrd.Serial;
                    readWeights.Add(wrd.Momentum / Mathf.Max(1f, dummy.Mass));
                }

                if (predTrace && hits.Read(out var prd))
                {
                    if (prd.Predicted)
                    {
                        var p = (prd.Path[StrikeHits.ContactPoint], hits.PredictedRoot, hits.PredictedFacing, hits.PredictedPull, prd.ToContact);
                        predClip[prd.Serial] = hits.PredictedClip;
                        if (!firstPred.ContainsKey(prd.Serial)) firstPred[prd.Serial] = p;
                        lastPred[prd.Serial] = p;
                    }
                    else if (prd.ToContact > 0f)
                    {
                        nowSerial = prd.Serial;
                    }
                }

                if (predTrace && loco.Shot != null && loco.Shot.ContactFrame >= 0)
                {
                    float c = loco.Shot.ContactFrame / loco.Shot.Rate;
                    if (loco.ShotTime >= c && loco.ShotTime - loco.ShotRate * dt < c && firstPred.TryGetValue(nowSerial, out var fp) && hits.Blade(loco.Shot, out _, out _, out var strikeNow))
                    {
                        var lp = lastPred[nowSerial];
                        string Err((Vector3 strike, Vector3 root, float facing, Vector3 pull, float ahead) q)
                        {
                            Vector3 armP = Quaternion.Inverse(Quaternion.Euler(0f, q.facing, 0f)) * (q.strike - q.root);
                            Vector3 armA = Quaternion.Inverse(Quaternion.Euler(0f, loco.Facing, 0f)) * (strikeNow - loco.Position);
                            Vector3 rootOff = loco.Position - q.root; rootOff.y = 0f;
                            return string.Format(ci, "{0:0.00} s ahead: off {1:0.00} m; the root {2:0.00} m off, facing {3:+0;-0} deg off, the arm (in the body's frame) {4:0.00} m off (pull {5:0.00} m planned)",
                                q.ahead, (strikeNow - q.strike).magnitude, rootOff.magnitude, Mathf.DeltaAngle(q.facing, loco.Facing), (armA - armP).magnitude, q.pull.magnitude);
                        }

                        Console.WriteLine("      predicted " + (predClip.TryGetValue(nowSerial, out var pcn) && pcn != null ? pcn.Name.Replace(prefix, "") : "?") + ", played " + loco.Shot.Name.Replace(prefix, "") + string.Format(ci, " (strike {0} vs read {1}, root {2} vs {3})", strikeNow, fp.Item1, loco.Position, fp.Item2) + ": first read " + Err(fp) + " | last read " + Err(lp));
                        predOff.Add((strikeNow - fp.Item1).magnitude);
                    }
                }

                hits.Step(dt, dummy);
                dummy.Watch(hits);
                dummy.Step(dt, loco.Position + Vector3.up * 1.3f * size, frozen);
                dummyFeet.Sample(dt, clock, "combo");
                if (dummyFeet.PlantDriftWorst > feetWorst)
                {
                    feetWorst = dummyFeet.PlantDriftWorst;
                    feetWorstWhat = string.Format(ci, "{0:0.00} s of the combo from {1}{2}", clock, first.Name.Replace(prefix, ""), dummy.Retires > 0 ? $", after giving ground {dummy.Retires} times" : "");
                }
                steady.Step(dummy);
                film.Add(i * dt, string.Join(">", new[] { first.Name }.Concat(chain.Select(l => l.To.Name))), loco, poser, anim);
                dummyFilm.Add(i * dt, "dummy", dummy.Loco, dummy.Poser, dummy.Anim);
                if (loco.Switches != switches)
                {
                    switches = loco.Switches;
                    sinceSwitch = 0f;
                }

                sinceSwitch += dt;
                {
                    float vr = (loco.Position - lastRoot).magnitude / dt, vc = (anim.Part(PartId.Chest).position - lastChest).magnitude / dt;
                    string at = string.Format(ci, "{0:0.00} s, {1}", i * dt, loco.Shot != null ? loco.Shot.Name.Replace(prefix, "") : loco.Current.ToString());
                    if (vr > fastestRoot) { fastestRoot = vr; fastestRootAt = at; }
                    if (vc > fastestChest) { fastestChest = vc; fastestChestAt = at; }
                    lastRoot = loco.Position;
                    lastChest = anim.Part(PartId.Chest).position;
                }

                for (int k = 0; k < parts.Length; k++)
                {
                    Vector3 now = anim.Part(parts[k]).position;
                    float v = (now - last[k]).magnitude / dt;
                    // (About each switch between strikes: the strikes' own swings are fast anyway.)
                    // (Not the blow itself: a follow-on taken up late in its wind-up swings into its live moments within that.)
                    bool inBlow = loco.Current == Locomotor.Mode.OneShot && loco.Shot != null && loco.Shot.ContactFrame >= 0 && loco.ShotTime > loco.Shot.ContactFrame / loco.Shot.Rate - hits.LiveBefore;
                    if (v > 15f && Environment.GetEnvironmentVariable("COMBO_SPEED") != null)
                    {
                        Console.WriteLine(string.Format(ci, "      {0} at {1:0.0} m/s at {2:0.00} s ({3} at {4:0.00} s, {5:0.00} s after a switch; facing {6:0})", parts[k], v, i * dt, loco.Shot?.Name ?? loco.Current.ToString(), loco.ShotTime, sinceSwitch, loco.Facing));
                    }

                    if (sinceSwitch < 0.3f && !inBlow && v > fastest)
                    {
                        fastest = v;
                        fastestWhere = $"{parts[k]} {sinceSwitch:0.00} s after a switch in the combo from {first.Name} ({loco.Shot?.Name} at {loco.ShotTime:0.00} s, the fade's half-life {loco.SwitchHalfLifeNow:0.00} s)";
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
            passed.Add((aimNearest * 100f).ToString("0", ci) + " cm");
            retiresAll += dummy.Retires;
            groundAll += dummy.GroundSteps;
            Console.WriteLine("    " + (results.Count == 0 ? "nothing landed" : string.Join("; ", results)) + "; the blades passed their aim points by " + string.Join(", ", passed) + (dummy.Retires > 0 ? $"; the dummy gave ground {dummy.Retires} times" : ""));
            Console.WriteLine(string.Format(ci, "    through the dummy's sword without meeting it: {0} steps in the blows, {1} outside them", hits.PassedThroughLive, hits.PassedThroughOutside));
            Console.WriteLine(string.Format(ci, "    the attacker's root at most {0:0.0} m/s ({1}), its chest {2:0.0} m/s ({3})", fastestRoot, fastestRootAt, fastestChest, fastestChestAt));
            if (hitTimes.Count > 1)
            {
                Console.WriteLine("    between blows: " + string.Join(", ", hitTimes.Zip(hitTimes.Skip(1), (a, b) => (b - a).ToString("0.00", ci) + " s"))
                    + string.Format(ci, "; first contact to last {0:0.00} s", hitTimes[hitTimes.Count - 1] - hitTimes[0]));
                beats.AddRange(hitTimes.Zip(hitTimes.Skip(1), (a, b) => b - a));
                if (hitTimes.Count >= 3)
                {
                    spans.Add(hitTimes[hitTimes.Count - 1] - hitTimes[0]);
                }
            }
            File.WriteAllText(Path.Combine(dir, first.Name + (chain.Count > 0 ? "_" + chain[chain.Count - 1].To.Name.Replace(prefix, "") + "_" + chain[0].To.Name.Replace(prefix, "") : "") + ".json"), film.ToString());
            File.WriteAllText(Path.Combine(dir, first.Name + ".dummy"), dummyFilm.ToString());
            allShieldMeets.AddRange(hits.ShieldMeets); armThrough += dummy.ArmThroughBoard; armThroughMost = Mathf.Max(armThroughMost, dummy.ArmThroughBoardMost);
            allInside += hits.InsideOutside; allDeep += hits.InsideDeep; longestDeep = Math.Max(longestDeep, hits.LongestDeep);
            UnityEngine.Object.DestroyImmediate(root);
            UnityEngine.Object.DestroyImmediate(droot);
        }

        beats.Sort();
        spans.Sort();
        follows.Sort();
        if (follows.Count > 0)
        {
            Console.WriteLine(string.Format(ci, "Follow-ons' turns onto the target: median {0:0} degrees, most {1:0}; {2} of {3} within 45", follows[follows.Count / 2], follows[follows.Count - 1], follows.Count(f => f <= 45f), follows.Count));
        }

        if (beats.Count > 0)
        {
            Console.WriteLine(string.Format(ci, "Hit to hit: median {0:0.00} s, most {1:0.00} s; first contact to the third: median {2:0.00} s, most {3:0.00} s ({4} of {5} combos landed three)",
                beats[beats.Count / 2], beats[beats.Count - 1], spans.Count > 0 ? spans[spans.Count / 2] : 0f, spans.Count > 0 ? spans[spans.Count - 1] : 0f, spans.Count, combos));
        }

        ShieldMeetSummary.Print(allShieldMeets);
        foreach (var st in steadies) st.Close();
        ShieldSteadiness.Print(steadies);
        Console.WriteLine($"Outside its blows the blade touched a body or a shield {allInside} steps, was over 2 cm into one {allDeep} steps, the longest {longestDeep} steps running");
        Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "The shield arm through its own board: {0} steps, at most {1:0.00} m", armThrough, armThroughMost));
        Console.WriteLine(string.Format(ci, "The dummy's planted feet moved at most {0:0.0} cm ({1}); it gave ground {2} times, {3} of them to a heavy or deep blow", feetWorst * 100f, feetWorstWhat, retiresAll, groundAll));
        if (readWeights.Count > 0)
        {
            var rw = readWeights.OrderBy(v => v).ToList();
            Console.WriteLine(string.Format(ci, "The weights the dummy read coming (m/s, a block's): least {0:0.00}, median {1:0.00}, most {2:0.00} ({3} blows)", rw[0], rw[rw.Count / 2], rw[rw.Count - 1], rw.Count));
        }
        if (predOff.Count > 0)
        {
            var po = predOff.OrderBy(v => v).ToList();
            Console.WriteLine(string.Format(ci, "The combos' next blows read ahead: at contact the blade came from the first read median {0:0.00} m, 90% {1:0.00} m, at most {2:0.00} m ({3} blows)", po[po.Count / 2], po[(int)(po.Count * 0.9f)], po[po.Count - 1], po.Count));
        }

        Console.WriteLine(string.Format(ci, "All: {0} combos, {1} strikes: {2} landed, {3} blocked, {4} missed; about the switches the hands and feet moved at most {5:0.0} m/s ({6})",
            combos, strikesPlayed, landed, blocked, missed, fastest, fastestWhere));
        return 0;
    }
}
