// `parries`: milestone 11's defences tried blow by blow. Each body's weapon blows (every move it
// throws, at the chest, the head and the belly) at a defender of each body that always defends (a
// shield-bearer blocks, the others parry), from where it would engage. For each: what the blow met
// (parried, blocked, a guarding arm, a body part, nothing), how far the parry or block was up when
// the blow got there, and for a blow that got past, how far its blade passed from the parrying weapon.
//   PARRY_DEFENDER=Small (one defender), PARRY_SKILL=0.5 (or "presets": 0.2, 0.5 and 0.8 in turn, each
//   blow three times, the defender's own chances: milestone 11's pressure runner, shelled up)
using System;
using System.Collections.Generic;
using System.Globalization;
using PhysicsBattler.Combat;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

internal static class ParryReport
{
    public static int Run()
    {
        string only = Environment.GetEnvironmentVariable("PARRY_DEFENDER");
        string skillText = Environment.GetEnvironmentVariable("PARRY_SKILL") ?? "0.5";
        Environment.SetEnvironmentVariable("AIM_STYLES", "1");
        if (skillText == "presets")
        {
            foreach (float preset in new[] { 0.2f, 0.5f, 0.8f })
            {
                var shares = new Dictionary<string, (int blows, int defended)>();
                for (int round = 0; round < 3; round++)
                {
                    foreach (string defenderBody in new[] { "Small", "Medium", "Large" })
                    {
                        if (only != null && only != defenderBody) continue;
                        foreach (string attackerBody in new[] { "Small", "Medium", "Large" })
                        {
                            var weapon = attackerBody == "Medium" ? SwingReport.Sword() : SwingReport.Club();
                            foreach (var move in weapon.moves)
                            {
                                foreach (var part in new[] { AimPart.Chest, AimPart.Head, AimPart.Belly })
                                {
                                    if (Try(attackerBody, weapon, move, defenderBody, part, preset, out _, out bool defended, true, round) == null) continue;
                                    shares.TryGetValue(defenderBody, out var t);
                                    shares[defenderBody] = (t.blows + 1, t.defended + (defended ? 1 : 0));
                                }
                            }
                        }
                    }
                }

                foreach (var pair in shares)
                {
                    Console.WriteLine($"skill {preset:0.0}, {pair.Key} shelled up: defended {pair.Value.defended} of {pair.Value.blows} blows ({100f * pair.Value.defended / Mathf.Max(1, pair.Value.blows):0}%; gate 30–60%)");
                }
            }

            return 0;
        }

        float skill = float.Parse(skillText, CultureInfo.InvariantCulture);
        var totals = new Dictionary<string, (int tried, int defended)>();
        foreach (string defenderBody in new[] { "Small", "Medium", "Large" })
        {
            if (only != null && only != defenderBody)
            {
                continue;
            }

            foreach (string attackerBody in new[] { "Small", "Medium", "Large" })
            {
                var weapon = attackerBody == "Medium" ? SwingReport.Sword() : SwingReport.Club();
                foreach (var move in weapon.moves)
                {
                    foreach (var part in new[] { AimPart.Chest, AimPart.Head, AimPart.Belly })
                    {
                        string result = Try(attackerBody, weapon, move, defenderBody, part, skill, out bool tried, out bool defended);
                        if (result == null)
                        {
                            continue;
                        }

                        Console.WriteLine($"{defenderBody} vs {attackerBody} {move.name.Replace("MD_", "")} at the {part}: {result}");
                        if (tried)
                        {
                            totals.TryGetValue(defenderBody, out var t);
                            totals[defenderBody] = (t.tried + 1, t.defended + (defended ? 1 : 0));
                        }
                    }
                }
            }
        }

        foreach (var pair in totals)
        {
            Console.WriteLine($"{pair.Key}: defended {pair.Value.defended} of {pair.Value.tried} blows it tried to ({100f * pair.Value.defended / Mathf.Max(1, pair.Value.tried):0}%)");
        }

        return 0;
    }

