// What passes through what (the plan "A different approach", step 1): each step of a fight on the real engine, how
// deep each living unit's blade, shield and body are inside another's, by the engine's own overlap query.
#if PHYSX
using System;
using System.Collections.Generic;
using System.Text;
using PhysicsBattler.Engine;
using PhysicsBattler.Units;
using UnityEngine;

internal sealed class Overlaps
{
    private sealed class Kind
    {
        public string Name;
        public float From;          // deeper than this counts (metres)
        public int Steps, Episodes;
        public float Deepest;
        public string DeepestWhat = "";
        public readonly SortedDictionary<string, int> Who = new SortedDictionary<string, int>();   // steps by who was in it
        public readonly HashSet<(Collider, Collider)> Now = new HashSet<(Collider, Collider)>(), Last = new HashSet<(Collider, Collider)>();
    }

    private readonly Kind bladeBody = new Kind { Name = "a blade in another's body", From = 0.02f };
    private readonly Kind bladeShield = new Kind { Name = "a blade in a shield", From = 0.01f };
    private readonly Kind bladeBlade = new Kind { Name = "a blade through a blade", From = 0.005f };
    private readonly Kind shieldBody = new Kind { Name = "a shield in another's body", From = 0.02f };
    private readonly Kind bodyBody = new Kind { Name = "a body in another's body", From = 0.02f };
    private readonly Kind bladeSelf = new Kind { Name = "a blade in its own body", From = 0.02f };
    private readonly PhysXWorld world;
    private int steps;
    private readonly Dictionary<UnitController, PhysicsBattler.Authority.UpperBodyArticulation> uppers = new Dictionary<UnitController, PhysicsBattler.Authority.UpperBodyArticulation>();

    public Overlaps(PhysXWorld world)
    {
        this.world = world;
    }

    private static bool Up(UnitController u) => u != null && u.IsAlive && u.Clip != null && !u.Clip.Fallen;

    public void Step(IReadOnlyList<UnitController> units)
    {
        steps++;
        foreach (var k in All)
        {
            k.Last.Clear();
            foreach (var p in k.Now) k.Last.Add(p);
            k.Now.Clear();
        }

        for (int i = 0; Trace && i < units.Count; i++)
        {
            var upper = units[i].Clip?.Upper;
            if (!uppers.TryGetValue(units[i], out var had) || had != upper)
            {
                uppers[units[i]] = upper;
                Console.WriteLine($"UPPER {Time.time:0.000} {units[i].name} at {units[i].Root.position.x:0.0},{units[i].Root.position.z:0.0}: {(upper == null ? "none" : $"built, {upper.Own.Count} parts, {upper.Colliders.Count} colliders")}");
            }
        }

        for (int i = 0; Trace && i < units.Count; i++)
        {
            var upper = units[i].Clip?.Upper;
            if (upper != null && upper.HeldTurn > 4f)
            {
                Console.WriteLine($"HELD {Time.time:0.000} {units[i].name} at {units[i].Root.position.x:0.0},{units[i].Root.position.z:0.0}: turned in the hand {upper.HeldTurn:0.0}° in the step");
            }
        }

        for (int i = 0; i < units.Count; i++)
        {
            var u = units[i];
            if (!Up(u)) continue;
            foreach (var blade in u.ClipBladeColliders)
            {
                foreach (var part in u.Rig.Parts)
                {
                    // (Its own weapon arm holds it.)
                    if (part.partCollider != null && part.id != PhysicsBattler.Rig.PartId.HandR && part.id != PhysicsBattler.Rig.PartId.ForearmR && !u.Rig.IsSevered(part.id))
                    {
                        Note(bladeSelf, blade, part.partCollider, u, u, part.id.ToString());
                    }
                }
            }

            for (int j = 0; j < units.Count; j++)
            {
                var v = units[j];
                if (j == i || !Up(v) || (u.Root.position - v.Root.position).sqrMagnitude > 3f * 3f) continue;
                foreach (var blade in u.ClipBladeColliders)
                {
                    foreach (var part in v.Rig.Parts)
                    {
                        if (part.partCollider != null && !v.Rig.IsSevered(part.id)) Note(bladeBody, blade, part.partCollider, u, v, part.id.ToString());
                    }

                    foreach (var board in v.Clip.ShieldColliders) Note(bladeShield, blade, board, u, v, "shield");
                    if (j > i)
                    {
                        foreach (var other in v.ClipBladeColliders) Note(bladeBlade, blade, other, u, v, "blade");
                    }
                }

                foreach (var board in u.Clip.ShieldColliders)
                {
                    foreach (var part in v.Rig.Parts)
                    {
                        if (part.partCollider != null && !v.Rig.IsSevered(part.id)) Note(shieldBody, board, part.partCollider, u, v, part.id.ToString());
                    }
                }

                if (j > i)
                {
                    foreach (var a in u.Rig.Parts)
                    {
                        if (a.partCollider == null || u.Rig.IsSevered(a.id)) continue;
                        foreach (var b in v.Rig.Parts)
                        {
                            if (b.partCollider != null && !v.Rig.IsSevered(b.id) && (a.body.position - b.body.position).sqrMagnitude < 0.8f * 0.8f)
                            {
                                Note(bodyBody, a.partCollider, b.partCollider, u, v, $"{a.id} in {b.id}");
                            }
                        }
                    }
                }
            }
        }
    }

