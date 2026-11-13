using System;
using System.Collections.Generic;
using System.Linq;
using PhysicsBattler.Locomotion;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

/// <summary>
/// The legs read as a body's legs over a fight (the plan "Grounded legs", step 0; a measure only): how long the pose's
/// legs, the body's and the clip's own are past a leg's ranges (the thigh's place, which way the knee folds, its bend,
/// the ankle), by what the fighter is doing. The ranges are set from the clips (<see cref="LegLimits"/>, <c>legrange</c>):
/// where the clip's own legs read past them in a fight, it is a blend of clips that no clip has.
/// </summary>
public sealed class LegMeasure
{
    private sealed class Tally
    {
        public int Steps;
        public readonly int[] Pose = new int[5], Body = new int[5], Clip = new int[5];   // (place, turn, bend, ankle, any)
        public readonly float[] PoseWorst = new float[4], ClipWorst = new float[4];
        public float Worst;
        public string WorstAt = "";
    }

    private static readonly string[] What = { "the thigh's place", "the knee's fold", "the knee's bend", "the ankle" };
    private static readonly string[] Modes = { "standing", "walking", "in a clip" };
    private readonly List<UnitController> units;
    private readonly Tally[] tallies = { new Tally(), new Tally(), new Tally() };
    private float clock;

    public LegMeasure(List<UnitController> units)
    {
        this.units = units;
    }

    private static float[] Pasts(LegReading r) => new[] { r.PastPlace, r.PastTurn, r.PastBend, r.PastAnkle };

    private static LegReading ReadLeg(Func<PartId, Transform> part, bool left)
    {
        Transform hips = part(PartId.Pelvis), thigh = part(left ? PartId.ThighL : PartId.ThighR), shin = part(left ? PartId.ShinL : PartId.ShinR), foot = part(left ? PartId.FootL : PartId.FootR);
        return LegLimits.Read(hips.rotation, thigh.position, shin.position, foot.position, foot.rotation * Vector3.forward, left);
    }

    public void Step(float dt)
    {
        clock += dt;
        foreach (var u in units)
        {
            var clip = u.Clip;
            if (clip == null || !u.IsAlive || clip.Fallen || !clip.Snapped)
            {
                continue;
            }

            var t = tallies[Mathf.Clamp((int)clip.Loco.Current, 0, 2)];
            for (int side = 0; side < 2; side++)
            {
                bool left = side == 0;
                if (clip.Rig.IsSevered(left ? PartId.FootL : PartId.FootR))
                {
                    continue;
                }

                var pose = ReadLeg(id => clip.Anim.Part(id), left);
                var body = ReadLeg(id => clip.Rig.Part(id).transform, left);
                var own = LegLimits.ReadClip(clip.Loco.RawPoints, (int)LocoBone.Hips, (int)LocoBone.Spine, (int)LocoBone.LeftUpLeg, (int)LocoBone.RightUpLeg, left);
                float[] p = Pasts(pose), b = Pasts(body), c = Pasts(own);
                t.Steps++;
                for (int i = 0; i < 4; i++)
                {
                    t.Pose[i] += p[i] > 10f ? 1 : 0;
                    t.Body[i] += b[i] > 10f ? 1 : 0;
                    t.Clip[i] += c[i] > 10f ? 1 : 0;
                    t.PoseWorst[i] = Mathf.Max(t.PoseWorst[i], p[i]);
                    t.ClipWorst[i] = Mathf.Max(t.ClipWorst[i], c[i]);
                }

                t.Pose[4] += pose.Worst > 10f ? 1 : 0;
                t.Body[4] += body.Worst > 10f ? 1 : 0;
                t.Clip[4] += own.Worst > 10f ? 1 : 0;
                if (pose.Worst > t.Worst)
                {
                    t.Worst = pose.Worst;
                    t.WorstAt = $"at {clock:0.00} s {u.name}'s {(left ? "left" : "right")} leg ({(clip.Loco.Shot != null ? clip.Loco.Shot.Name : clip.Loco.Current.ToString())}): raised {pose.Raised:0}° toward {pose.Toward:0}° (highest there {LegLimits.Highest(pose.Toward):0}), the knee folding {pose.Turn:0}° off straight back, bent {pose.Bend:0}°, toes {pose.Point:0}° down";
                }
            }
        }
    }

    public string Report()
    {
        var sb = new System.Text.StringBuilder();
        if (tallies.Sum(t => t.Steps) == 0)
        {
            return "";
        }

        sb.AppendLine("    the legs read as a body's legs (a leg's ranges as the clips have them, nothing is held to them: the thigh forward 125°, to the side 75, back 55, across 30; the knee turned out to 85° and in to 50 from folding straight back; bent to 155; the ankle 35 up, 60 down); over 10° past, of the time:");
        for (int m = 0; m < 3; m++)
        {
            var t = tallies[m];
            if (t.Steps == 0)
            {
                continue;
            }

            var parts = new List<string>();
            for (int i = 0; i < 4; i++)
            {
                if (t.PoseWorst[i] > 10f || t.ClipWorst[i] > 10f)
                {
                    parts.Add($"{What[i]} {100f * t.Pose[i] / t.Steps:0.0}% (worst {t.PoseWorst[i]:0}°; the clip {100f * t.Clip[i] / t.Steps:0.0}%, {t.ClipWorst[i]:0}°)");
                }
            }

            sb.AppendLine($"      {Modes[m]} ({t.Steps / 120f:0} unit-seconds): the pose's legs {100f * t.Pose[4] / t.Steps:0.0}%, the body's {100f * t.Body[4] / t.Steps:0.0}%, the clip's own {100f * t.Clip[4] / t.Steps:0.0}%{(parts.Count > 0 ? ": " + string.Join("; ", parts) : "")}");
            if (t.Worst > 30f)
            {
                sb.AppendLine($"          worst {t.WorstAt}");
            }
        }

        return sb.ToString();
    }
}
