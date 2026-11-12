using System;
using System.Collections.Generic;
using System.Linq;
using PhysicsBattler.Locomotion;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

/// <summary>
/// How heavy the weapons are in a fight, measured: for each blow that meets something, how much of its speed the
/// weapon keeps along its way once the hit stop is over (below nought: it came back off what it met), and how far
/// the part it struck and the body's hips go the blow's way after it.
/// </summary>
public sealed class HeftMeasure
{
    private sealed class Hit
    {
        public UnitController By, On;
        public PartId Part;
        public string Kind, Met;
        public Vector3 Way, PartAt, HipsAt;
        public float Speed;
        public int Since = -1;          // (steps since the hit stop ended; -1 while it lasts)
        public float Kept3 = float.NaN, Kept9 = float.NaN, Part6 = float.NaN, Part18 = float.NaN, Hips30 = float.NaN, Hips60 = float.NaN;
        public int Waited;
    }

    private readonly List<UnitController> units;
    private readonly HashSet<UnitController> heard = new HashSet<UnitController>();
    private readonly Dictionary<UnitController, Vector3> lastStrike = new Dictionary<UnitController, Vector3>();
    private readonly Dictionary<UnitController, Vector3> velocity = new Dictionary<UnitController, Vector3>();
    private readonly List<Hit> open = new List<Hit>(), done = new List<Hit>();

    public HeftMeasure(List<UnitController> units)
    {
        this.units = units;
    }

    private static Vector3 Flat(Vector3 v)
    {
        v.y = 0f;
        return v.sqrMagnitude > 1e-8f ? v.normalized : Vector3.zero;
    }

    public void Step(float dt)
    {
        foreach (var u in units)
        {
            var clip = u.Clip;
            if (clip == null || clip.Strikes == null)
            {
                continue;
            }

            if (heard.Add(u))
            {
                var by = u;
                clip.Strikes.Struck += s =>
                {
                    var on = units.FirstOrDefault(x => x.Clip == s.Struck);
                    if (on == null)
                    {
                        return;
                    }

                    open.Add(new Hit
                    {
                        By = by, On = on, Part = s.Shield ? PartId.HandL : s.Parried ? PartId.HandR : s.Part,
                        Kind = clip.Strikes.Weapon != null && clip.Strikes.Weapon.kind == PhysicsBattler.Combat.WeaponKind.Club ? "club" : "sword",
                        Met = s.Shield ? "a shield" : s.Parried ? "a parry" : "a body",
                        Way = s.Direction.sqrMagnitude > 1e-8f ? s.Direction.normalized : Vector3.forward, Speed = Mathf.Max(0.5f, s.Speed),
                        PartAt = on.Clip.Rig.Part(s.Shield ? PartId.HandL : s.Parried ? PartId.HandR : s.Part).transform.position, HipsAt = on.Clip.Rig.Part(PartId.Pelvis).transform.position,
                    });
                };
            }

            if (clip.Strikes.Blade(null, out _, out _, out Vector3 strike))
            {
                if (lastStrike.TryGetValue(u, out Vector3 was))
                {
                    velocity[u] = (strike - was) / dt;
                }

                lastStrike[u] = strike;
            }
        }

        for (int i = open.Count - 1; i >= 0; i--)
        {
            var h = open[i];
            if (h.Since < 0)
            {
                // (Both stand still through the hit stop: what the weapon does is taken from when it ends.)
                if (h.By.Clip.Strikes.HitStop > 0f && h.Waited++ < 30)
                {
                    continue;
                }

                h.Since = 0;
            }

            h.Since++;
            velocity.TryGetValue(h.By, out Vector3 v);
            float along = Vector3.Dot(v, h.Way) / h.Speed;
            Vector3 part = h.On.Clip.Rig.Part(h.Part).transform.position, hips = h.On.Clip.Rig.Part(PartId.Pelvis).transform.position;
            Vector3 flat = Flat(h.Way);
            if (h.Since == 3) h.Kept3 = along;
            if (h.Since == 9) h.Kept9 = along;
            if (h.Since == 6) h.Part6 = Vector3.Dot(part - h.PartAt, h.Way);
            if (h.Since == 18) h.Part18 = Vector3.Dot(part - h.PartAt, h.Way);
            if (h.Since == 30) h.Hips30 = Vector3.Dot(hips - h.HipsAt, flat);
            if (h.Since == 60)
            {
                h.Hips60 = Vector3.Dot(hips - h.HipsAt, flat);
                done.Add(h);
                open.RemoveAt(i);
            }
        }
    }

    private static string Median(IEnumerable<float> values, string format, float scale = 1f)
    {
        var v = values.Where(x => !float.IsNaN(x)).OrderBy(x => x).ToList();
        return v.Count == 0 ? "-" : (v[v.Count / 2] * scale).ToString(format, System.Globalization.CultureInfo.InvariantCulture);
    }

    public string Report()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("    the weapons' weight (each blow that met something; the median of each):");
        foreach (var g in done.Concat(open.Where(h => h.Since >= 18)).GroupBy(h => h.Kind + " on " + h.Met).OrderBy(g => g.Key))
        {
            var hits = g.ToList();
            sb.AppendLine($"      a {g.Key}: {hits.Count} blows at {Median(hits.Select(h => h.Speed), "0.0")} m/s; the weapon's speed along its way after the hit stop: {Median(hits.Select(h => h.Kept3), "0", 100f)}% of it at 0.05 s, "
                + $"{Median(hits.Select(h => h.Kept9), "0", 100f)}% at 0.15 s (below 0: it came back); it came back in {hits.Count(h => h.Kept3 < -0.05f)} of them; "
                + $"the part struck went {Median(hits.Select(h => h.Part6), "0", 100f)} cm the blow's way in 0.1 s and {Median(hits.Select(h => h.Part18), "0", 100f)} cm in 0.3 s; "
                + $"the hips {Median(hits.Select(h => h.Hips30), "0", 100f)} cm in 0.5 s and {Median(hits.Select(h => h.Hips60), "0", 100f)} cm in 1 s");
        }

        return sb.ToString();
    }
}