    private IEnumerable<Kind> All
    {
        get
        {
            yield return bladeBody;
            yield return bladeShield;
            yield return bladeBlade;
            yield return shieldBody;
            yield return bodyBody;
            yield return bladeSelf;
        }
    }

    private void Note(Kind kind, Collider a, Collider b, UnitController u, UnitController v, string what)
    {
        if (a == null || b == null || !a.enabled || !b.enabled) return;
        float depth = world.Overlap(a, b);
        if (depth <= kind.From) return;
        kind.Steps++;
        if (Breakdown)
        {
            string who = $"{Body(u)} into {Body(v)}{(u.Team == v.Team && u != v ? " (same side)" : "")}, {Where(what)}";
            kind.Who[who] = kind.Who.TryGetValue(who, out int n) ? n + 1 : 1;
        }

        if (Trace && u != v)
        {
            Collider sa = Solid(u, a), sb = Solid(v, b);
            float solids = sa != null && sb != null ? world.Overlap(sa, sb) : float.NaN;
            float apart = sa != null ? Vector3.Distance(sa.transform.position, a.transform.position) : float.NaN;
            float apartB = sb != null ? Vector3.Distance(sb.transform.position, b.transform.position) : float.NaN;
            Console.WriteLine($"OVERLAP {Time.time:0.000} {kind.Name}: {u.name} at {u.Root.position.x:0.0},{u.Root.position.z:0.0} into {v.name} at {v.Root.position.x:0.0},{v.Root.position.z:0.0} {what}: {depth * 100f:0.0} cm; solids {solids * 100f:0.0} cm; "
                + $"solid from shadow {apart * 100f:0.0} / {apartB * 100f:0.0} cm; jumps {u.Clip.Upper?.Jumps} / {v.Clip.Upper?.Jumps}; "
                + $"engine: {world.EngineShape(sa)} / {world.EngineShape(sb)}; hand off {(u.Clip.Upper != null && u.Clip.Upper.Holds(PhysicsBattler.Rig.PartId.HandR) ? u.Clip.Upper.Off(PhysicsBattler.Rig.PartId.HandR) * 100f : float.NaN):0.0} cm; "
                + $"shield hand off {(u.Clip.Upper != null && u.Clip.Upper.Holds(PhysicsBattler.Rig.PartId.HandL) ? u.Clip.Upper.Off(PhysicsBattler.Rig.PartId.HandL) * 100f : float.NaN):0.0} cm; "
                + $"struck: hand off {(v.Clip.Upper != null && v.Clip.Upper.Holds(PhysicsBattler.Rig.PartId.HandR) ? v.Clip.Upper.Off(PhysicsBattler.Rig.PartId.HandR) * 100f : float.NaN):0.0} cm, shield hand off {(v.Clip.Upper != null && v.Clip.Upper.Holds(PhysicsBattler.Rig.PartId.HandL) ? v.Clip.Upper.Off(PhysicsBattler.Rig.PartId.HandL) * 100f : float.NaN):0.0} cm; "
                + $"{u.Clip.Loco.Current} {(u.Clip.Loco.Current == PhysicsBattler.Locomotion.Locomotor.Mode.OneShot ? u.Clip.Loco.Shot?.Name : "")} / {v.Clip.Loco.Current} {(v.Clip.Loco.Current == PhysicsBattler.Locomotion.Locomotor.Mode.OneShot ? v.Clip.Loco.Shot?.Name : "")}");
        }

        if (Trace && u != v && kind == shieldBody && a is BoxCollider board && b is CapsuleCollider limb)
        {
            Vector3 size = Vector3.Scale(board.size, board.transform.lossyScale);
            Vector3 axis = limb.direction == 0 ? Vector3.right : limb.direction == 1 ? Vector3.up : Vector3.forward;
            float half = Mathf.Max(0f, limb.height * 0.5f - limb.radius);
            Vector3 p0 = board.transform.InverseTransformPoint(limb.transform.TransformPoint(limb.center - axis * half)) - board.center;
            Vector3 p1 = board.transform.InverseTransformPoint(limb.transform.TransformPoint(limb.center + axis * half)) - board.center;
            p0 = Vector3.Scale(p0, board.transform.lossyScale);
            p1 = Vector3.Scale(p1, board.transform.lossyScale);
            Console.WriteLine($"   BOARD {size.x * 100f:0.0} x {size.y * 100f:0.0} x {size.z * 100f:0.0} cm; the limb (radius {limb.radius * 100f:0.0} cm) from {p0.x * 100f:0.0} {p0.y * 100f:0.0} {p0.z * 100f:0.0} to {p1.x * 100f:0.0} {p1.y * 100f:0.0} {p1.z * 100f:0.0} cm in it");
        }

        kind.Now.Add((a, b));
        if (!kind.Last.Contains((a, b))) kind.Episodes++;
        if (depth > kind.Deepest)
        {
            kind.Deepest = depth;
            kind.DeepestWhat = $"{u.name}'s into {v.name}'s {what} at {Time.time:0.00} s";
        }
    }

