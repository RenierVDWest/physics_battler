// `spar <dir> [seconds]`: two clip fighters trade blows (the plan "Bringing the lab into the fight", step 2): A walks in
// until a strike of its own lands, then the two take turns, each striking at the other with a strike that lands from
// where it stands (every third turn a combo), each meeting the other's blows with its shield or sword. Not physical
// (the bodies carried to their poses), as the other lab runners are.
// SPAR_A / SPAR_B: open | shield | sword | club (each one's defence, default shield; club: a Large with its club alone); SPAR_SKILL: both one's skill (default 0.5).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using PhysicsBattler.Combat;
using PhysicsBattler.Locomotion;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

internal static class SparReport
{
    public static int Run(string dir, float seconds)
    {
        Directory.CreateDirectory(dir);
        LocoLibrary.Load();
        var ci = CultureInfo.InvariantCulture;
        float skill = float.TryParse(Environment.GetEnvironmentVariable("SPAR_SKILL"), NumberStyles.Float, ci, out float sk) ? sk : 0.5f;
        var a = Fighter("A", Environment.GetEnvironmentVariable("SPAR_A") ?? "shield", Vector3.zero, 0f, skill);
        // (B where A's strikes reach it from its guard, a little outside the fighting distance.)
        var b = Fighter("B", Environment.GetEnvironmentVariable("SPAR_B") ?? "shield", new Vector3(0f, 0f, ClipFighter.FightingDistance(a.Size, 1f) + 0.3f), 180f, skill);
        foreach (var (f, other) in new[] { (a, b), (b, a) })
        {
            if (f.Stance == ClipFighter.Stances.Sword)
            {
                f.Parry = new SwordParry(f.Anim, f.Loco, f.Rig, f.Size, f.Poser.WeaponLength)
                {
                    Opponent = new OpponentShape(other.Anim, other.Size, other.Stance == ClipFighter.Stances.Shield ? other.Rig.SlotWeaponL : null, 0.27f)
                    {
                        WeaponSlot = other.Anim.SlotWeaponR, WeaponLength = other.Poser.WeaponLength,
                    },
                };
                if (f.Strikes != null && f.Strikes.Weapon != null && f.Strikes.Weapon.kind == WeaponKind.Club)
                {
                    f.Parry.AsClub();
                }
            }
        }

        var log = new List<string>();
        float clock = 0f;
        // (SPAR_TRACE=from,to: the parries' reads between those seconds.)
        var traceArg = Environment.GetEnvironmentVariable("SPAR_TRACE");
        if (traceArg != null)
        {
            var ft = traceArg.Split(',');
            float tFrom = float.Parse(ft[0], ci), tTo = float.Parse(ft[1], ci);
            SwordParry.Trace = line =>
            {
                if (clock >= tFrom && clock <= tTo && !line.StartsWith("placed", StringComparison.Ordinal) && !line.Contains(" jump "))
                {
                    Console.WriteLine(string.Format(ci, "{0,6:0.00} s  parry: {1}", clock, line));
                }
            };
        }
        a.Strikes.Landed += text => log.Add(string.Format(ci, "{0,6:0.00} s  A > B: {1}; B: {2}", clock, text, b.LastReaction));
        b.Strikes.Landed += text => log.Add(string.Format(ci, "{0,6:0.00} s  B > A: {1}; A: {2}", clock, text, a.LastReaction));
        Vector3 bladeWas = Vector3.zero, lastTurn = Vector3.zero;
        bool bladeKnown = false;
        float lastTurnDeg = 0f;
        int flicks = 0;
        var onlyA = new List<ClipFighter> { a };
        var onlyB = new List<ClipFighter> { b };
        const float dt = 1f / 60f;
        int turn = 0, thrownA = 0, thrownB = 0, combosA = 0, combosB = 0, outOfReach = 0;
        float idleFor = 0f, nearest = 99f, nearestAt = 0f, worstPart = 0f;
        string nearestWhat = "", worstWhat = "";
        bool inRange = false;
        // (SPAR_CLASH=1: the one struck at strikes back 0.25 s into the other's blow: blows that cross, a strike struck.)
        bool clash = Environment.GetEnvironmentVariable("SPAR_CLASH") != null;
        float clashAt = -1f;
        ClipFighter clasher = null, clashTarget = null;
        var filmA = new LocoFilm(dt, 2) { BladeLength = a.Poser.WeaponLength, ShieldRadius = a.Stance == ClipFighter.Stances.Shield ? 0.3f : 0f };
        var filmB = new LocoFilm(dt, 2) { BladeLength = b.Poser.WeaponLength, ShieldRadius = b.Stance == ClipFighter.Stances.Shield ? 0.3f : 0f };
        var lastPos = new Dictionary<(ClipFighter, PartId), Vector3>();
        var measuresA = new LocoMeasures(a.Anim, a.Loco, a.Poser);
        var measuresB = new LocoMeasures(b.Anim, b.Loco, b.Poser);
        for (int i = 0; i < Mathf.RoundToInt(seconds / dt); i++)
        {
            clock = i * dt;
            float keep = ClipFighter.FightingDistance(a.Size, b.Size);
            a.KeepDistance = keep;
            b.KeepDistance = keep;
            b.StandDistance = ClipFighter.StandingDistance(a.Size, b.Size);
            a.KeepOff(b);
            b.KeepOff(a);
            bool frozen = a.Strikes.HitStop > 0f || b.Strikes.HitStop > 0f;

            // A: walks in until one of its strikes lands, then stands, facing B; the two take turns.
            Vector3 toB = b.Loco.Position - a.Loco.Position;
            toB.y = 0f;
            float yaw = Mathf.Atan2(toB.x, toB.z) * Mathf.Rad2Deg;
            Vector3 want = Vector3.zero;
            if (!inRange && !a.Striking)
            {
                // (Walked in slowly, and stopped as soon as a strike lands or it is near the fighting distance: a walk's
                // start clip carries it on a little past where it is told to stop.)
                inRange = a.StrikeThatLands(b) != null || toB.magnitude < keep + 0.35f * a.Size;
                if (!inRange)
                {
                    want = toB.normalized * 0.4f;
                }
            }

            if (inRange && !frozen && !a.Busy && !b.Busy)
            {
                idleFor += dt;
                if (idleFor > 0.6f)
                {
                    idleFor = 0f;
                    var (striker, target) = turn % 2 == 0 ? (a, b) : (b, a);
                    var clip = striker.StrikeThatLands(target);
                    if (clip == null)
                    {
                        outOfReach++;
                        log.Add(string.Format(ci, "{0,6:0.00} s  {1}: no strike reaches", clock, striker == a ? "A" : "B"));
                        if (striker == a)
                        {
                            inRange = false;
                        }
                    }
                    else if (turn % 3 == 2 && striker.ComboAt(target, clip, turn, out var opener, out var chain))
                    {
                        log.Add(string.Format(ci, "{0,6:0.00} s  {1} combo: {2}{3}", clock, striker == a ? "A" : "B", opener.Name, string.Concat(chain.ConvertAll(l => " > " + l.To.Name))));
                        if (striker == a) combosA++; else combosB++;
                    }
                    else
                    {
                        striker.StrikeAt(target, clip);
                        log.Add(string.Format(ci, "{0,6:0.00} s  {1} strikes: {2}", clock, striker == a ? "A" : "B", clip.Name));
                    }

                    if (clip != null)
                    {
                        if (striker == a) thrownA++; else thrownB++;
                        if (clash)
                        {
                            clashAt = clock + 0.25f;
                            clasher = target;
                            clashTarget = striker;
                        }
                    }

                    turn++;
                }
            }
            else
            {
                idleFor = 0f;
            }

            if (clashAt >= 0f && clock >= clashAt && !frozen)
            {
                clashAt = -1f;
                var back = clasher.Busy ? null : clasher.StrikeThatLands(clashTarget);
                if (back != null)
                {
                    clasher.StrikeAt(clashTarget, back);
                    log.Add(string.Format(ci, "{0,6:0.00} s  {1} strikes into it: {2}", clock, clasher == a ? "A" : "B", back.Name));
                    if (clasher == a) thrownA++; else thrownB++;
                }
            }

            a.Watch(b.Strikes);
            a.Move(dt, want, a.Striking || a.Loco.Current == Locomotor.Mode.OneShot ? a.Loco.Facing : yaw, b.Loco.Position, frozen);
            a.Strikes.Step(dt, onlyB);
            b.Watch(a.Strikes);
            b.Step(dt, a.Anim.Part(PartId.Chest).position, frozen);
            b.Strikes.Step(dt, onlyA);
            ClipFighter.KeepApart(a, b);
            a.Apply(dt);
            // (A's weapon, step to step, out of its strikes: how often it flicks one way and straight back, a jitter.)
            if (a.Anim.SlotWeaponR != null)
            {
                Vector3 wayNow = a.Anim.SlotWeaponR.rotation * Vector3.forward;
                bool inStrike = a.Loco.Current == Locomotor.Mode.OneShot && a.Loco.Shot != null && a.Loco.Shot.ContactFrame >= 0;
                if (bladeKnown && !inStrike)
                {
                    Vector3 turnNow = wayNow - bladeWas;
                    float deg = Vector3.Angle(bladeWas, wayNow);
                    if (deg > 1.5f && lastTurnDeg > 1.5f && Vector3.Dot(turnNow, lastTurn) < 0f)
                    {
                        flicks++;
                        if (Environment.GetEnvironmentVariable("SPAR_FLICKS") != null)
                        {
                            Console.WriteLine(string.Format(ci, "{0,6:0.00} s  A's weapon flicked {1:0.0} then {2:0.0} deg ({3} {4}; parry {5})", clock, lastTurnDeg, deg, a.Loco.Current, a.Loco.Shot?.Name, a.Parry?.CurrentName));
                        }
                    }

                    lastTurn = turnNow;
                    lastTurnDeg = deg;
                }
                else
                {
                    lastTurnDeg = 0f;
                }

                bladeWas = wayNow;
                bladeKnown = true;
            }
            measuresA.Sample(dt, clock, "A");
            measuresB.Sample(dt, clock, "B");
            filmA.Add(clock, "A", a.Loco, a.Poser, a.Anim);
            filmB.Add(clock, "B", b.Loco, b.Poser, b.Anim);
            Vector3 gap = b.Loco.Position - a.Loco.Position;
            if (new Vector2(gap.x, gap.z).magnitude < nearest)
            {
                nearest = new Vector2(gap.x, gap.z).magnitude;
                nearestAt = clock;
                nearestWhat = $"A {a.Loco.Current} {a.Loco.Shot?.Name}, B {b.Loco.Current} {b.Loco.Shot?.Name}{(b.Combo != null && b.Combo.Playing ? " (combo)" : "")}{(a.Combo != null && a.Combo.Playing ? " (A combo)" : "")}";
            }
            foreach (var f in new[] { a, b })
            {
                foreach (var id in new[] { PartId.HandR, PartId.HandL, PartId.Head, PartId.FootL, PartId.FootR })
                {
                    Vector3 now = f.Anim.Part(id).position;
                    if (lastPos.TryGetValue((f, id), out var was) && !frozen)
                    {
                        float v = (now - was).magnitude / dt;
                        if (v > worstPart)
                        {
                            worstPart = v;
                            worstWhat = $"{(f == a ? "A" : "B")}'s {id} at {clock:0.00} s ({f.Loco.Current} {f.Loco.Shot?.Name} at {f.Loco.ShotTime:0.00} s{(f.Combo != null && f.Combo.Playing ? ", in a combo" : "")}{(f.Blow.HasValue ? ", a blow coming" : "")})";
                        }
                    }

                    lastPos[(f, id)] = now;
                    if (float.IsNaN(now.x))
                    {
                        Console.WriteLine($"NaN in {id} at {clock:0.00} s");
                        return 1;
                    }
                }
            }
        }

        foreach (var line in log)
        {
            Console.WriteLine(line);
        }

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

        File.WriteAllText(Path.Combine(dir, "a.json"), filmA.ToString());
        File.WriteAllText(Path.Combine(dir, "b.json"), filmB.ToString());
        Console.WriteLine(string.Format(ci, "A's weapon out of its strikes flicked one way and straight back (over 1.5 deg each step) {0} times", flicks));
        Console.WriteLine(string.Format(ci, "A ({0}): {1} blows thrown ({2} combos): {3} landed, {4} blocked, {5} parried. B took {6} hits.",
            a.Stance, thrownA, combosA, a.Strikes.Hits, a.Strikes.Blocks, a.Strikes.Parries, b.Hits));
        Console.WriteLine(string.Format(ci, "B ({0}): {1} blows thrown ({2} combos): {3} landed, {4} blocked, {5} parried. A took {6} hits.",
            b.Stance, thrownB, combosB, b.Strikes.Hits, b.Strikes.Blocks, b.Strikes.Parries, a.Hits));
        Console.WriteLine(string.Format(ci, "No strike reached {0} times; the bodies came within {1:0.00} m at {4:0.00} s ({5}; root to root, the fighting distance {2:0.00}); fastest hand, head or foot {3:0.0} m/s ({6})",
            outOfReach, nearest, ClipFighter.FightingDistance(a.Size, b.Size), worstPart, nearestAt, nearestWhat, worstWhat));
        return 0;
    }