    private static string Try(string attackerBody, WeaponDefinition weapon, MoveDefinition move, string defenderBody, AimPart part, float skill, out bool tried, out bool defended,
        bool real = false, int round = 0)
    {
        tried = defended = false;
        if (!move.UsableBy(attackerBody == "Small" ? 0.55f : attackerBody == "Large" ? 1.45f : 1f) || !move.targets.Contains(part))
        {
            return null;
        }

        var attacker = AimMatrix.Build(attackerBody, Vector3.zero, 0f, 0);
        var defender = AimMatrix.Build(defenderBody, Vector3.forward * 5f, 180f, 1);
        var both = new[] { attacker, defender };
        try
        {
            attacker.DealsHits = true;
            attacker.Equip(weapon, null);
            if (defenderBody == "Medium")
            {
                defender.Equip(SwingReport.Sword(), WeaponDefinition.CreateDefault(WeaponKind.Shield));
            }
            else
            {
                defender.Equip(SwingReport.Club(), null);
            }

            Smoke.GiveColliders(attacker);
            Smoke.GiveColliders(defender);
            defender.Profile.defenceSkill = skill;
            if (!real)
            {
                defender.Style.parryChance = 1f;
                defender.Style.blockChance = 1f;
            }
            else
            {
                // (Its own chances, shelled up, a different roll each round.)
                typeof(UnitController).GetField("combatRandom", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    ?.SetValue(defender, new System.Random(1000 + 97 * round + move.name.Length * 7 + (int)part));
                for (int i = 0; i < 3; i++) defender.Tempo.NoteIncoming(true);
                defender.Tempo.Force(PhysicsBattler.Motion.TempoState.Shell);
            }

            defender.Opponent = attacker;
            attacker.Opponent = defender;
            float distance = attacker.Style.engageDistance * attacker.Body.SizeFactor + 0.25f * defender.Body.SizeFactor;
            defender.Place(Vector3.forward * distance, 180f);
            for (int i = 0; i < 30; i++)
            {
                attacker.PreviewPose(1f / 60f);
                defender.PreviewPose(1f / 60f);
            }

            var state = MovePlanner.Prepare(attacker, defender, move, part);
            if (state == null || !MovePlanner.Acceptable(attacker, state))
            {
                return null;
            }

            HitReport first = null;
            string defence = null;
            void OnHit(HitReport h)
            {
                if (first == null && h.Attacker == attacker)
                {
                    first = h;
                }
            }

            void OnDefending(UnitController unit, UnitController by, MoveState m, string what)
            {
                if (unit == defender && defence == null)
                {
                    defence = what;
                }
            }

            UnitController.Hit += OnHit;
            UnitController.Defending += OnDefending;
            float upAtContact = -1f, gap = float.MaxValue;
            try
            {
                if (!attacker.StartMove(state))
                {
                    return null;
                }

                for (int i = 0; i < 300 && attacker.IsAttacking && first == null; i++)
                {
                    attacker.PreviewPose(1f / 60f);
                    defender.PreviewPose(1f / 60f);
                    float up = defender.Loadout.HasShield ? defender.BlockBlend : defender.ParryBlend;
                    if (state.ContactPassed && upAtContact < 0f)
                    {
                        upAtContact = up;
                    }

                    if (state.Active && defender.MainWeapon != null && !defender.Loadout.HasShield)
                    {
                        attacker.StrikingPart(state, out var g, out var t, out _);
                        var dw = defender.MainWeapon;
                        Vector3 a = dw.transform.position, b = dw.transform.position + dw.transform.rotation * (Vector3.forward * dw.definition.length * dw.Scale);
                        gap = Mathf.Min(gap, SegmentGap(g, t, a, b));
                    }
                }
            }
            finally
            {
                UnitController.Hit -= OnHit;
                UnitController.Defending -= OnDefending;
            }

            tried = defence != null && (defence.StartsWith("parries") || defence.StartsWith("raises the shield"));
            defended = first != null && (first.Blocked || first.ArmGuard);
            string met = first == null ? "missed" : first.Parried ? "PARRIED" : first.Shield ? "BLOCKED" : first.ArmGuard ? $"guarding arm ({first.Part})" : $"struck the {first.Part}";
            string up2 = upAtContact >= 0f ? $"{upAtContact:0.00}" : "-";
            string g2 = gap < float.MaxValue && !defended ? $", blades passed {gap * 100f:0} cm apart" : "";
            return $"{met}; {(defence ?? "never saw it")}; up at contact {up2}{g2}";
        }
        finally
        {
            foreach (var u in both)
            {
                UnityEngine.Object.DestroyImmediate(u.gameObject);
            }
        }
    }

    /// <summary>The closest two segments come.</summary>
    private static float SegmentGap(Vector3 p1, Vector3 q1, Vector3 p2, Vector3 q2)
    {
        float best = float.MaxValue;
        for (int i = 0; i <= 10; i++)
        {
            Vector3 p = Vector3.Lerp(p1, q1, i / 10f);
            Vector3 d = q2 - p2;
            float t = Mathf.Clamp01(Vector3.Dot(p - p2, d) / Mathf.Max(1e-8f, d.sqrMagnitude));
            best = Mathf.Min(best, Vector3.Distance(p, p2 + d * t));
        }

        return best;
    }
}
