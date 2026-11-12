using System;
using System.Collections.Generic;
using System.Linq;
using PhysicsBattler.Motion;
using PhysicsBattler.Muscles;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

/// <summary>
/// How often the animation's pose, and the physical body, bend a joint further than a body does (the limit table,
/// <see cref="JointLimitTable.Physical"/>, in each joint's own frame): for each kind of unit and each joint, the
/// share of its time on its feet past its range by over 10 and over 30 degrees, and the worst. And how deep a
/// shield, and the arm that holds it, is in its own head and trunk.
/// </summary>
public sealed class LimitMeasure
{
    private static readonly (PartId part, PartId on)[] Joints =
    {
        (PartId.UpperArmL, PartId.Chest), (PartId.ForearmL, PartId.UpperArmL), (PartId.HandL, PartId.ForearmL),
        (PartId.UpperArmR, PartId.Chest), (PartId.ForearmR, PartId.UpperArmR), (PartId.HandR, PartId.ForearmR),
        (PartId.Head, PartId.Chest), (PartId.Chest, PartId.Spine), (PartId.Spine, PartId.Pelvis),
        (PartId.ThighL, PartId.Pelvis), (PartId.ShinL, PartId.ThighL), (PartId.FootL, PartId.ShinL),
        (PartId.ThighR, PartId.Pelvis), (PartId.ShinR, PartId.ThighR), (PartId.FootR, PartId.ShinR),
    };

    private sealed class Tally
    {
        public int Steps, Over10, Over30, BodyOver10, BodyOver30;
        public float Worst, WorstPitch, WorstY, WorstZ, BodyWorst;
    }

    private sealed class ArmTally
    {
        public int Steps;
        public readonly int[] Over = new int[6], Over30 = new int[6], BodyOver = new int[6], ClipOver = new int[6];   // (reach, turn, bend, roll, wrist, any)
        public readonly float[] Worst = new float[5], BodyWorst = new float[5], ClipWorst = new float[5];
        public string WorstAt = "";
        public int Held, Held30;
        public float HeldMost;
        public float WorstAll;
    }

    private static readonly string[] What = { "elbow's place", "upper arm's turn", "elbow's bend", "forearm's roll", "wrist" };
    private readonly Dictionary<(string, string, string), ArmTally> arms = new Dictionary<(string, string, string), ArmTally>();
    private float clock;

    private static ArmReading ReadArm(Func<PartId, Transform> part, bool left, in ArmReading clip) => ArmLimits.Read(part(PartId.Chest).rotation,
        part(left ? PartId.UpperArmL : PartId.UpperArmR).position, part(left ? PartId.ForearmL : PartId.ForearmR).position,
        part(left ? PartId.HandL : PartId.HandR).position, part(left ? PartId.HandL : PartId.HandR).rotation, left, clip);

    /// <summary>The clip's own arm (the recorded motion, before anything of ours moves it), read the same way.</summary>
    public static ArmReading ReadClipArm(Vector3[] p, bool left) => ArmLimits.ReadClip(p, (int)PhysicsBattler.Locomotion.LocoBone.Neck,
        (int)PhysicsBattler.Locomotion.LocoBone.Spine2, (int)PhysicsBattler.Locomotion.LocoBone.LeftArm, (int)PhysicsBattler.Locomotion.LocoBone.RightArm, left);

    private static float[] Pasts(ArmReading a) => new[] { a.PastReach, a.PastTurn, a.PastBend, a.PastRoll, a.PastWrist };

    private sealed class Hold
    {
        public int Steps, ElbowOut, ElbowUp, WristOver, InFistOver;
        public float Elbow, Wrist, InFist, InFistMost;
    }

    private readonly Dictionary<(string, string), Hold> holds = new Dictionary<(string, string), Hold>();
    private readonly List<UnitController> units;
    private readonly Func<UnitController, string> kind;
    private readonly Dictionary<(string, PartId), Tally> tallies = new Dictionary<(string, PartId), Tally>();
    private readonly Dictionary<string, (int steps, int head, int trunk, float headDeepest, float trunkDeepest)> shields = new Dictionary<string, (int, int, int, float, float)>();
    private static readonly PartId[] ShieldArm = { PartId.ForearmL, PartId.HandL }, Head = { PartId.Head }, Trunk = { PartId.Pelvis, PartId.Spine, PartId.Chest };

