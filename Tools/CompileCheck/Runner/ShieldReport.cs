// `shield [dir]`: milestone 9's shield runner. A medium sword-and-shield unit stands at the origin
// facing +Z; a medium swordsman attacks it from 12 directions (every 30 degrees), each sword move in
// turn. It may block only what it sees, and only inside the cone its shield arm covers (it turns to
// face a seen blow from outside it, if there is time). Prints, for each blow, how the defender saw
// it, what it did and what the blow struck, with the shield's angle to its threat, its clearance from
// its own body and how far its hand turned from the guard; with a folder, writes every step's pose
// and shield disc to CSV for Tools/Playtest/shieldview.py to draw.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using PhysicsBattler.Combat;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using PhysicsBattler.Tests;
using PhysicsBattler.Units;
using UnityEngine;

internal static class ShieldReport
{
    public static int Run(string dir)
    {
        if (dir != null) Directory.CreateDirectory(dir);
        var ci = CultureInfo.InvariantCulture;
        var sword = WeaponDefinition.CreateDefault(WeaponKind.Sword);
        sword.moves = new[] { MoveDefinition.Create(MoveStyle.SwordCut), MoveDefinition.Create(MoveStyle.SwordBackhand), MoveDefinition.Create(MoveStyle.SwordCleave), MoveDefinition.Create(MoveStyle.SwordThrust) };
        var shield = WeaponDefinition.CreateDefault(WeaponKind.Shield);

        UnitController defender = null;
        var attacker = Build("Medium", Vector3.forward * 3f, 180f, 1, CombatStyle.CreateFor("Medium"));
        attacker.Equip(sword, null);
        attacker.DealsHits = true;
        var both = new UnitController[3];
        // The one it is busy with: an enemy in front of it that doesn't strike (where its eyes are).
        var decoy = Build("Medium", Vector3.forward * 2f, 180f, 1, CombatStyle.CreateFor("Medium"));
        decoy.Equip(sword, null);

        // A fresh defender for every blow (a cut can take its arm off).
        void NewDefender()
        {
            if (defender != null) BalanceTests.Destroy(defender);
            UnitController.ResetSeeds(1);
            defender = Build("Medium", Vector3.zero, 0f, 0, CombatStyle.CreateFor("Medium"));
            defender.Equip(sword, shield);
            var face = defender.OffWeapon.gameObject.AddComponent<BoxCollider>();
            face.center = shield.faceCenter;
            face.size = new Vector3(2f * shield.radius, 2f * shield.radius, 0.04f);
            defender.Style.blockChance = 1f;
            both[0] = defender;
            both[1] = attacker;
            both[2] = decoy;
            defender.Opponent = decoy;
        }

        string defended = "";
        void OnDefending(UnitController u, UnitController by, MoveState m, string what)
        {
            if (u == defender) defended = what;
        }

        var hits = new List<HitReport>();
        void OnHit(HitReport h)
        {
            if (h.Attacker == attacker) hits.Add(h);
        }

        UnitController.Defending += OnDefending;
        UnitController.Hit += OnHit;
        int seenBlocked = 0, seenReachable = 0, unseenBlocked = 0, unseen = 0, seenOutside = 0, outsideBlocked = 0;
        float worstClear = float.MaxValue, worstBehind = float.MaxValue;
        int steps = 0, faced = 0, handNear = 0;
        var modeSteps = new Dictionary<string, (int n, int faced)>();
        var csv = new StringBuilder("trial,dir,move,t,blowT");
        foreach (var id in PartInfo.All) csv.Append($",{id}.x,{id}.y,{id}.z");
        csv.Append(",sc.x,sc.y,sc.z,sf.x,sf.y,sf.z,sr,grip.x,grip.y,grip.z,tip.x,tip.y,tip.z,eye.x,eye.y,eye.z,look.x,look.y,look.z,aim.x,aim.y,aim.z,mode");
        csv.AppendLine();
        var table = new StringBuilder();
        table.AppendLine("  from   move         saw it      did                                 struck            face°  clear cm  hand°");
        int trial = 0;
        try
        {
            for (int k = 0; k < 12; k++)
            {
                float angle = k * 30f;   // where the attacker stands, clockwise from the front
                foreach (var move in sword.moves)
                {
                    trial++;
                    NewDefender();
                    float distance = attacker.Style.engageDistance * attacker.Body.SizeFactor + 0.25f * defender.Body.SizeFactor;
                    Vector3 at = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * distance;
                    attacker.Teleport(at, angle + 180f);
                    decoy.Teleport(Vector3.forward * 2f, 180f);
                    decoy.Opponent = defender;
                    BalanceTests.Simulate(both, 45);
                    // The hand's turn against the chest in the guard, to measure the block's against.
                    Quaternion guardHand = Quaternion.Inverse(defender.Anim.Part(PartId.Chest).rotation) * defender.Anim.Part(PartId.HandL).rotation;
                    var state = MovePlanner.Prepare(attacker, defender, move, AimPart.Chest);
                    if (state == null)
                    {
                        table.AppendLine($"  {angle,4:0}°  {move.name,-12} (no blow from here)");
                        continue;
                    }

                    Sight before = defender.SightOf(attacker);
                    defended = "";
                    hits.Clear();
                    if (!attacker.StartMove(state)) continue;
                    float maxFace = 0f, minClear = float.MaxValue, maxHand = 0f;
                    for (int i = 0; i < 400 && attacker.IsAttacking; i++)
                    {
                        BalanceTests.Simulate(both);
                        var p = defender.Pose;
                        var chest = defender.Anim.Part(PartId.Chest);
                        float off = defender.ShieldOffThreat;
                        maxFace = Mathf.Max(maxFace, off);
                        minClear = Mathf.Min(minClear, p.ShieldClearance);
                        Vector3 fwd = chest.rotation * Vector3.forward;
                        fwd.y = 0f;
                        float behind = Vector3.Dot(p.ShieldCenter - chest.position, fwd.normalized);
                        worstBehind = Mathf.Min(worstBehind, behind);
                        Quaternion hand = Quaternion.Inverse(chest.rotation) * defender.Anim.Part(PartId.HandL).rotation;
                        float handTurn = Quaternion.Angle(guardHand, hand);
                        maxHand = Mathf.Max(maxHand, handTurn);
                        steps++;
                        faced += off <= 45f ? 1 : 0;
                        string m = defender.ShieldMode.ToString();
                        var ms = modeSteps.TryGetValue(m, out var v) ? v : (0, 0);
                        modeSteps[m] = (ms.Item1 + 1, ms.Item2 + (off <= 45f ? 1 : 0));
                        handNear += handTurn <= 40f ? 1 : 0;
                        if (dir != null) Row(csv, trial, angle, move.name, i / 60f, state.T, defender, attacker, ci);
                    }

                    worstClear = Mathf.Min(worstClear, minClear);
                    bool shieldTook = hits.Count > 0 && hits[0].Shield;
                    string struck = hits.Count == 0 ? "missed" : hits[0].Shield ? "the shield" : hits[0].Part.ToString();
                    bool saw = defended.StartsWith("raises") || defended.StartsWith("doesn't") || defended.StartsWith("can't") || defended.StartsWith("nothing");
                    bool outside = !ShieldCone.Contains(attacker.Root.position - defender.Root.position, 0f, true);
                    if (!saw)
                    {
                        unseen++;
                        unseenBlocked += shieldTook ? 1 : 0;
                    }
                    else if (outside)
                    {
                        seenOutside++;
                        outsideBlocked += shieldTook ? 1 : 0;
                    }
                    else
                    {
                        seenReachable++;
                        seenBlocked += shieldTook ? 1 : 0;
                    }

                    string did = defended.Length > 36 ? defended.Substring(0, 36) : defended;
                    table.AppendLine($"  {angle,4:0}°  {move.name.Replace("MD_", ""),-12} {before,-10}  {did,-36} {struck,-16} {maxFace,5:0}  {minClear * 100f,7:0.0}  {maxHand,5:0}");
                    BalanceTests.Simulate(both, 20);
                }
            }
        }
        finally
        {
            UnitController.Defending -= OnDefending;
            UnitController.Hit -= OnHit;
        }

        Console.Write(table);
        Console.WriteLine($"seen, in front or on the shield side: {seenBlocked}/{seenReachable} taken on the shield");
        Console.WriteLine($"seen, on the weapon side (outside the cone at first): {outsideBlocked}/{seenOutside} taken on the shield (it turns to face them)");
        Console.WriteLine($"never seen: {unseenBlocked}/{unseen} taken on the shield (should be 0)");
        Console.WriteLine($"during the blows: the shield faced within 45° of its threat {100f * faced / Mathf.Max(1, steps):0}% of the time; its hand within 40° of its guard turn {100f * handNear / Mathf.Max(1, steps):0}%");
        foreach (var pair in modeSteps) Console.WriteLine($"  {pair.Key}: {pair.Value.n} steps, faced within 45° {100f * pair.Value.faced / Mathf.Max(1, pair.Value.n):0}%");
        Console.WriteLine($"the shield's rim came within {worstClear * 100f:0.0} cm of its own body (negative: inside); its middle at most {-Mathf.Min(0f, worstBehind) * 100f:0.0} cm behind the chest");
        if (dir != null) File.WriteAllText(Path.Combine(dir, "shield.csv"), csv.ToString());
        BalanceTests.Destroy(both);
        BalanceTests.Destroy(decoy);
        return 0;
    }