    /// <summary>A Medium with a sword, and a shield (a board on its left slot, as dummyhits gives it) unless it defends open or with the sword.</summary>
    private static ClipFighter Fighter(string name, string stance, Vector3 at, float facing, float skill)
    {
        // ("club": a Large with its club alone, parrying with it, its free hand tucked, as the lab arms it.)
        bool club = stance == "club";
        string bodyName = club ? "Large" : "Medium";
        var rigFile = RigFile.Parse(File.ReadAllText(Path.Combine(Application.dataPath, $"_Battler/Data/Rigs/{bodyName.ToLowerInvariant()}_rig.json")));
        var profile = UnitProfile.CreateDefault();
        profile.body = bodyName;
        var root = UnitBuilder.Build(rigFile, profile, name, _ => null, name == "A" ? 0 : 1);
        var kind = stance == "sword" || club ? ClipFighter.Stances.Sword : stance == "open" ? ClipFighter.Stances.Open : ClipFighter.Stances.Shield;
        var f = new ClipFighter(root, root.GetComponent<UnitRig>(), profile, at, facing, 0f, LocoLibrary.CombatPrefix(bodyName), physical: false);
        f.Poser.BodyKey = bodyName;
        f.Rig.SlotWeaponR.localRotation = UnitController.MainGrip;
        f.Anim.SlotWeaponR.localRotation = UnitController.MainGrip;
        f.Poser.CarryR = LocoBody.Carry.Weapon;
        f.Poser.BladeR = UnitController.MainGrip * Vector3.forward;
        var sword = WeaponDefinition.CreateDefault(club ? WeaponKind.Club : WeaponKind.Sword);
        f.Poser.WeaponLength = sword.length * f.Size;
        if (kind == ClipFighter.Stances.Shield)
        {
            f.Rig.SlotWeaponL.localRotation = UnitController.ShieldGrip;
            f.Anim.SlotWeaponL.localRotation = UnitController.ShieldGrip;
            f.Poser.CarryL = LocoBody.Carry.Shield;
            var board = new GameObject(name + " shield");
            board.transform.SetParent(f.Rig.SlotWeaponL, false);
            var box = board.AddComponent<BoxCollider>();
            box.size = new Vector3(0.522f, 0.522f, 0.05f);   // (P_Weapon_Shield's)
            box.center = new Vector3(0f, 0f, 0.075f);
            box.isTrigger = true;
            f.ShieldColliders.Add(box);
        }

        f.Stance = kind;
        f.ShieldSkill = f.ParrySkill = skill;
        f.EnableStrikes(sword);
        f.Strikes.Mass = 70f * f.Size * f.Size * f.Size;
        return f;
    }
}