    public LimitMeasure(List<UnitController> units, Func<UnitController, string> kind)
    {
        this.units = units;
        this.kind = kind;
        if (Environment.GetEnvironmentVariable("FIGHT_ARMTRACE") == "1")
        {
            foreach (var u in units)
            {
                var named = u;
                if (u.Clip != null)
                {
                    u.Clip.ArmGateTrace = line => Console.WriteLine($"[ArmGate] {named.name} {line}");
                }
            }
        }
    }

    public void Step()
    {
        clock += 1f / 60f;
        foreach (var u in units)
        {
            var clip = u.Clip;
            if (clip == null || !u.IsAlive || clip.Fallen || !clip.Snapped)
            {
                continue;
            }

            string k = kind(u);
            foreach (var (id, on) in Joints)
            {
                var part = clip.Rig.Part(id);
                if (part == null || clip.Rig.IsSevered(id))
                {
                    continue;
                }

                if (!tallies.TryGetValue((k, id), out var t))
                {
                    tallies[(k, id)] = t = new Tally();
                }

                float pose = JointMath.PastLimits(part.JointTurn(clip.Anim.Part(on).rotation, clip.Anim.Part(id).rotation), part.limits, out float pitch, out float y, out float z);
                float body = JointMath.PastLimits(part.JointTurn(clip.Rig.Part(on).transform.rotation, part.transform.rotation), part.limits, out _, out _, out _);
                t.Steps++;
                t.Over10 += pose > 10f ? 1 : 0;
                t.Over30 += pose > 30f ? 1 : 0;
                t.BodyOver10 += body > 10f ? 1 : 0;
                t.BodyOver30 += body > 30f ? 1 : 0;
                t.BodyWorst = Mathf.Max(t.BodyWorst, body);
                if (pose > t.Worst)
                {
                    t.Worst = pose;
                    t.WorstPitch = pitch;
                    t.WorstY = y;
                    t.WorstZ = z;
                }
            }

            string state = clip.SinceStruck < 0.6f ? "just struck" : clip.Striking ? "striking" : "guarding";
            for (int side = 0; side < 2; side++)
            {
                bool left = side == 0;
                if (clip.Rig.IsSevered(left ? PartId.HandL : PartId.HandR))
                {
                    continue;
                }

                var key = (k, left ? "left arm" : "right arm", state);
                if (!arms.TryGetValue(key, out var at))
                {
                    arms[key] = at = new ArmTally();
                }

                var inClip = ReadClipArm(clip.Loco.RawPoints, left);
                var pose = ReadArm(id => clip.Anim.Part(id), left, inClip);
                var body = ReadArm(id => clip.Rig.Part(id).transform, left, inClip);
                float[] p = Pasts(pose), b = Pasts(body);
                at.Held += clip.ArmHeldBy[side] > 0.5f ? 1 : 0;
                at.Held30 += clip.ArmHeldBy[side] > 30f ? 1 : 0;
                at.HeldMost = Mathf.Max(at.HeldMost, clip.ArmHeldBy[side]);
                float[] c = Pasts(inClip);
                at.ClipOver[5] += inClip.Worst > 10f ? 1 : 0;
                at.Steps++;
                for (int i = 0; i < 5; i++)
                {
                    at.ClipOver[i] += c[i] > 10f ? 1 : 0;
                    at.ClipWorst[i] = Mathf.Max(at.ClipWorst[i], c[i]);
                    at.Over[i] += p[i] > 10f ? 1 : 0;
                    at.Over30[i] += p[i] > 30f ? 1 : 0;
                    at.BodyOver[i] += b[i] > 10f ? 1 : 0;
                    at.Worst[i] = Mathf.Max(at.Worst[i], p[i]);
                    at.BodyWorst[i] = Mathf.Max(at.BodyWorst[i], b[i]);
                }

                at.Over[5] += pose.Worst > 10f ? 1 : 0;
                at.Over30[5] += pose.Worst > 30f ? 1 : 0;
                at.BodyOver[5] += body.Worst > 10f ? 1 : 0;
                if (pose.Worst > at.WorstAll)
                {
                    at.WorstAll = pose.Worst;
                    at.WorstAt = $"at {clock:0.00} s {u.name}: raised {pose.Raised:0}° toward {pose.Toward:0}° (highest there {ArmLimits.Highest(pose.Toward):0}), turned {pose.Turn:0}, bent {pose.Bend:0}, rolled {pose.Roll:0}, wrist {pose.Flex:0}/{pose.Thumbward:0}";
                }
            }

            if (clip.Blocking && !clip.Rig.IsSevered(PartId.HandL) && clip.ShieldBoard(out _, out Vector3 boardLooks, out _))
            {
                // (How the shield is held: which way the elbow points (against straight down), the hand's turn on the forearm
                // (the knuckles against the forearm's line), the board's turn in the fist (its face against the knuckles).)
                if (!holds.TryGetValue((k, state), out var h))
                {
                    holds[(k, state)] = h = new Hold();
                }

                Transform su = clip.Anim.Part(PartId.UpperArmL), sf = clip.Anim.Part(PartId.ForearmL), sh = clip.Anim.Part(PartId.HandL);
                Vector3 way = PhysicsBattler.Motion.TwoBoneIK.BendWay(su, sf, sh);
                // (Against the lowest the elbow can be with the fist where it is: straight down, across the line from the shoulder to the fist.)
                Vector3 lowest = Vector3.ProjectOnPlane(Vector3.down, (sh.position - su.position).normalized);
                float elbow = way.sqrMagnitude > 0.5f && lowest.sqrMagnitude > 1e-4f ? Vector3.Angle(way, lowest) : 0f;
                float wrist = Vector3.Angle(sh.rotation * Vector3.down, sh.position - sf.position);
                var bodyHand = clip.Rig.Part(PartId.HandL).transform;
                float inFist = Vector3.Angle(boardLooks, bodyHand.rotation * Vector3.down);
                h.Steps++;
                h.Elbow += elbow;
                h.ElbowOut += elbow > 60f ? 1 : 0;
                h.ElbowUp += sf.position.y > su.position.y + 0.02f ? 1 : 0;
                h.Wrist += wrist;
                h.WristOver += wrist > 30f ? 1 : 0;
                h.InFist += inFist;
                h.InFistOver += inFist > 20f ? 1 : 0;
                h.InFistMost = Mathf.Max(h.InFistMost, inFist);
            }

            if (clip.Upper != null && clip.Blocking && !clip.Rig.IsSevered(PartId.HandL))
            {
                float head = clip.Upper.Deepest(ShieldArm, Head), trunk = clip.Upper.Deepest(ShieldArm, Trunk);
                foreach (string key in new[] { k, k + ", " + state })
                {
                    shields.TryGetValue(key, out var s);
                    shields[key] = (s.steps + 1, s.head + (head > 0.02f ? 1 : 0), s.trunk + (trunk > 0.02f ? 1 : 0), Mathf.Max(s.headDeepest, head), Mathf.Max(s.trunkDeepest, trunk));
                }
            }
        }
    }