    /// <summary>A unit with its fighting style (the shield style's rhythm holds the shield).</summary>
    internal static UnitController Build(string body, Vector3 at, float yaw, int team, CombatStyle style)
    {
        var rig = PhysicsBattler.Rig.RigFile.Parse(File.ReadAllText(Path.Combine(Application.dataPath, $"_Battler/Data/Rigs/{body.ToLowerInvariant()}_rig.json")));
        var profile = UnitProfile.CreateDefault();
        profile.body = body;
        profile.style = style;
        var root = UnitBuilder.Build(rig, profile, $"{body} {team}", _ => null, 0);
        root.transform.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
        var unit = root.GetComponent<UnitController>();
        unit.Profile = profile;
        unit.Team = team;
        typeof(UnitController).GetMethod("Awake", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(unit, null);
        return unit;
    }

    /// <summary>Back to full health and poise (each blow is a trial of its own).</summary>
    private static void Restore(UnitController u)
    {
        const System.Reflection.BindingFlags any = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
        typeof(UnitController).GetProperty("Health", any).SetValue(u, u.Profile.maxHealth);
        typeof(UnitController).GetProperty("Poise", any).SetValue(u, u.Profile.maxPoise);
    }

    private static void Row(StringBuilder csv, int trial, float angle, string move, float t, float blowT, UnitController u, UnitController a, CultureInfo ci)
    {
        csv.Append(trial).Append(',').Append(angle.ToString("0", ci)).Append(',').Append(move.Replace("MD_", "")).Append(',')
            .Append(t.ToString("0.000", ci)).Append(',').Append(blowT.ToString("0.000", ci));
        foreach (var id in PartInfo.All) P(csv, u.Anim.Part(id).position, ci);
        P(csv, u.Pose.ShieldCenter, ci);
        P(csv, u.Pose.ShieldFace, ci);
        csv.Append(',').Append((u.Loadout.Off != null ? u.Loadout.Off.radius * u.Loadout.Scale : 0f).ToString("0.000", ci));
        var slot = a.Anim.SlotWeaponR;
        P(csv, slot.position, ci);
        P(csv, slot.position + slot.rotation * (a.Loadout.Main.tip * a.Loadout.Scale), ci);
        P(csv, u.EyePosition, ci);
        P(csv, u.EyeRotation * Vector3.forward, ci);
        P(csv, u.ShieldAimPoint, ci);
        csv.Append(',').Append(u.ShieldMode).AppendLine();
    }

    private static void P(StringBuilder csv, Vector3 v, CultureInfo ci)
    {
        csv.Append(',').Append(v.x.ToString("0.000", ci)).Append(',').Append(v.y.ToString("0.000", ci)).Append(',').Append(v.z.ToString("0.000", ci));
    }
}
