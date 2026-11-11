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
        kind.Now.Add((a, b));
        if (!kind.Last.Contains((a, b))) kind.Episodes++;
        if (depth > kind.Deepest)
        {
            kind.Deepest = depth;
            kind.DeepestWhat = $"{u.name}'s into {v.name}'s {what} at {Time.time:0.00} s";
        }
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
        }

        return sb.ToString();
    }
}
#endif