    public string Report()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("    joints bent further than a body bends (the limit table; share of the time on its feet the POSE is past its range by over 10° / over 30°, its worst and how it was turned then; then the same for the BODY):");
        foreach (var g in tallies.GroupBy(t => t.Key.Item1).OrderBy(g => g.Key))
        {
            sb.AppendLine($"      {g.Key} ({g.First().Value.Steps / 60f:0} s):");
            foreach (var e in g)
            {
                var t = e.Value;
                if (t.Worst < 10f && t.BodyWorst < 10f)
                {
                    continue;
                }

                var lim = units.First(u => kind(u) == g.Key).Clip.Rig.Part(e.Key.Item2).limits;
                sb.AppendLine($"        {e.Key.Item2,-10} pose {100f * t.Over10 / t.Steps,5:0.0}% / {100f * t.Over30 / t.Steps,5:0.0}%, worst {t.Worst,3:0}° past (about its axis {t.WorstPitch:0} [{lim.pitchMin:0} to {lim.pitchMax:0}], second {t.WorstY:0} [±{lim.twist:0}], third {t.WorstZ:0} [±{lim.swing:0}]);  body {100f * t.BodyOver10 / t.Steps,5:0.0}% / {100f * t.BodyOver30 / t.Steps,5:0.0}%, worst {t.BodyWorst:0}°");
            }
        }