    /// <summary>OVERLAP_WHO=1: each kind's time split by whether each unit had its physical upper body and what was struck.</summary>
    private static readonly bool Breakdown = Environment.GetEnvironmentVariable("OVERLAP_WHO") == "1";

    /// <summary>OVERLAP_TRACE=1: a line for each step one unit's blade, shield or body is in another's.</summary>
    private static readonly bool Trace = Environment.GetEnvironmentVariable("OVERLAP_TRACE") == "1";

    /// <summary>The solid that stands for a rig's collider on the unit's physical upper body (null: none).</summary>
    private static Collider Solid(UnitController u, Collider of)
    {
        if (u.Clip.Upper == null) return null;
        Collider best = null;
        float least = 0.02f;
        foreach (var c in u.Clip.Upper.Colliders)
        {
            if (c == null || c.name != of.name + " (solid)") continue;
            float d = Vector3.Distance(c.transform.position, of.transform.position);
            if (d < least)
            {
                least = d;
                best = c;
            }
        }

        return best;
    }

    private static string Body(UnitController u) => u.Clip.Upper != null ? "physical" : "posed";

    private static string Where(string what)
    {
        if (what.Contains(" in ")) return "bodies";
        return what.StartsWith("Thigh") || what.StartsWith("Shin") || what.StartsWith("Foot") || what == "Pelvis" ? "legs or pelvis" : what == "shield" || what == "blade" ? what : "upper body";
    }

    public string Report()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"  what passed through what, living units on their feet ({steps / 60f:0} s):");
        foreach (var k in All)
        {
            sb.AppendLine(k.Steps == 0
                ? $"    {k.Name} (over {k.From * 100f:0.#} cm): never"
                : $"    {k.Name} (over {k.From * 100f:0.#} cm): {k.Episodes} times, {k.Steps / 60f:0.0} s in all, deepest {k.Deepest * 100f:0.0} cm ({k.DeepestWhat})");
            foreach (var w in k.Who) sb.AppendLine($"        {w.Value / 60f:0.00} s {w.Key}");
        }

        return sb.ToString();
    }
}
#endif