        sb.AppendLine("    arms as a body's arms (read from where the joints are; share of the time the POSE, as it goes to the body, is over 10° past a person's range in any way [over 30°], and the BODY; how often the gate at the end of the pose had to bring the arm back; then each way: pose share, worst pose, worst body):");
        foreach (var e in arms.OrderBy(e => e.Key.Item1).ThenBy(e => e.Key.Item2).ThenBy(e => e.Key.Item3))
        {
            var t = e.Value;
            if (t.Steps < 30)
            {
                continue;
            }

            var parts = new List<string>();
            for (int i = 0; i < 5; i++)
            {
                if (t.Worst[i] > 10f || t.BodyWorst[i] > 10f)
                {
                    parts.Add($"{What[i]} {100f * t.Over[i] / t.Steps:0.0}% (worst {t.Worst[i]:0}°, body {t.BodyWorst[i]:0}°; clip {100f * t.ClipOver[i] / t.Steps:0.0}%, {t.ClipWorst[i]:0}°)");
                }
            }

            sb.AppendLine($"      {e.Key.Item1}, {e.Key.Item2}, {e.Key.Item3} ({t.Steps / 60f:0.0} s): pose {100f * t.Over[5] / t.Steps:0.0}% [{100f * t.Over30[5] / t.Steps:0.0}%], body {100f * t.BodyOver[5] / t.Steps:0.0}%, the clip itself {100f * t.ClipOver[5] / t.Steps:0.0}%; the gate brought it back {100f * t.Held / t.Steps:0.0}% of the time [from over 30° past {100f * t.Held30 / t.Steps:0.0}%, at most {t.HeldMost:0}°]{(parts.Count > 0 ? ": " + string.Join("; ", parts) : "")}");
            if (t.WorstAll > 30f)
            {
                sb.AppendLine($"          worst {t.WorstAt}");
            }
        }

        sb.AppendLine($"      an arm's joint turned over {PhysicsBattler.Locomotion.ClipFighter.ArmJump:0}° in a step: {units.Sum(u => u.Clip != null ? u.Clip.ArmSentJumps : 0)} times in the pose as the stages left it, {units.Sum(u => u.Clip != null ? u.Clip.ArmGoneJumps : 0)} as the gate let it go; of those the gate's own making (the pose sent turned under half as far): {units.Sum(u => u.Clip != null ? u.Clip.ArmGateJumps : 0)}");
        foreach (var e in holds.OrderBy(e => e.Key.Item1).ThenBy(e => e.Key.Item2))
        {
            var h = e.Value;
            if (h.Steps < 30)
            {
                continue;
            }

            sb.AppendLine($"      how the shield is held: {e.Key.Item1}, {e.Key.Item2} ({h.Steps / 60f:0.0} s): the elbow {h.Elbow / h.Steps:0}° round from the lowest it could be on average, over 60° round {100f * h.ElbowOut / h.Steps:0.0}% of the time, above its shoulder {100f * h.ElbowUp / h.Steps:0.0}%; the hand turned {h.Wrist / h.Steps:0}° on the forearm on average, over 30° {100f * h.WristOver / h.Steps:0.0}%; the board turned {h.InFist / h.Steps:0}° in the fist on average, over 20° {100f * h.InFistOver / h.Steps:0.0}%, at most {h.InFistMost:0}°");
        }

        {
            var at = units.Where(u => u.Clip != null).SelectMany(u => u.Clip.ShieldReadAt).OrderBy(x => x).ToList();
            var before = units.Where(u => u.Clip != null).SelectMany(u => u.Clip.ShieldReadBefore).OrderBy(x => x).ToList();
            if (at.Count > 0)
            {
                sb.AppendLine($"      blows a shield moved to meet: {at.Count}; it first moved {100f * at[at.Count / 2]:0}% of the way through the blow at the middle (the earliest {100f * at[0]:0}%, a tenth before {100f * at[at.Count / 10]:0}%), {before[before.Count / 2]:0.00} s before its contact at the middle (the longest {before[before.Count - 1]:0.00} s)");
            }
        }

        if (units.Any(u => u.Clip != null && u.Clip.ShieldKeptOut > 0))
        {
            sb.AppendLine($"      a shield's board moved off its own head or trunk as it was posed: {units.Sum(u => u.Clip != null ? u.Clip.ShieldKeptOut : 0)} times; still over 1 cm in after that: {units.Sum(u => u.Clip != null ? u.Clip.ShieldStillIn : 0)} steps, the deepest {100f * units.Max(u => u.Clip != null ? u.Clip.ShieldStillInMost : 0f):0.0} cm");
        }

        foreach (var s in shields.OrderBy(s => s.Key))
        {
            sb.AppendLine($"      {s.Key}: its shield or shield arm over 2 cm into its own head {100f * s.Value.head / s.Value.steps:0.0}% of the time (deepest {s.Value.headDeepest * 100f:0} cm), into its own trunk {100f * s.Value.trunk / s.Value.steps:0.0}% (deepest {s.Value.trunkDeepest * 100f:0} cm)");
        }

        return sb.ToString();
    }
}
